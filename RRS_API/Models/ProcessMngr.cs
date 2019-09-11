using System;
using System.Collections.Generic;
using System.Drawing;
using ImageRecognition.Objects;
using OcrProject.Parser;
using RRS_API.Models;
using Server;
using System.IO;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.Linq;

namespace RRS_API.Controllers
{
    public class ProcessMngr
    {
        private AzureConnection AzureConnection = AzureConnection.getInstance();
        private ocrResult ocrResult = new ocrResult();
        private TextParser TextParser = new TextParser();
        private MarksDrawing marksDrawing = new MarksDrawing();
        private EmailSender EmailSender = new EmailSender();
        private string selectedFamilyID;
        private string MarkedImagesPath = System.Web.Hosting.HostingEnvironment.MapPath(@"\App_Data\Marked\");


        /*
         *  Handle single POST of photos 
         */
        public void processPhotos(string selectedFamilyID, string selectedMarket, Dictionary<string, Image> imgNameAndImg)
        {
            setSelectedFamilyID(selectedFamilyID);
            //before we start using ocr engine , we need to approve the image not aploaded before            
            List<Receipt> receipts = ocrResult.fromImagesToText(detectMultiplicty(imgNameAndImg));
            receipts = TextParser.getAllRecieptsData(receipts);
            getSidAndDesc(receipts, selectedMarket);
        }

        /*
         * This method check if the user uploaded a receipt that was uploaded in the past
         * if uploaded, we need to remove it from the dictionary
         */
        public Dictionary<string, Image> detectMultiplicty(Dictionary<string, Image> imgNameAndImg)
        {
            Dictionary<string, Image> toReturn = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Image> rec in imgNameAndImg)
            {
                string receiptID = rec.Key;
                Image image = rec.Value;

                if (!(AzureConnection.SelectQuery("SELECT * FROM FamilyUploads where FamilyID ='" + selectedFamilyID + "' AND ReceiptID='" + receiptID + "'").Count > 0))
                {
                    //means receipt not uploaded in the past - thus we add it
                    toReturn.Add(receiptID, image);
                }
            }
            return toReturn;
        }

        /*
         * 
         */
        public void getSidAndDesc(List<Receipt> receipts, string selectedMarket)
        {
            //iterate over all the receipts
            foreach (Receipt receipt in receipts)
            {
                receipt.setMarketID(selectedMarket);
                List<ocrWord> wordsToDraw = new List<ocrWord>();
                double averageX = 0, averageY = 0, numOfWords = 0;

                //iterate over all the item in receipt
                foreach (KeyValuePair<String, List<MetaData>> sid in receipt.getIdToMetadata())
                {
                    foreach (MetaData obj in sid.Value)
                    {
                        String originalSQL = "SELECT * FROM " + selectedMarket + " WHERE sID='" + sid.Key + "'";
                        String newSQL = "SELECT * FROM " + selectedMarket + " WHERE sID='";
                        String prefix = "729"; //need to make it generic, not just for shupersal

                        if (sid.Key.Length < 13)
                        {
                            String newSid = prefix + getZero(sid.Key) + sid.Key;
                            newSQL += newSid + "'";
                        }
                        else
                        {
                            newSQL += sid.Key + "'";
                        }

                        //First try execute originalSQL
                        List<String> productInfoFromDb = AzureConnection.SelectQuery(originalSQL);

                        //if no result, try execute newSQL 
                        if (productInfoFromDb.Count == 0)
                        {
                            productInfoFromDb = AzureConnection.SelectQuery(newSQL);
                        }

                        //if there are results for originalSQL/newSQL, update description and price
                        if (productInfoFromDb.Count > 0)
                        {
                            foreach (String s in productInfoFromDb)
                            {
                                if (s != null && !s.Equals(""))
                                {
                                    String[] descAndPrice = s.Split(',');
                                    String description = descAndPrice[1];
                                    String price = descAndPrice[2];
                                    obj.setDescription(description);
                                    obj.setPrice(price);
                                }
                            }

                            List<ocrWord> tmpWords = receipt.getWord(sid.Key);
                            foreach (ocrWord word in tmpWords)
                            {
                                numOfWords += 1;
                                averageX = ((averageX * (numOfWords - 1)) + word.getX() + word.getWidth()) / numOfWords;
                                averageY = ((averageY * (numOfWords - 1)) + word.getY()) / numOfWords;
                                obj.setYcoordinate(word.getY());
                                wordsToDraw.Add(word);
                            }
                        }
                    }
                }
                //draw marks on images -> save them -> insert detected products to db -> send email to researcher
                marksDrawing.draw(wordsToDraw, receipt, averageX, averageY);
                saveMarkedImage(receipt);
                saveDataToDB(receipt);
            }
            //EmailSender.sendEmail(receipts.Count, selectedFamilyID);

            //free
            ocrResult = null;
            TextParser = null;
            marksDrawing = null;
            EmailSender = null;
        }

