using System;
using System.Collections.Generic;
using System.Drawing;
using ImageRecognition.Objects;
using OcrProject.Parser;
using RRS_API.Models;
using Server;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using RRS_API.Models.ImageRecognition;
using RRS_API.Models.Mangagers;
using System.Text;
using System.Security.Cryptography;
using log4net;
using System.Reflection;
using RRS_API.Models.Objects;
using RRS_API.Models.StringSimilarityAlgorithms;

namespace RRS_API.Controllers
{
    public class ReceiptMngr : AMngr
    {
        #region fields
        private ocrResult ocrResult = new ocrResult();
        private TextParser TextParser = new TextParser();
        private MarksDrawing marksDrawing = new MarksDrawing();
        private EmailSender EmailSender = new EmailSender();
        private MarkedImageSaver MarkedImageSaver = new MarkedImageSaver();
        private DateTime UploadTime;
        private string selectedFamilyID;
        private JaroWinklerDistance jwd = new JaroWinklerDistance();

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region public Methods
        /*
         *  Handle single POST of photos 
         */
        public void processPhotos(string selectedFamilyID, string selectedMarket, Dictionary<string, Image> imgNameAndImg)
        {
            _logger.Debug($"ProcessPhotos started, selectedFamilyID: {selectedFamilyID}, selectedMarket: {selectedMarket}");
            UploadTime = DateTime.Now;
            setSelectedFamilyID(selectedFamilyID);
            //before we start using ocr engine , we need to approve the receipt not uploaded before            
            imgNameAndImg = detectMultiplicty(imgNameAndImg);
            if (imgNameAndImg.Count != 0)
            {
                //first set status to "-1" = in progress
                // pair.key - receiptID - hash of image
                foreach (KeyValuePair<string, Image> pair in imgNameAndImg)
                {
                    insertToFamiliyUploads(selectedFamilyID, pair.Key, selectedMarket, -1, UploadTime.ToString());
                }
                //call ocr proccessing
                List<Receipt> receipts = ocrResult.fromImagesToText(imgNameAndImg);
                //parsing ocr result
                receipts = TextParser.getAllRecieptsData(receipts);
                compeleteInfoAndMarks(receipts, selectedMarket);
            }
        }

        /*
         * return hash (sha1) for a given image
         * same images get the same hash 
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
        * return all families from db
        */
        public string GetAllFamilies()
        {
            string queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads";
            List<string> familiesList = AzureConnection.SelectQuery(queryStatusWorking);
            return JsonConvert.SerializeObject(familiesList);
        }

