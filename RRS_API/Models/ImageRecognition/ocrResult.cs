using System;
using System.Collections.Generic;
using IronOcr;
using System.Drawing;
using System.Threading;
using RRS_API.Models;
using System.Threading.Tasks;
//using log4net;

namespace Server
{
    /*
     * This class responsible for ocr proccessing
     */

    class ocrResult
    {
        private AdvancedOcr ocr; //ocr object
        private List<Receipt> receipts;
        //private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ocrResult()
        {
            IronOcr.License.LicenseKey = "IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020";
            bool res = IronOcr.License.IsValidLicense("IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020");
            this.ocr = new AdvancedOcr()
            {
                Language = IronOcr.Languages.Hebrew.OcrLanguagePack,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceResolution = true,
                EnhanceContrast = true,
                CleanBackgroundNoise = true,
                ColorDepth = 15,
                RotateAndStraighten = true,
                DetectWhiteTextOnDarkBackgrounds = true,
                ReadBarCodes = false,
                Strategy = AdvancedOcr.OcrStrategy.Advanced,
                InputImageType = AdvancedOcr.InputTypes.Snippet
            };
            this.receipts = new List<Receipt>();
        }

        /*
         * This method responsible for iterate over all the receipts and send to initReceiptsList method
         * imgList - A Dictionary with image names and the image itself
         * using threads
         */
        public List<Receipt> fromImagesToText(Dictionary<string, Image> imgList)
        {
            List<Thread> threadsPool = new List<Thread>();
            foreach (KeyValuePair<string, Image> pair in imgList)
            {
                initReceiptsList(pair.Key, pair.Value);

                /*
                //run each convertion in seperate thread
                Thread t = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    initReceiptsList(pair.Key, pair.Value);
                });
                threadsPool.Add(t);
                t.Start();
                */
            }

            /*
            foreach (Thread t in threadsPool)
            {
                t.Join();
            }
            */

            return this.receipts;
        }

        /*
         * This method responsible for calling detectWords methond with all the three modes
         */
        private void initReceiptsList(String imgName, Image img)
        {
            ImageProccessing imageProccessing = new ImageProccessing(img, new Size((int)(img.Width), (int)(img.Height)));
            Bitmap imgInNewResolution = imageProccessing.getImageInNewResolution();

            OcrResult ocrResults;

            //try 3 different modes to get best results
            //grayScaleV1 - mode 1
            ocrResults = ocr.Read(imgInNewResolution);
            if (ocrResults.Pages.Count > 0)
            {
                Receipt receipt = new Receipt(ocrResults.Pages[0].Width, ocrResults.Pages[0].Height, imgName, imgInNewResolution, img);//create receipt object with sizes and name
                detectWords(ocrResults, imgInNewResolution, receipt);

                var mode1 = new Task(() =>
                {
                    ocrResults = ocr.Read(imageProccessing.getMode1());
                    detectWords(ocrResults, imgInNewResolution, receipt);
                });

                var mode2 = new Task(() =>
                {
                    ocrResults = ocr.Read(imageProccessing.getMode2());
                    detectWords(ocrResults, imgInNewResolution, receipt);
                });

                var mode3 = new Task(() =>
                {
                    ocrResults = ocr.Read(imageProccessing.getMode3());
                    detectWords(ocrResults, imgInNewResolution, receipt);
                });

                var mode4 = new Task(() =>
                {
                    ocrResults = ocr.Read(imageProccessing.getMode4());
                    detectWords(ocrResults, imgInNewResolution, receipt);
                });

                mode1.Start();
                mode2.Start();
                mode3.Start();
                mode4.Start();

                mode1.Wait();
                mode2.Wait();
                mode3.Wait();
                mode4.Wait();

                //add receipt after trying all m modes
                this.receipts.Add(receipt);
            }
        }

        /*
         * This function is the most important - detecting all words using ocr engine
         */
        private void detectWords(OcrResult ocrResults, Bitmap imgInNewResolution, Receipt receipt)
        {
            //page object
            foreach (var page in ocrResults.Pages)
            {
                //page -> paragraphs
                foreach (OcrResult.OcrParagraph ocrParagraph in page.Paragraphs)
                {
                    //paragraph -> lines
                    foreach (var line in ocrParagraph.Lines)
                    {
                        receipt.addRow(line.Text);

                        //line -> words
                        foreach (var word in line.Words)
                        {
                            //becuase we run 3 modes - we need to check if word exists from previous round
                            if (!receipt.getWordsList().ContainsKey(word.Text))
                            {
                                receipt.addWord(new ocrWord(word.X, word.Y, word.Width, word.Height, word.Text)); //add word to receipt object
                            }
                        }
                    }
                }
            }
        }
    }
}
