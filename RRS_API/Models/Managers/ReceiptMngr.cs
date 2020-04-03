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
using RRS_API.Models.Parsers;

namespace RRS_API.Controllers
{
    //This class responsible for the main scenario
    public class ReceiptMngr
    {
        #region fields
        private OcrProcessing ocrProcessing = new OcrProcessing();
        private OcrTextParser ocrTextParser = new OcrTextParser();
        private MarksDrawing marksDrawing = new MarksDrawing();
        private MarkedImageSaver MarkedImageSaver = new MarkedImageSaver();
        private JaroWinklerDistance jwd = new JaroWinklerDistance();
        private DBConnection DBConnection = DBConnection.GetInstance();
        NutrientMngr nm = new NutrientMngr();
        private DateTime uploadTime;
        private string selectedFamilyID;
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region public Methods
        // Handle single POST request of photos 
        public void ProcessReceipts(string selectedFamilyID, string selectedMarket, Dictionary<string, Image> imgNameAndImg)
        {
            _logger.Debug($"ProcessPhotos started, selectedFamilyID: {selectedFamilyID}, selectedMarket: {selectedMarket}");
            uploadTime = DateTime.Now;
            this.selectedFamilyID = selectedFamilyID;
            //before we start using ocr engine , we need to approve the receipt not uploaded before            
            imgNameAndImg = DetectReceiptMultiplicty(imgNameAndImg);
            if (imgNameAndImg.Count != 0)
            {
                //first set status to "-1" = in progress
                // pair.key - receiptID - hash of image
                foreach (KeyValuePair<string, Image> pair in imgNameAndImg)
                {
                    DBConnection.UpdateFamilyUploads(selectedFamilyID, selectedMarket, pair.Key, -1, uploadTime.ToString());
                }
                //call ocr proccessing
                List<Receipt> receipts = ocrProcessing.FromImagesToText(imgNameAndImg);
                //parsing ocr result
                receipts = ocrTextParser.GetAllRecieptsData(receipts);
                CompleteInfoAndMarks(receipts, selectedMarket);
            }
        }

        /*
        * Return all existing families from db
        * acc - view unapproved receipts
        * not acc - view approved receipts
        * if admin - can see all the families
        * if not admin - can see only his family
        */
        public string GetAllFamiliesByReceiptStatus(string accView,string username, bool isGlobalAdmin, bool isLocalAdmin)
        {
            List<string> familiesList = new List<string>();
            string queryStatusWorking;

            //global admin
            if (isGlobalAdmin)
            {
                if (accView.Equals("Acc"))
                    queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE NOT ReceiptStatus = 1";
                else
                    queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE ReceiptStatus = 1";
                familiesList = DBConnection.SelectQuery(queryStatusWorking);
            }

            //local admin
            else if (isLocalAdmin)
            {
                if (accView.Equals("Acc"))
                    queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE NOT ReceiptStatus = 1 AND FamilyID ='" + username + "'";
                else
                    queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE ReceiptStatus = 1 AND FamilyID ='" + username + "'";
                familiesList = DBConnection.SelectQuery(queryStatusWorking);
            }
            //regular user
            else
            {
                queryStatusWorking = "SELECT DISTINCT FamilyID FROM FamilyUploads WHERE ReceiptStatus = 1 AND FamilyID ='" + username + "'";
                familiesList = DBConnection.SelectQuery(queryStatusWorking);
                //familiesList.Add(username);
            }
            return JsonConvert.SerializeObject(familiesList);
        }