        /*
         * 
         */
        private void saveDataToDB(Receipt receipt)
        {
            try
            {
                this.insertToFamiliyUploads(selectedFamilyID, receipt.getName(), receipt.getMarketID(), receipt.getOriginalImage(), 0);
                foreach (KeyValuePair<string, List<MetaData>> data in receipt.getIdToMetadata())
                {
                    string id = data.Key;
                    List<MetaData> values = data.Value;

                    if (values.ToArray()[0].getDescription() != "")
                    {
                        string desc = values.ToArray()[0].getDescription();
                        string price = values.ToArray()[0].getPrice();
                        string quantitiy = values.ToArray()[0].getQuantity();
                        double yCoordinate = values.ElementAt(0).getyCoordinate();
                        AzureConnection.insertReceiptData(receipt.getName(), id, desc, quantitiy, price, yCoordinate);
                    }
                }
            }
            catch (Exception) { }
        }

        /*
         * 
         */
        private void insertToFamiliyUploads(string selectedFamilyID, string imageName, string marketID, Image markedImage, int status)
        {
            //string familyPath = MarkedImagesPath + selectedFamilyID;
            //try to update FamilyUploads
            //get exception if receipt allready been uploaded
            var stream = ToStream(markedImage, ImageFormat.Jpeg);
            Image image = System.Drawing.Image.FromStream(stream);
            try
            {
                using (MemoryStream m = new MemoryStream())
                {
                    ImageCodecInfo jpgCodec = ImageCodecInfo.GetImageEncoders().Where(codec => codec.FormatID.Equals(ImageFormat.Jpeg.Guid)).FirstOrDefault();
                    if (jpgCodec != null)
                    {
                        EncoderParameters parameters = new EncoderParameters();
                        parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 24); //8, 16, 24, 32 (base on your format)
                        image.Save(stream, jpgCodec, parameters);
                    }
                    Bitmap b = new Bitmap(stream);
                    ImageConverter converter = new ImageConverter();
                    byte[] imageBytes = (byte[])converter.ConvertTo(b, typeof(byte[]));
                    string base64String = Convert.ToBase64String(imageBytes);
                    AzureConnection.updateFamilyUploads(selectedFamilyID, marketID, imageName, status);
                    //AzureConnection.updateFamilyUploads(selectedFamilyID, imageName, familyPath, status);
                }
            }
            catch (Exception) { }
        }

        /*
         * 
         */
        private string getZero(string sid)
        {
            String zeros = "";
            for (int i = 0; i < 10 - sid.Length; i++)
                zeros += "0";
            return zeros;
        }

        /*
         * 
         */
        private void setSelectedFamilyID(string selectedFamilyID)
        {
            this.selectedFamilyID = selectedFamilyID;
        }

        /*
         * 
         */
        public string GetTotalUploadsDetails()
        {
            List<string> results = AzureConnection.SelectQuery("SELECT * FROM FamilyUploads");
            List<FamilyUploads> toReturn = new List<FamilyUploads>();

            foreach (string record in results)
            {
                string familyID = record.Split(',')[0];
                if (!(checkIfFamilyExists(familyID, toReturn)))
                {
                    toReturn.Add(new FamilyUploads { familyID = familyID, count = 1 });

                }
            }

            return JsonConvert.SerializeObject(toReturn);
        }

        /*
         * 
         */
        private bool checkIfFamilyExists(string familyID, List<FamilyUploads> toReturn)
        {
            foreach (var element in toReturn)
            {
                if (element.familyID.Equals(familyID))
                {
                    element.count += 1;
                    return true;
                }
            }
            return false;
        }

