using System;
using System.Collections.Generic;
using IronOcr;
using System.Drawing;
using System.Threading;
//using log4net;

namespace Server
{
    class ocrResult
    {
        //fields
        private AdvancedOcr ocr; //ocr object
        //private Dictionary<String, receipt> imgNameToReceipt;
        private List<receipt> receipts;
        //private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //C'tor
        public ocrResult()
        {
            IronOcr.License.LicenseKey = "IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020";
            bool res = IronOcr.License.IsValidLicense("IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020");
            this.ocr = new AdvancedOcr()
            {
                Language = IronOcr.Languages.Hebrew.OcrLanguagePack,
                ColorSpace = AdvancedOcr.OcrColorSpace.GrayScale,
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
            //this.imgNameToReceipt = new Dictionary<string, receipt>();
            this.receipts = new List<receipt>();
        }

        /*
         * This method responsible for iterate over all the receipts in a given directory need to be converted.
         * send each receipt to createTextFile function
         * imgList - A Dictionary with image names and the image itself
         */
        public List<receipt> fromImagesToText(Dictionary<string, Image> imgList)
        {
            List<Thread> threads = new List<Thread>();
            foreach (KeyValuePair<string, Image> pair in imgList)
            {
                //run each convertion in seperate thread
                Thread t = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    initReceiptsList(pair.Key, pair.Value);
                });
                threads.Add(t);
                t.Start();
            }

            //join
            foreach (Thread t in threads)
            {
                t.Join();
            }

            return this.receipts;
        }

        /*
         * This method responsible for converting each file to text 
         */
        private void initReceiptsList(String imgName, Image img)
        {
            OcrResult ocrResults = ocr.Read(img);
            OcrResult.OcrPage ocrPage = ocrResults.Pages[0];//only 1 page
            receipt receipt = new receipt(ocrPage.Width, ocrPage.Height, imgName, img);//create receipt object with sizes and name

            //iterating over all the paragraph
            foreach (OcrResult.OcrParagraph ocrParagraph in ocrPage.Paragraphs)
            {
                //split ocrResult text to lines and add the to receipt
                string[] allLines = ocrResults.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                receipt.addRows(allLines);
                //iterating over all the words in current paragraph
                foreach (OcrResult.OcrWord w in ocrParagraph.Words)
                {
                    ocrWord ocrWord = new ocrWord(w.X, w.Y, w.Text);
                    receipt.addWord(ocrWord); //add word to receipt object
                }
            }
            //this.imgNameToReceipt.Add(imgName, receipt);
            this.receipts.Add(receipt);
        }
    }
}