        /*
        * return all specific families data from db
        */
        public string GetAllNotApprovedFamilyData(string familyID)
        {
            string queryStatusWorking = "SELECT * FROM FamilyUploads WHERE ReceiptStatus = -1 AND FamilyID = '" + familyID + "'";
            List<string> resultsStatusWorking = DBConnection.SelectQuery(queryStatusWorking);
            string query = "SELECT fu.FamilyID, fu.ReceiptID, fu.MarketID, fu.UploadTime, fu.ReceiptStatus, ISNULL(rd.ProductID,'') as ProductID , ISNULL(rd.Description,'') as Description, ISNULL(rd.Quantity,'') as Quantity, ISNULL(rd.Price,'') as Price, ISNULL(rd.Ycoordinate,'') as Ycoordinate, ISNULL(rd.validProduct,'') as validProduct FROM FamilyUploads as fu FULL JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID WHERE fu.ReceiptStatus = 0 AND FamilyID = '" + familyID + "'";
            List<string> results = DBConnection.SelectQuery(query);
            Dictionary<string, ReceiptToReturn> allInfo = new Dictionary<string, ReceiptToReturn>();
            string MarkedImagesPath = MarkedImageSaver.GetMarkedImagesPath();
            foreach (string record in results)
            {
                string[] recordSplit = record.Split(',');
                string FamilyID = recordSplit[0];
                string receiptID = recordSplit[1];
                string marketID = recordSplit[2];
                string uploadTime = recordSplit[3];
                string receiptStatus = recordSplit[4];
                string productID = recordSplit[5];
                List<string> OptionalNames = DBConnection.SelectQuery("SELECT SID, OptionalName, Similarity FROM OptionalProducts WHERE MarketID ='" + marketID + "' AND ProductID ='" + productID + "'");
                List<ResearchProduct> rp = CreateResearchProductListForProduct(OptionalNames);
                rp.Sort((el1, el2) => Double.Parse(el2.similarity).CompareTo(Double.Parse(el1.similarity)));
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
                    allInfo[receiptID].AddProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
                else//Create Receipt for family ID
                {
                    using (Image image = Image.FromFile(MarkedImagesPath + "/" + FamilyID + "/" + receiptID))
                    {
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, image.RawFormat); //image to memory stream
                            string base64String = Convert.ToBase64String(m.ToArray());//Convert byte[] to Base64 String
                            allInfo.Add(receiptID, new ReceiptToReturn(receiptID, marketID, base64String, receiptStatus, uploadTime));
                            allInfo[receiptID].AddProduct(productID, description, quantity, price, yCoordinate, validProduct, rp);
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
                    rtr.UpdateProducts(sortedProducts);
                }
            }
            return JsonConvert.SerializeObject(sorted);
        }

