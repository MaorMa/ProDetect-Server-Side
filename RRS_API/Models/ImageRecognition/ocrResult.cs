using System;
using System.Collections.Generic;
using IronOcr;
using System.Drawing;
using System.Threading;
using RRS_API.Models;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using log4net;
using System.Reflection;
using System.Linq;

namespace Server
{
    /*
     * This class responsible for ocr proccessing
     */

    class ocrResult
    {
        private AdvancedOcr ocr; //ocr object
        private List<Receipt> receipts;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ocrResult()
        {
            _logger.Debug("ocrResult started");
            IronOcr.License.LicenseKey = "IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020";
            IronOcrInstallation.InstallationPath = System.Web.HttpContext.Current.Server.MapPath("~/RequiredDLL/");
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
            _logger.Debug($"fromImagesToText started with list of size {imgList.Keys.Count}");
            //List<Thread> threadsPool = new List<Thread>();
            foreach (KeyValuePair<string, Image> pair in imgList)
            {
                initReceiptsList(pair.Key, flip(pair.Value));

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
         * return hash of given image object
         */
        public string getHash(Image image)
        {
            using (var ms = new MemoryStream())
            {
                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                image.Save(ms, image.RawFormat);
                return BitConverter.ToString(sha1.ComputeHash(ms.ToArray())).Replace("-", "");
            }
        }

        /*
         * This method responsible for calling detectWords methond with all the three modes
         */
        private void initReceiptsList(String imgName, Image img)
        {
            _logger.Debug($"Initializing Receipt list imgName: {imgName}");
            ImageProccessing imageProccessing = new ImageProccessing(img, new Size((int)(img.Width), (int)(img.Height)));
            Bitmap imgInNewResolution = imageProccessing.getImageInNewResolution();

            OcrResult ocrResults = null;

            //try 4 different modes to get best results
            //grayScaleV1 - mode 1
            try
            {
                _logger.Debug($"OcrRead imgName: {imgName} - Mode1");
                try
                {
                    ocrResults = ocr.Read(imageProccessing.getMode1());
                    _logger.Info($"OcrRead Result: {ocrResults.Pages.Count}");
                }
                catch (Exception e)
                {
                    _logger.Error($"Read",e);
                }
                if (ocrResults.Pages.Count > 0)
                {
                    Receipt receipt = new Receipt(ocrResults.Pages[0].Width, ocrResults.Pages[0].Height, imgName, imgInNewResolution, img);//create receipt object with sizes and name
                    detectWords(ocrResults, receipt);
                    var NewResolution = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - New Resolution");
                            ocrResults = ocr.Read(imageProccessing.getImageInNewResolution());
                            detectWords(ocrResults, receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - New Resolution", e);
                        }
                    });

                    var mode1 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode1");
                            ocrResults = ocr.Read(imageProccessing.getMode1());
                            detectWords(ocrResults, receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode1", e);
                        }
                    });

                    var mode2 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode2");
                            ocrResults = ocr.Read(imageProccessing.getMode2());
                            detectWords(ocrResults, receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode2", e);
                        }
                    });

                    var mode3 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode3");
                            ocrResults = ocr.Read(imageProccessing.getMode3());
                            detectWords(ocrResults, receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode3", e);
                        }
                    });

                    NewResolution.Start();
                    mode1.Start();
                    mode2.Start();
                    mode3.Start();

                    NewResolution.Wait();
                    mode1.Wait();
                    mode2.Wait();
                    mode3.Wait();

                    //add receipt after trying all m modes
                    _logger.Debug($"Adding receipt {receipt.getName()} to list of ready receits");
                    this.receipts.Add(receipt);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error OcrRead imgName: {imgName} - Mode1", e);
            }
        }

        private Image flip(Image image)
        {
            const int exifOrientationID = 0x112; //274
            if (!image.PropertyIdList.Contains(exifOrientationID))
            {
                return image;
            }
            //change rotation manually
            else
            {
                _logger.Debug($"Receipt upload from Phone");
                var prop = image.GetPropertyItem(exifOrientationID);
                int val = BitConverter.ToUInt16(prop.Value, 0);
                var rot = RotateFlipType.RotateNoneFlipNone;

                if (val == 3 || val == 4)
                    rot = RotateFlipType.Rotate180FlipNone;
                else if (val == 5 || val == 6)
                    rot = RotateFlipType.Rotate90FlipNone;
                else if (val == 7 || val == 8)
                    rot = RotateFlipType.Rotate270FlipNone;

                if (val == 2 || val == 4 || val == 5 || val == 7)
                    rot |= RotateFlipType.RotateNoneFlipX;

                if (rot != RotateFlipType.RotateNoneFlipNone)
                    image.RotateFlip(rot);
            }
            return image;
        }

        /*
         * This function is the most important - detecting all words using ocr engine
         */
        private void detectWords(OcrResult ocrResults, Receipt receipt)
        {
            _logger.Debug($"Detecting words for receipt: {receipt.getName()}");
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
            _logger.Debug($"Succesful Detecting words for receipt: {receipt.getName()}");
        }
    }
}