        /*
        * return all specific families data from db
        */
        public string GetAllFamilyData(string familyID)
        {
            string queryStatusWorking = "SELECT * FROM FamilyUploads WHERE ReceiptStatus = -1 AND FamilyID = '" + familyID + "'";
            List<string> resultsStatusWorking = AzureConnection.SelectQuery(queryStatusWorking);
            string query = "SELECT fu.FamilyID, fu.ReceiptID, fu.MarketID, fu.UploadTime, fu.ReceiptStatus, ISNULL(rd.ProductID,'') as ProductID , ISNULL(rd.Description,'') as Description, ISNULL(rd.Quantity,'') as Quantity, ISNULL(rd.Price,'') as Price, ISNULL(rd.Ycoordinate,'') as Ycoordinate, ISNULL(rd.validProduct,'') as validProduct FROM FamilyUploads as fu FULL JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID WHERE fu.ReceiptStatus = 0 AND FamilyID = '" + familyID + "'";
            List<string> results = AzureConnection.SelectQuery(query);
            Dictionary<string, ReceiptToReturn> allInfo = new Dictionary<string, ReceiptToReturn>();
            string MarkedImagesPath = MarkedImageSaver.getMarkedImagesPath();
            foreach (string record in results)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string receiptID = recordSplit[1];
                string marketID = recordSplit[2];
                string uploadTime = recordSplit[3];
                string receiptStatus = recordSplit[4];
                string productID = recordSplit[5];
                List<string> OptionalNames = AzureConnection.SelectQuery("SELECT SID, OptionalName FROM OptionalProducts WHERE ProductID ='" + productID + "'");
                List<ResearchProduct> rp = createResarchListForProductID(OptionalNames);
                string description = recordSplit[6];
                string quantity = recordSplit[7];
                string price = recordSplit[8];
                double yCoordinate;
                bool validProduct = recordSplit[10].Equals("True");
                if (recordSplit[9] != "")
                {
                    yCoordinate = Convert.ToDouble(recordSplit[9]);
                }
                else
                {
                    yCoordinate = 0;
                }
                if (allInfo.ContainsKey(receiptID))//Receipt exists for family ID
                    allInfo[receiptID].addProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
                else//Create Receipt for family ID
                {
                    using (Image image = Image.FromFile(MarkedImagesPath + "/" + FamilyID + "/" + receiptID))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat); //image to memory stream
                                                            //Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(m.ToArray());
                            allInfo.Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String, receiptStatus, uploadTime));
                            allInfo[receiptID].addProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
                        }
                    }
                }
            }
            foreach (string record in resultsStatusWorking)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string marketID = recordSplit[1];
                string receiptID = recordSplit[2];
                string receiptStatus = recordSplit[3];
                string uploadTime = recordSplit[4];
                if (allInfo.ContainsKey(FamilyID))//Family ID exists
                {
                    allInfo.Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", receiptStatus, uploadTime));
                }
                else
                {
                    allInfo.Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", receiptStatus, uploadTime));
                }
            }
            List<ReceiptToReturn> sorted = allInfo.Values.ToList();
            foreach (ReceiptToReturn rtr in sorted)
            {
                    if (!rtr.status.Equals("-1"))
                    {
                        List<MetaData> sortedProducts = rtr.products.OrderBy(y => y.getyCoordinate()).ToList();
                        rtr.updateProducts(sortedProducts);
                }
            }
            return JsonConvert.SerializeObject(sorted);
        }

        /*
        * return all recognized data from db
        */
        public string GetAllRecognizedData()
        {
            string queryStatusWorking = "SELECT * FROM FamilyUploads WHERE ReceiptStatus = -1";
            List<string> resultsStatusWorking = AzureConnection.SelectQuery(queryStatusWorking);
            string query = "SELECT fu.FamilyID, fu.ReceiptID, fu.MarketID, fu.UploadTime, fu.ReceiptStatus, ISNULL(rd.ProductID,'') as ProductID , ISNULL(rd.Description,'') as Description, ISNULL(rd.Quantity,'') as Quantity, ISNULL(rd.Price,'') as Price, ISNULL(rd.Ycoordinate,'') as Ycoordinate, ISNULL(rd.validProduct,'') as validProduct FROM FamilyUploads as fu FULL JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID WHERE fu.ReceiptStatus = 0";
            List<string> results = AzureConnection.SelectQuery(query);
            Dictionary<string, Dictionary<string, ReceiptToReturn>> allInfo = new Dictionary<string, Dictionary<string, ReceiptToReturn>>();
            string MarkedImagesPath = MarkedImageSaver.getMarkedImagesPath();
            foreach (string record in results)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string receiptID = recordSplit[1];
                string marketID = recordSplit[2];
                string uploadTime = recordSplit[3];
                string receiptStatus = recordSplit[4];
                string productID = recordSplit[5];
                List<string> OptionalNames = AzureConnection.SelectQuery("SELECT SID, OptionalName FROM OptionalProducts WHERE ProductID ='" + productID + "'");
                List<ResearchProduct> rp = createResarchListForProductID(OptionalNames);
                string description = recordSplit[6];
                string quantity = recordSplit[7];
                string price = recordSplit[8];
                double yCoordinate;
                bool validProduct = recordSplit[10].Equals("True");
                if (recordSplit[9] != "")
                {
                    yCoordinate = Convert.ToDouble(recordSplit[9]);
                }
                else
                {
                    yCoordinate = 0;
                }
                if (allInfo.ContainsKey(FamilyID))//Family ID exists
                {
                    if (allInfo[FamilyID].ContainsKey(receiptID))//Receipt exists for family ID
                        allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
                    else//Create Receipt for family ID
                    {
                        using (Image image = Image.FromFile(MarkedImagesPath + "/" + FamilyID + "/" + receiptID))
                        {
                            using (MemoryStream m = new MemoryStream())
                            {
                                image.Save(m, image.RawFormat); //image to memory stream
                                //Convert byte[] to Base64 String
                                string base64String = Convert.ToBase64String(m.ToArray());
                                allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String, receiptStatus, uploadTime));
                                allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
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
                            allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String, receiptStatus, uploadTime));
                            allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
                        }
                    }
                }
            }

            foreach (string record in resultsStatusWorking)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string marketID = recordSplit[1];
                string receiptID = recordSplit[2];
                string receiptStatus = recordSplit[3];
                string uploadTime = recordSplit[4];
                if (allInfo.ContainsKey(FamilyID))//Family ID exists
                {
                    allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", receiptStatus, uploadTime));
                }
                else
                {
                    allInfo.Add(FamilyID, new Dictionary<string, ReceiptToReturn>());
                    allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", receiptStatus, uploadTime));
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
                    if (!ReceiptToReturn.status.Equals("-1"))
                    {
                        List<MetaData> sortedProducts = ReceiptToReturn.products.OrderBy(y => y.getyCoordinate()).ToList();
                        ReceiptToReturn.updateProducts(sortedProducts);
                    }
                }
            }
            return JsonConvert.SerializeObject(sorted);
        }

        /*
         * Return list of researchProduct for productId - for approval
         */
        private List<ResearchProduct> createResarchListForProductID(List<string> optionalName)
        {
            List<ResearchProduct> researchProduct = new List<ResearchProduct>();
            foreach (string s in optionalName)
            {
                string[] sidAndName = s.Split(',');
                researchProduct.Add(new ResearchProduct(sidAndName[0], sidAndName[1]));
            }
            return researchProduct;
        }

        /*
         * first we delete all products of given receipt
         * then we add all products of updated receipt
         */
        public void UpdateReceiptData(string familyID, ReceiptToReturn receiptToUpdate)
        {
            try
            {
                string receiptID = receiptToUpdate.receiptID;
                List<MetaData> sortedProducts = receiptToUpdate.products.ToList();

                //delete -> insert -> updateStatus
                AzureConnection.deleteReceiptData(receiptID);
                foreach (MetaData product in sortedProducts)
                {
                    string productID = product.getsID();
                    string productDescription = product.getDescription();
                    string productQuantity = product.getQuantity();
                    string productPrice = product.getPrice();
                    AzureConnection.insertReceiptData(familyID, receiptID, productID, productDescription, productQuantity, productPrice, 0, true);
                }
                AzureConnection.updateStatus(familyID, receiptID, "1");
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public string GetAllApprovedData()
        {
            Dictionary<string, Dictionary<string, ReceiptToReturn>> allInfo = new Dictionary<string, Dictionary<string, ReceiptToReturn>>();
            string queryStatusApproved = "SELECT fu.FamilyID, fu.ReceiptID, fu.MarketID, rd.ProductID, rd.Description, rd.Quantity,rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1";
            List<string> resultsStatusApproved = AzureConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string receiptID = recordSplit[1];
                string marketID = recordSplit[2];
                string productID = recordSplit[3];
                List<string> nutrients = AzureConnection.SelectQuery("select * from Nutrients where Nutrients.SID = (select SID from OptionalProducts AS OP WHERE OP.ProductID ='" + productID + "'");
                string description = recordSplit[4];
                string quantity = recordSplit[5];
                string price = recordSplit[6];
                if (allInfo.ContainsKey(FamilyID))//Family ID exists
                {
                    if (allInfo[FamilyID].ContainsKey(receiptID))//Receipt exists for family ID
                        allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, 0, true, nutrients);
                    else//Create Receipt for family ID
                    {
                        allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", "1", ""));
                        allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, 0, true, nutrients);
                    }
                }
                else//Family ID not exists
                {
                    allInfo.Add(FamilyID, new Dictionary<string, ReceiptToReturn>());
                    allInfo[FamilyID].Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", "1", ""));
                    allInfo[FamilyID][receiptID].addProduct(productID, description, quantity, price, 0, true, nutrients);
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
            return JsonConvert.SerializeObject(toReturn.ToList());
        }


        /*
         * This method return product info 
         * info: Description, price
         */
        public List<string> GetProductInfo(string productID, string MarketID)
        {
            String originalSQL = "SELECT * FROM " + MarketID + " WHERE sID='" + productID + "'";
            String newSQL = "SELECT * FROM " + MarketID + " WHERE sID='";
            String prefix = "729"; //need to make it generic, not just for shupersal

            if (productID.Length < 13)
            {
                String newSid = prefix + getZero(productID) + productID;
                newSQL += newSid + "'";
            }
            else
            {
                newSQL += productID + "'";
            }

            //First try execute originalSQL
            List<String> productInfoFromDb = AzureConnection.SelectQuery(originalSQL);

            //if no result, try execute newSQL 
            if (productInfoFromDb.Count == 0)
            {
                productInfoFromDb = AzureConnection.SelectQuery(newSQL);
            }

            return productInfoFromDb;
        }

        /*
         *  
         */
        public string GetTotalUploadsDetails()
        {
            List<string> results = AzureConnection.SelectQuery("SELECT * FROM FamilyUploads WHERE NOT ReceiptStatus = 1");
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
        #endregion

        #region private Methods
        /*
         * This method check if the user uploaded a receipt that was uploaded in the past
         * if uploaded, we need to remove it from the dictionary
         */
        private Dictionary<string, Image> detectMultiplicty(Dictionary<string, Image> imgNameAndImg)
        {
            Dictionary<string, Image> toReturn = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Image> rec in imgNameAndImg)
            {
                string receiptID = getHash(rec.Value);
                Image image = rec.Value;

                if (!(AzureConnection.SelectQuery("SELECT * FROM FamilyUploads where FamilyID ='" + selectedFamilyID + "' AND ReceiptID='" + receiptID + "' AND NOT ReceiptStatus = -1").Count > 0))
                {
                    //means receipt not uploaded in the past - thus we add it
                    toReturn.Add(receiptID, image);
                }
            }
            return toReturn;
        }

        /*
         * update info for each product
         * then mark and save
         */
        private void compeleteInfoAndMarks(List<Receipt> receipts, string selectedMarket)
        {
            //iterate over all the receipts
            foreach (Receipt receipt in receipts)
            {
                receipt.setMarketID(selectedMarket);
                List<ocrWord> wordsToDraw = new List<ocrWord>();
                List<ocrWord> tmpWords;
                double averageX = 0, averageY = 0, numOfWords = 0, normalizedX, normalizedWidth;

                //iterate over all the item in receipt
                foreach (KeyValuePair<String, List<MetaData>> sid in receipt.getIdToMetadata())
                {
                    foreach (MetaData obj in sid.Value)
                    {
                        List<string> productInfoFromDb = GetProductInfo(sid.Key, selectedMarket);
                        //if there are results for originalSQL/newSQL, update description and price
                        if (productInfoFromDb.Count > 0)
                        {
                            foreach (string s in productInfoFromDb)
                            {
                                if (s != null && !s.Equals(""))
                                {
                                    string[] descAndPrice = s.Split(',');
                                    string description = descAndPrice[1];
                                    string price = descAndPrice[2];
                                    obj.setDescription(description);
                                    obj.setPrice(price);
                                    obj.setOptionalProducts(jwd.getTopFiveSimilarProducts(description));
                                }
                            }

                            tmpWords = receipt.getWord(sid.Key);
                            foreach (ocrWord word in tmpWords)
                            {
                                numOfWords += 1;
                                normalizedX = (int)((word.getX() / receipt.getWidth()) * receipt.getOriginalImage().Width);
                                normalizedWidth = (int)((word.getWidth() / receipt.getWidth()) * receipt.getOriginalImage().Width);
                                averageX = ((averageX * (numOfWords - 1)) + normalizedX + normalizedWidth) / numOfWords;
                                averageY = ((averageY * (numOfWords - 1)) + word.getY()) / numOfWords;
                                obj.setYcoordinate(word.getY());
                                wordsToDraw.Add(word);
                            }
                        }
                    }
                }
                receipt.setAverageCoordinates(averageX, averageY);
                //draw marks on images -> save them -> insert detected products to db -> send email to researcher
                setvalidProduct(receipt);
                marksDrawing.draw(wordsToDraw, receipt);
                MarkedImageSaver.saveMarkedImage(receipt, selectedFamilyID);
                saveDataToDB(selectedFamilyID, receipt);

                wordsToDraw = null;
                tmpWords = null;
            }
            //EmailSender.sendEmail(receipts.Count, selectedFamilyID);
            ocrResult = null;
            TextParser = null;
            marksDrawing = null;
            EmailSender = null;
            receipts = null;
            MarkedImageSaver = null;
            AzureConnection = null;
        }

        /*
         * if word is False Positive - IsFP = true, else IsFP = False
         */
        private void setvalidProduct(Receipt receipt)
        {
            double normalizedX, normalizedWidth;
            bool xRuleDeviation;
            double averageX = receipt.getXAverage();

            foreach (KeyValuePair<String, List<MetaData>> sid in receipt.getIdToMetadata())
            {
                foreach (MetaData obj in sid.Value)
                {
                    if (obj.description != "")
                    {
                        List<ocrWord> tmpWords = receipt.getWord(sid.Key);
                        foreach (ocrWord word in tmpWords)
                        {
                            normalizedX = (int)((word.getX() / receipt.getWidth()) * receipt.getOriginalImage().Width);
                            normalizedWidth = (int)((word.getWidth() / receipt.getWidth()) * receipt.getOriginalImage().Width + 14);
                            xRuleDeviation = Math.Abs(normalizedX + normalizedWidth - (receipt.getXAverage())) < 0.08 * receipt.getWidth();
                            obj.setvalidProduct(xRuleDeviation);
                        }
                    }

                }
            }
        }
        /*
         * 
         */
        private void saveDataToDB(string FamilyID, Receipt receipt)
        {
            try
            {
                AzureConnection.updateStatus(FamilyID, receipt.getName(), "0");
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
                        bool validProduct = values.ElementAt(0).getvalidProduct();
                        List<ResearchProduct> optionalProducts = values.ElementAt(0).getOptionalProducts();
                        AzureConnection.insertReceiptData(FamilyID, receipt.getName(), id, desc, quantitiy, price, yCoordinate, validProduct);
                        //foreach (ResearchProduct rp in optionalProducts)
                        //{
                        //AzureConnection.insertOptionalProducts(id, rp.SID, rp.Name);
                        AzureConnection.insertOptionalProducts(id, optionalProducts);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error: Exceprion {e}");
                throw e;
            }
        }

        /*
         * 
         */
        private void insertToFamiliyUploads(string selectedFamilyID, string imageName, string marketID, int status, string UploadTime)
        {
            //try to update FamilyUploads
            //get exception if receipt allready been uploaded
            try
            {
                AzureConnection.updateFamilyUploads(selectedFamilyID, marketID, imageName, status, UploadTime);
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
    }
    #endregion
}