        /*
         * First we delete all products of given receipt
         * Then we add all products of updated receipt
         */
        public void UpdateReceiptData(string familyID, ReceiptToReturn receiptToUpdate)
        {
            try
            {
                ProductDescriptionParser pdp = new ProductDescriptionParser();
                string receiptID = receiptToUpdate.receiptID;
                List<MetaData> sortedProducts = receiptToUpdate.products.ToList();

                //delete -> insert -> updateStatus
                DBConnection.DeleteReceiptData(receiptID);
                foreach (MetaData product in sortedProducts)
                {
                    string productID = product.getsID();
                    string productDescription = product.getDescription();
                    //Parse the desc to find quantity
                    double quantityInDesc;
                    string quantityFoundInDesc = pdp.GetQuantityFromDescription(productDescription);
                    string productQuantity = product.getQuantity();

                    //if not found quantity in description
                    if (quantityFoundInDesc.Equals("") || productQuantity.Contains('.'))
                    {
                        //if qunatitny is float (0.35 for example) than we set quantityFoundInDesc to 1
                        if (productQuantity.Contains('.'))
                        {
                            quantityFoundInDesc = "1";
                        }

                        //not float, set quantityFoundInDesc to 0.1 (default is 100 gr)
                        else
                        {
                            quantityFoundInDesc = "0.1";
                        }
                    }


                    //found quantity in description
                    else
                    {
                        if(double.TryParse(pdp.GetQuantityFromDescription(productDescription), out quantityInDesc))
                        {
                            quantityFoundInDesc = (quantityInDesc / 1000) + "";
                        }
                    }
                    
                    string productPrice = product.getPrice();
                    ResearchProduct rp = product.getOptionalProductsChosen();
                    DBConnection.InsertReceiptData(familyID, receiptID, productID, productDescription, productQuantity, quantityFoundInDesc, productPrice, 0, true);
                    if (rp != null) {
                        DBConnection.DeleteOptionalData(receiptToUpdate.marketID, product.getsID()); //delete all optional exists
                        DBConnection.InsertOptionalProduct(receiptToUpdate.marketID, product.getsID(), rp);
                    }

                    //not chosen - delete all up to 5 optionals
                    else if (rp == null)
                    {
                        DBConnection.DeleteOptionalData(receiptToUpdate.marketID, product.getsID());

                    }
                }
                DBConnection.UpdateStatus(familyID, receiptID, "1");
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        /*
        * Return receipt to accept
        */
        public void ReturnReceiptToAccept(string familyID, ReceiptToReturn receiptToUpdate)
        {
            //first we update status to -1
            DBConnection.UpdateStatus(familyID, receiptToUpdate.receiptID, "-1");
            string marketId = receiptToUpdate.marketID;

            //than we need to find similar products
            foreach (MetaData product in receiptToUpdate.products)
            {
                string productId = product.getsID();
                if (DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketId + "' AND OP.ProductID ='" + productId + "'").Count == 0)
                {
                    List<ResearchProduct> optionalProducts = jwd.GetTopFiveSimilarProducts(product.description);
                    DBConnection.InsertOptionalProducts(marketId, productId, optionalProducts);
                }
            }

            //after finished, set status to 0
            DBConnection.UpdateStatus(familyID, receiptToUpdate.receiptID, "0");
        }

        public string GetAllApprovedFamilyData(string familyId)
        {
            //nm.updateNutrients(@"C:\Users\Maor\Desktop\nutrients\mabat_foods_2013.xlsx");
            Dictionary<string, ReceiptToReturn> allInfo = new Dictionary<string, ReceiptToReturn>();
            string queryStatusApproved = "SELECT fu.ReceiptID, fu.MarketID, fu.UploadTime, rd.ProductID, rd.Description, rd.Quantity, rd.DescriptionQuantity, rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyId + "'";
            List<string> resultsStatusApproved = DBConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string receiptID = recordSplit[0];
                string marketID = recordSplit[1];
                string uploadTime = recordSplit[2];
                string productID = recordSplit[3];
                List<string> nutrientsString = DBConnection.SelectQuery("select * from Nutrients where Nutrients.SID = (select TOP (1) SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "')");
                List<Nutrient> nutrients = new List<Nutrient>();
                if (nutrientsString.Count != 0)
                    nutrients = nm.ToNutList(nutrientsString.ElementAt(0).Split(',').ToList());
                string description = recordSplit[4];
                string quantity = recordSplit[5];
                string descriptionQuantity = recordSplit[6];
                double calculatedQuantity = (double.Parse(quantity) * double.Parse(descriptionQuantity));
                for (int i = 0; i < nutrients.Count; i++)
                {
                    nutrients[i].Value = nutrients.ElementAt(i).Value * calculatedQuantity * 10;
                }
                string price = recordSplit[7];
                if (allInfo.ContainsKey(receiptID))//Receipt exists
                    allInfo[receiptID].AddProduct(productID, description, calculatedQuantity.ToString(), price, 0, true, nutrients);
                else//Create Receipt for family ID
                {
                    allInfo.Add(receiptID, new ReceiptToReturn(receiptID, marketID, "", "1", uploadTime));
                    allInfo[receiptID].AddProduct(productID, description, calculatedQuantity.ToString(), price, 0, true, nutrients);
                }

            }
            List<ReceiptToReturn> toReturn = new List<ReceiptToReturn>();
            foreach (KeyValuePair<string, ReceiptToReturn> receipt in allInfo)//for all receipts for family
            {
                toReturn.Add(receipt.Value);
            }
            return JsonConvert.SerializeObject(toReturn);
        }

        /*
         * This method return product info 
         * info: Description, price
         */
        public List<string> GetProductInfo(string productID, string MarketID)
        {
            string originalSQL = "SELECT * FROM " + MarketID + " WHERE sID='" + productID + "'";
            string newSQL = "SELECT * FROM " + MarketID + " WHERE sID='";
            string prefix = "729";

            if (productID.Length < 13)
            {
                string newSid = prefix + getZero(productID) + productID;
                newSQL += newSid + "'";
            }
            else
            {
                newSQL += productID + "'";
            }

            //First try execute originalSQL
            List<string> productInfoFromDb = DBConnection.SelectQuery(originalSQL);

            //if no result, try execute newSQL 
            if (productInfoFromDb.Count == 0)
            {
                productInfoFromDb = DBConnection.SelectQuery(newSQL);
            }

            return productInfoFromDb;
        }

        public string GetProductDataWithOptionalNames(string productID, string marketID)
        {
            Dictionary<List<string>, List<ResearchProduct>> toReturn = new Dictionary<List<string>, List<ResearchProduct>>();
            List<string> productData = GetProductInfo(productID, marketID);
            if (productData.Count != 0)
            {
                List<ResearchProduct> products = GetAndInsertOptionalNames(productData[0].Split(',')[1],marketID, productID);
                toReturn.Add(productData, products);
            }
            return JsonConvert.SerializeObject(toReturn.ToList());
        }

        public List<ResearchProduct> GetAndInsertOptionalNames(string prudctName, string marketID, string productID)
        {
            List<ResearchProduct> products;
            if (DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'").Count == 0)
            {
                products = jwd.GetTopFiveSimilarProducts(prudctName); 
                DBConnection.InsertOptionalProducts(marketID, productID, products);
            }
            else
            {
                products = CreateResearchProductListForProduct(DBConnection.SelectQuery("select SID,OptionalName,Similarity from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'"));
            }
            return products;
        }

        public List<ResearchProduct> GetOptionalNames(string prudctName)
        {
            List<ResearchProduct> products;
            products = jwd.GetTopFiveSimilarProducts(prudctName);
            return products;
        }

        public bool DeleteReceipt(string receiptID)
        {
            try
            {
                DBConnection.DeleteReceiptData(receiptID);
                DBConnection.DeleteFamilyReceipt(receiptID);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Error - DeleteReceipt 596", e);
                return false;
            }
        }

        #endregion

        #region private Methods

        /*
        * return hash (sha1) for a given image
        * same images get the same hash 
        */
        private string GetReceiptHashCode(Image image)
        {
            using (var ms = new MemoryStream())
            {
                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                image.Save(ms, image.RawFormat);
                return BitConverter.ToString(sha1.ComputeHash(ms.ToArray())).Replace("-", "");
            }
        }

        /*
        * Return list of researchProduct for productId - for approval
        */
        private List<ResearchProduct> CreateResearchProductListForProduct(List<string> optionalName)
        {
            List<ResearchProduct> researchProduct = new List<ResearchProduct>();
            foreach (string s in optionalName)
            {
                string[] sidAndName = s.Split(',');
                researchProduct.Add(new ResearchProduct(sidAndName[0], sidAndName[1], sidAndName[2]));
            }
            return researchProduct;
        }
        
        /*
         * This method check if the receipt uploaded in the past
         * if uploaded, we need to remove it from the dictionary
         */
        private Dictionary<string, Image> DetectReceiptMultiplicty(Dictionary<string, Image> imgNameAndImg)
        {
            Dictionary<string, Image> toReturn = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Image> rec in imgNameAndImg)
            {
                string receiptID = GetReceiptHashCode(rec.Value);
                Image image = rec.Value;

                if (!(DBConnection.SelectQuery("SELECT * FROM FamilyUploads where ReceiptID='" + receiptID + "' AND NOT ReceiptStatus = -1").Count > 0))
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
        private void CompleteInfoAndMarks(List<Receipt> receipts, string selectedMarket)
        {
            //iterate over all the receipts
            foreach (Receipt receipt in receipts)
            {
                receipt.SetMarketID(selectedMarket);
                List<OcrWord> wordsToDraw = new List<OcrWord>();
                List<OcrWord> tmpWords;
                double averageX = 0, averageY = 0, numOfWords = 0, normalizedX, normalizedWidth;

                //iterate over all the item in receipt
                foreach (KeyValuePair<string, List<MetaData>> sid in receipt.GetIdToMetadata())
                {
                    if (sid.Key.Length < 13 && receipt.GetIdToMetadata().ContainsKey("729" + sid.Key))
                        continue;
                    foreach (MetaData obj in sid.Value)
                    {
                        /*
                        using (var tw = new StreamWriter(@"C:\Users\Maor\Desktop\test.txt", true))
                        {
                            tw.WriteLine(sid.Key);
                        }
                        */
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
                                    obj.setOptionalProducts(jwd.GetTopFiveSimilarProducts(description));
                                }
                            }

                            tmpWords = receipt.GetWord(sid.Key);
                            foreach (OcrWord word in tmpWords)
                            {
                                numOfWords += 1;
                                normalizedX = (int)((word.getX() / receipt.GetWidth()) * receipt.GetOriginalImage().Width);
                                normalizedWidth = (int)((word.getWidth() / receipt.GetWidth()) * receipt.GetOriginalImage().Width);
                                averageX = ((averageX * (numOfWords - 1)) + normalizedX + normalizedWidth) / numOfWords;
                                averageY = ((averageY * (numOfWords - 1)) + word.getY()) / numOfWords;
                                obj.setYcoordinate(word.getY());
                                wordsToDraw.Add(word);
                            }
                        }
                    }
                }
                receipt.SetAverageCoordinates(averageX, averageY);
                //draw marks on images -> save them -> insert detected products to db -> send email to researcher
                SetReceiptValidProducts(receipt);
                marksDrawing.Draw(wordsToDraw, receipt);
                MarkedImageSaver.SaveMarkedImage(receipt, selectedFamilyID);
                SaveReceiptDataToDB(selectedFamilyID, receipt);

                wordsToDraw = null;
                tmpWords = null;
            }
            //EmailSender.sendEmail(receipts.Count, selectedFamilyID);
            //EmailSender = null;
            ocrProcessing = null;
            ocrTextParser = null;
            marksDrawing = null;
            receipts = null;
            MarkedImageSaver = null;
            DBConnection = null;
        }

