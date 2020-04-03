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
using System.Text.RegularExpressions;

namespace Server
{
    /*
     * This class responsible for ocr proccessing
     */

    class OcrProcessing
    {
        private AdvancedOcr ocr; //ocr object
        private List<Receipt> receipts;
        private static Mutex mutex = new Mutex();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public OcrProcessing()
        {
            IronOcr.License.LicenseKey = "IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020";
            IronOcrInstallation.InstallationPath = System.Web.HttpContext.Current.Server.MapPath("~/RequiredDLL/");
            this.ocr = new AdvancedOcr()
            {
                Language = IronOcr.Languages.Hebrew.OcrLanguagePack,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceResolution = true,
                EnhanceContrast = true,
                CleanBackgroundNoise = true,
                ColorDepth = 15,
                RotateAndStraighten = true,
                DetectWhiteTextOnDarkBackgrounds = false,
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
        public List<Receipt> FromImagesToText(Dictionary<string, Image> imgList)
        {
            _logger.Debug($"fromImagesToText started with list of size {imgList.Keys.Count}");
            //List<Thread> threadsPool = new List<Thread>();
            foreach (KeyValuePair<string, Image> pair in imgList)
            {
                InitReceiptsList(pair.Key, pair.Value);
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

            return this.receipts;

            //threads

            /*
            foreach (Thread t in threadsPool)
            {
                t.Join();
            }
            */
        }


        /*
         * This method responsible for calling detectWords methond with all the three modes
         */
        private void InitReceiptsList(string imgName, Image img)
        {
            _logger.Debug($"Initializing Receipt list imgName: {imgName}");
            PreProcessing preProccessing = new PreProcessing(img);
            Bitmap imgInNewResolution = preProccessing.GetImageInNewResolution();

            OcrResult ocrResults = null;
            try
            {
                _logger.Debug("Ocr Processing started");
                _logger.Debug($"OcrRead imgName: {imgName} - Mode1");
                try
                {
                    ocrResults = ocr.Read(preProccessing.GetMode1());
                    _logger.Info($"OcrRead Result: {ocrResults.Pages.Count}");
                }
                catch (Exception e)
                {
                    _logger.Error($"Read", e);
                }
                if (ocrResults.Pages.Count > 0)
                {
                    Receipt receipt = new Receipt(ocrResults.Pages[0].Width, ocrResults.Pages[0].Height, imgName, imgInNewResolution, ConvertTo24bpp(img));//create receipt object with sizes and name
                    DetectWords(ocrResults, receipt);
                    var newResolution = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - New Resolution");
                            DetectWords(ocr.Read(imgInNewResolution), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - New Resolution", e);
                        }
                    });

                    var mode2 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode2");
                            DetectWords(ocr.Read(preProccessing.GetMode2()), receipt);
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
                            DetectWords(ocr.Read(preProccessing.GetMode3()), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode3", e);
                        }
                    });

                    var mode4 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode4");
                            DetectWords(ocr.Read(preProccessing.GetMode4()), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode4", e);
                        }
                    });

                    newResolution.Start();
                    mode2.Start();
                    mode3.Start();
                    mode4.Start();

                    newResolution.Wait();
                    mode2.Wait();
                    mode3.Wait();
                    mode4.Wait();

                    //add receipt after trying all modes
                    _logger.Debug($"Adding receipt {receipt.GetName()} to list of ready receits");
                    this.receipts.Add(receipt);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error OcrRead imgName: {imgName} - Mode1", e);
            }
        }

        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height,
                          System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        /*
         * This function is the most important - detecting all words using ocr engine
         */
        private void DetectWords(OcrResult ocrResults, Receipt receipt)
        {
            _logger.Debug($"Detecting words for receipt: {receipt.GetName()}");
            //page object
            mutex.WaitOne();
            foreach (var page in ocrResults.Pages)
            {
                //page -> paragraphs
                foreach (OcrResult.OcrParagraph ocrParagraph in page.Paragraphs)
                {
                    //paragraph -> lines
                    foreach (var line in ocrParagraph.Lines)
                    {
                        receipt.AddRow(line.Text);

                        //line -> words
                        foreach (var word in line.Words)
                        {
                            double num;
                            string wordContent = word.Text;
                            if (double.TryParse(wordContent, out num) && !wordContent.Contains(",") && !wordContent.StartsWith("0") )//if number
                            {
                                string id = num.ToString();

                                if (id.Length == 13 && id.StartsWith("7") && !id.StartsWith("729"))
                                {
                                    id = id.Substring(Math.Max(0, id.Length - 10));
                                }

                                if (!receipt.GetWordsList().ContainsKey(id))
                                {
                                    receipt.AddWord(new OcrWord(word.X, word.Y, word.Width, word.Height, id)); //add word to receipt object
                                }
                            }
                        }
                    }
                }
            }
            mutex.ReleaseMutex();
            _logger.Debug($"Succesful Detecting words for receipt: {receipt.GetName()}");
        }
    }
}
