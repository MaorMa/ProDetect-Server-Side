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
    /// <summary>
    /// This method responsible for running ocr processing.
    /// Running 6 OCR tasks in pararell.
    /// </summary>

    public class OcrProcessing
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

        /// <summary>
        /// This method responsible for iterate over all the receipts and send to initReceiptsList method.
        /// </summary>
        /// <param name="imgList"> A Dictionary with image names and the image itself </param>
        /// <returns></returns>
        public List<Receipt> FromImagesToText(Dictionary<string, Image> imgList)
        {
            _logger.Debug($"fromImagesToText started with list of size {imgList.Keys.Count}");
            //List<Thread> threadsPool = new List<Thread>();
            foreach (KeyValuePair<string, Image> pair in imgList)
            {
                try
                {
                    InitReceiptsList(pair.Key, pair.Value);
                }
                catch (Exception e)
                {
                    _logger.Error($"Error - InitReceiptsList", e);
                }
            }

            return this.receipts;
        }

        /*
         * This method responsible for calling detectWords method
         */
         /// <summary>
         /// This method running 6 ocr process in pararell to detect ids in the receipt.
         /// </summary>
         /// <param name="imgName"></param>
         /// <param name="img"></param>
        private void InitReceiptsList(string imgName, Image img)
        {
            _logger.Debug($"Initializing Receipt list imgName: {imgName}");
            PreProcessing preProccessing = new PreProcessing(img);
            OcrResult ocrResults = null;
            try
            {
                _logger.Debug("Ocr Processing started");
                _logger.Debug($"OcrRead imgName: {imgName} - Mode1");
                try
                {
                    ocrResults = ocr.Read(preProccessing.GetMode1());
                }
                catch (Exception e)
                {
                    _logger.Error($"Read", e);
                }
                if (ocrResults.Pages.Count > 0)
                {
                    Receipt receipt = new Receipt(ocrResults.Pages[0].Width, ocrResults.Pages[0].Height, imgName, preProccessing.GetImageInNewResolution(), ConvertTo24bpp(img));//create receipt object with sizes and name
                    DetectWords(ocrResults, receipt);
                    var mode2 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - New Resolution");
                            DetectWords(ocr.Read(preProccessing.GetMode2()), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - New Resolution", e);
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
                            var image = preProccessing.GetMode4();
                            DetectWords(ocr.Read(new Bitmap(image)), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode4", e);
                        }
                    });

                    var mode5 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode5");
                            var image = preProccessing.GetMode5();
                            DetectWords(ocr.Read(new Bitmap(image)), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode5", e);
                        }
                    });

                    var mode6 = new Task(() =>
                    {
                        try
                        {
                            _logger.Debug($"OcrRead imgName: {imgName} - Mode6");
                            var image = preProccessing.GetMode6();
                            DetectWords(ocr.Read(new Bitmap(image)), receipt);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Error - OcrRead imgName: {imgName} - Mode6", e);
                        }
                    });

                    
                    mode2.Start();
                    mode3.Start();
                    mode4.Start();
                    mode2.Wait();
                    mode3.Wait();
                    mode4.Wait();

                    mode5.Start();
                    mode6.Start();
                    mode5.Wait();
                    mode6.Wait();
                    
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

         /// <summary>
         /// This method detects words (ids) in ocr result with coordinates.
         /// </summary>
         /// <param name="ocrResults"></param>
         /// <param name="receipt"></param>
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
                            if (double.TryParse(wordContent, out num) && !wordContent.Contains(",") && !wordContent.StartsWith("0"))//if number
                            {
                                string id = num.ToString();

                                if (id.Length == 13 && (id.StartsWith("7") || id.StartsWith("129")) && !id.StartsWith("780") && !id.StartsWith("761") && !id.StartsWith("762") && !id.StartsWith("729"))
                                {
                                    id = id.Substring(Math.Max(0, id.Length - 10));
                                }

                                //not contains
                                if (!receipt.GetWordsList().ContainsKey(id))
                                {
                                    receipt.AddWord(new OcrWord(word.X, word.Y, word.Width, word.Height, id)); //add word to receipt object
                                }

                                //new
                                //contains
                                if (receipt.GetWordsList().ContainsKey(id))
                                {
                                    OcrWord result = receipt.GetWordsList()[id].Find(item => Math.Abs(item.getY() - word.Y) < 50);
                                    if (result == null)
                                    {
                                        receipt.GetWordsList()[id].Add(new OcrWord(word.X, word.Y, word.Width, word.Height, id));
                                    }
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
