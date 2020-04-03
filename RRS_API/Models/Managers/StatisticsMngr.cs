using Newtonsoft.Json;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Mangagers
{
    public class StatisticsMngr
    {
        private DBConnection DBConnection = DBConnection.GetInstance();

        public string GetAllPricesByCategories(string familyID)
        {
            //query get marketID,ProductID,Price
            Dictionary<string, Double> categoryAndPrices = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID,rd.Quantity, rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = DBConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                string quantity = recordSplit[2];
                double price = double.Parse(recordSplit[3]) * double.Parse(quantity);
                List<string> sID = DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                if (sID.Count > 0)
                {
                    string category = GetCategoryByID(sID[0]);
                    //category already exists - add price

                    if (categoryAndPrices.ContainsKey(category))
                    {
                        categoryAndPrices[category] += price;
                    }

                    //category not exists - add category and price
                    else
                    {
                        categoryAndPrices.Add(category, price);
                    }
                }

            }
            return JsonConvert.SerializeObject(categoryAndPrices.ToList());
        }

        public string GetAllQuantitiesByCategories(string familyID)
        {
            Dictionary<string, Double> categoryAndQuantities = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID, rd.Quantity, rd.DescriptionQuantity FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = DBConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                double quantity = double.Parse(recordSplit[2]);
                double descriptionQuantity = double.Parse(recordSplit[3]);
                double totalQuantity = quantity * descriptionQuantity;
                List<string> sID = DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                if (sID.Count > 0)
                {
                    string category = GetCategoryByID(sID[0]);
                    //category already exists - add price
                    if (categoryAndQuantities.ContainsKey(category))
                    {
                        categoryAndQuantities[category] += totalQuantity;
                    }

                    //category not exists - add category and price
                    else
                    {
                        categoryAndQuantities.Add(category, totalQuantity);
                    }
                }
            }
            return JsonConvert.SerializeObject(categoryAndQuantities.ToList());
        }

        /*
        public string GetAllQuantitiesByNutrients(string familyID)
        {
            //string - nutrient
            //Double - quantity
            Dictionary<string, Double> nutrientAndQuantity = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID, rd.Quantity, rd.DescriptionQuantity FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = DBConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                double quantity = double.Parse(recordSplit[2]);
                double descriptionQuantity = double.Parse(recordSplit[3]);
                double totalQuantity = quantity * descriptionQuantity;
                List<string> nutrientsString = DBConnection.SelectQuery("select * from Nutrients where Nutrients.SID = (select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "')");
                List<Nutrient> nutrients = new List<Nutrient>();
                NutrientMngr nm = new NutrientMngr();
                if (nutrientsString.Count != 0)
                    nutrients = nm.toNutList(nutrientsString.ElementAt(0).Split(',').ToList());
                double calculatedQuantity = (quantity * descriptionQuantity);
                for (int i = 0; i < nutrients.Count; i++)
                {
                    nutrients[i].Value = nutrients.ElementAt(i).Value * calculatedQuantity * 10;

                    if (nutrientAndQuantity.ContainsKey(nutrients[i].Code))
                    {
                        nutrientAndQuantity[nutrients[i].Code] += nutrients[i].Value;
                    }

                    //category not exists - add category and price
                    else
                    {
                        nutrientAndQuantity.Add(nutrients[i].Code, nutrients[i].Value);
                    }
                }

            }
            return JsonConvert.SerializeObject(nutrientAndQuantity.ToList());
        }
        */


        public string GetCompareByCost(string familyID)
        {
            //{"category":{"myValue","otherValue"}},{"category":{"myValue","otherValue"}}...
            Dictionary<string, Tuple<double, double>> FamilyPriceCompareCategories = new Dictionary<string, Tuple<double, double>>();
            List<string> AllFamilies = DBConnection.GetFamiliesWithApprovedData();
            //foreach other family

            foreach (string family in AllFamilies)
            {
                int numOfMyReceipts = 0;
                int numOfOtherReceipts = 0;
                string numOfReceiptsOfCurrentFamilyQuery = "SELECT fu.ReceiptID FROM FamilyUploads as fu WHERE fu.ReceiptStatus = 1 AND fu.FamilyID='" + family + "'";
                List<string> numOfReceipts = DBConnection.SelectQuery(numOfReceiptsOfCurrentFamilyQuery);
                if (family.Equals(familyID))
                {
                    numOfMyReceipts += numOfReceipts.Count;
                }
                else
                {
                    numOfOtherReceipts += numOfReceipts.Count;
                }

                string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID,rd.Quantity, rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID WHERE fu.ReceiptStatus = 1 AND fu.FamilyID='" + family + "'";
                List<string> resultsStatusApproved = DBConnection.SelectQuery(queryStatusApproved);
                foreach (string record in resultsStatusApproved)
                {
                    string[] recordSplit = record.Split(',');
                    string marketID = recordSplit[0];
                    string productID = recordSplit[1];
                    string quantity = recordSplit[2];
                    double price = double.Parse(recordSplit[3]) * double.Parse(quantity);
                    List<string> sID = DBConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                    if (sID.Count > 0)
                    {
                        //get category
                        string category = GetCategoryByID(sID[0]);


                        //contains category
                        if (FamilyPriceCompareCategories.ContainsKey(category))
                        {
                            //me
                            if (family.Equals(familyID))
                            {
                                double myValue = FamilyPriceCompareCategories[category].Item1;
                                double otherValue = FamilyPriceCompareCategories[category].Item2;
                                FamilyPriceCompareCategories[category] = new Tuple<double, double>(myValue + (price / numOfMyReceipts), otherValue);
                            }

                            //other
                            else
                            {
                                double currentmyValue = FamilyPriceCompareCategories[category].Item1;
                                double newOtherValue = FamilyPriceCompareCategories[category].Item2 + ((price / numOfOtherReceipts) / (AllFamilies.Count - 1));
                                FamilyPriceCompareCategories[category] = new Tuple<double, double>(currentmyValue, newOtherValue);
                            }
                        }

                        //not contains category
                        else
                        {
                            if (family.Equals(familyID))
                            {
                                FamilyPriceCompareCategories.Add(category, new Tuple<double, double>(price / numOfMyReceipts, 0));
                            }
                            //other
                            else
                            {
                                FamilyPriceCompareCategories[category] = new Tuple<double, double>(0, (price / numOfOtherReceipts) / (AllFamilies.Count - 1));
                            }
                        }
                    }
                }
            }

            return JsonConvert.SerializeObject(FamilyPriceCompareCategories.ToList());
        }

        private string GetCategoryByID(string id)
        {

            if (id.StartsWith("1") || id.StartsWith("3") || id.StartsWith("811"))
            {
                return "מוצרי חלב וביצים";
            }
            else if (id.StartsWith("2"))
            {
                return "בשר, עוף ודגים";

            }
            else if (id.StartsWith("4"))
            {
                return "קיטניות";
            }
            else if (id.StartsWith("5"))
            {
                return "חטיפים, מאפים ובצקים";
            }
            else if (id.StartsWith("6") || id.StartsWith("7"))
            {
                return "פירות וירקות";
            }
            else if (id.StartsWith("821") || id.StartsWith("83"))
            {
                return "שמנים ורטבים";
            }
            else if (id.StartsWith("910") || id.StartsWith("911") || id.StartsWith("912"))
            {
                return "תבלינים";
            }
            else if (id.StartsWith("913") || id.StartsWith("914") || id.StartsWith("915") || id.StartsWith("916") || id.StartsWith("917") || id.StartsWith("918"))
            {
                return "ממתקים וממתיקים";
            }
            else if (id.StartsWith("92") || id.StartsWith("94"))
            {
                return "תה, קפה, ומשקאות לא אלכוהוליים";
            }
            else if (id.StartsWith("93"))
            {
                return "משקאות אלכוהוליים";
            }
            else
            {
                return "אחר";
            }
        }
    }
}