        /*
         * return all recognized data 
         */
        public string GetAllRecognizedData()
        {
            string query = "SELECT fu.FamilyID, fu.ReceiptID, fu.MarketID, rd.ProductID, rd.Description, rd.Quantity,rd.Price, rd.YCoordinate FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID and fu.ReceiptStatus = 0";
            List<string> results = AzureConnection.SelectQuery(query);
            Dictionary<string, Dictionary<string, ReceiptToReturn>> allInfo = new Dictionary<string, Dictionary<string, ReceiptToReturn>>();
            foreach (string record in results)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string receiptID = recordSplit[1];
                string marketID = recordSplit[2];
                string productID = recordSplit[3];
                string description = recordSplit[4];
                string quantity = recordSplit[5];
                string price = recordSplit[6];
                double yCoordinate = Convert.ToDouble(recordSplit[7]);
                if (allInfo.ContainsKey(FamilyID))//Family ID exists
                {
                    if (allInfo[FamilyID].ContainsKey(receiptID))//Receipt exists for family ID
                        allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate);
                    else//Create Receipt for family ID
                    {
                        using (Image image = Image.FromFile(MarkedImagesPath + "/" + FamilyID + "/" + receiptID))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat);
                                byte[] imageBytes = m.ToArray();
                                //Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(imageBytes);
                                allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String));
                                allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate);
                            }
                        }
                    }
                }
                else//Family ID not exists
                {
                    using (Image image = Image.FromFile(MarkedImagesPath + "/" + FamilyID + "/" + receiptID))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat);
                            byte[] imageBytes = m.ToArray();
                            //Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            allInfo.Add(FamilyID, new Dictionary<string, ReceiptToReturn>());
                            allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String));
                            allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate);
                        }
                    }
                }
            }

            Dictionary<string, List<ReceiptToReturn>> toReturn = new Dictionary<string, List<ReceiptToReturn>>();
            foreach (KeyValuePair<string, Dictionary<string, ReceiptToReturn>> data in allInfo)//for all families
            {
                List<ReceiptToReturn> receiptsForFamily = new List<ReceiptToReturn>();
                foreach (KeyValuePair<string, ReceiptToReturn> receipt in data.Value)//for all receipts for family
                {
                    receiptsForFamily.Add(receipt.Value);
                }
                toReturn.Add(data.Key, receiptsForFamily);
            }

            List<KeyValuePair<string, List<ReceiptToReturn>>> sorted = toReturn.ToList();
            foreach (KeyValuePair<string, List<ReceiptToReturn>> keyValue in sorted)
            {
                foreach (ReceiptToReturn ReceiptToReturn in keyValue.Value)
                {
                    List<MetaData> sortedProducts = ReceiptToReturn.products.OrderBy(y => y.getyCoordinate()).ToList();
                    ReceiptToReturn.updateProducts(sortedProducts);
                }
            }
            return JsonConvert.SerializeObject(sorted);
        }

        //move to FilesHandler class
        private void saveMarkedImage(Receipt receipt)
        {
            Image markedImage = receipt.getOriginalImage();
            var stream = ToStream(markedImage, ImageFormat.Jpeg);
            Image image = System.Drawing.Image.FromStream(stream);
            ImageCodecInfo jpgCodec = ImageCodecInfo.GetImageEncoders().Where(codec => codec.FormatID.Equals(ImageFormat.Jpeg.Guid)).FirstOrDefault();
            if (jpgCodec != null)
            {
                EncoderParameters parameters = new EncoderParameters();
                parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 24); //8, 16, 24, 32 (base on your format)
                image.Save(stream, jpgCodec, parameters);
            }

            string path = MarkedImagesPath + "\\" + selectedFamilyID;
            System.IO.Directory.CreateDirectory(path);
            Bitmap bm = new Bitmap(stream);
            bm.Save(path + "\\" + receipt.getName());
        }

        //move to FilesHandler class
        private Stream ToStream(Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        /*
         * first we delete all products of given receipt
         * then we add all products of updated receipt
         */
        public void UpdateReceiptData(List<KeyValuePair<string, List<ReceiptToReturn>>> familyToReceipts)
        {
            try
            {
                // families - >  receipts -> products
                foreach (KeyValuePair<string, List<ReceiptToReturn>> keyValue in familyToReceipts)
                {
                    foreach (ReceiptToReturn ReceiptToReturn in keyValue.Value)
                    {
                        string receiptID = ReceiptToReturn.receiptID;
                        List<MetaData> sortedProducts = ReceiptToReturn.products.ToList();

                        //delete -> insert -> updateStatus
                        AzureConnection.deleteReceiptData(receiptID);
                        AzureConnection.updateStatus(receiptID);
                        foreach (MetaData product in sortedProducts)
                        {
                            string productID = product.getsID();
                            string productDescription = product.getDescription();
                            string productQuantity = product.getQuantity();
                            string productPrice = product.getPrice();
                            AzureConnection.insertReceiptData(receiptID, productID, productDescription, productQuantity, productPrice, 0);
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public List<string> GetProductInfo(string productID, string MarketID)
        {
            string query = "SELECT * FROM " + MarketID + " where sID='" + productID + "'";
            return AzureConnection.SelectQuery(query);
        }
    }
}