        /*
         * if word is False Positive - IsFP = true, else IsFP = False
         */
        private void SetReceiptValidProducts(Receipt receipt)
        {
            double normalizedX, normalizedWidth;
            bool xRuleDeviation;
            double averageX = receipt.GetXAverage();

            foreach (KeyValuePair<string, List<MetaData>> sid in receipt.GetIdToMetadata())
            {
                foreach (MetaData obj in sid.Value)
                {
                    if (obj.description != "")
                    {
                        List<OcrWord> tmpWords = receipt.GetWord(sid.Key);
                        foreach (OcrWord word in tmpWords)
                        {
                            normalizedX = (int)((word.getX() / receipt.GetWidth()) * receipt.GetOriginalImage().Width);
                            normalizedWidth = (int)((word.getWidth() / receipt.GetWidth()) * receipt.GetOriginalImage().Width + 14);
                            xRuleDeviation = Math.Abs(normalizedX + normalizedWidth - (receipt.GetXAverage())) < 0.08 * receipt.GetWidth();
                            obj.setvalidProduct(xRuleDeviation);
                        }
                    }

                }
            }
        }

        private void SaveReceiptDataToDB(string FamilyID, Receipt receipt)
        {
            try
            {
                DBConnection.UpdateStatus(FamilyID, receipt.GetName(), "0");
                foreach (KeyValuePair<string, List<MetaData>> data in receipt.GetIdToMetadata())
                {
                    string productId = data.Key;
                    List<MetaData> values = data.Value;

                    if (values.ToArray()[0].getDescription() != "")
                    {
                        string desc = values.ToArray()[0].getDescription();
                        string price = values.ToArray()[0].getPrice();
                        string quantitiy = values.ToArray()[0].getQuantity();
                        double yCoordinate = values.ElementAt(0).getyCoordinate();
                        bool validProduct = values.ElementAt(0).getvalidProduct();
                        string marketId = receipt.GetMarketID();
                        List<ResearchProduct> optionalProducts = values.ElementAt(0).getOptionalProducts();
                        DBConnection.InsertReceiptData(FamilyID, receipt.GetName(), productId, desc, quantitiy, "1", price, yCoordinate, validProduct);
                        //foreach (ResearchProduct rp in optionalProducts)
                        //{
                        //AzureConnection.insertOptionalProducts(id, rp.SID, rp.Name);
                        if (DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketId + "' AND OP.ProductID ='" + productId + "'").Count == 0)
                            DBConnection.InsertOptionalProducts(marketId, productId, optionalProducts);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error: Exeption {e}");
            }
        }

        private string getZero(string sid)
        {
            String zeros = "";
            for (int i = 0; i < 10 - sid.Length; i++)
                zeros += "0";
            return zeros;
        }

        /*
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
        */
    }
    #endregion
}