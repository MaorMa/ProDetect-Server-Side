using Newtonsoft.Json;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Mangagers
{
    public class StatisticsMngr : AMngr
    {

        public string GetAllPricesByCategories(string familyID)
        {
            //query get marketID,ProductID,Price
            Dictionary<string, Double> categoryAndPrices = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID,rd.Quantity, rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = AzureConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                string quantity = recordSplit[2];
                double price = double.Parse(recordSplit[3]) * double.Parse(quantity);
                List<string> sID = AzureConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                if (sID.Count > 0)
                {
                    string category = getCategoryByID(sID[0]);
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
            Dictionary<string, Double> categoryAndPrices = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID, rd.Quantity, rd.DescriptionQuantity FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = AzureConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                double quantity = double.Parse(recordSplit[2]);
                double descriptionQuantity = double.Parse(recordSplit[3]);
                double totalQuantity = quantity * descriptionQuantity;
                List<string> sID = AzureConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                if (sID.Count > 0)
                {
                    string category = getCategoryByID(sID[0]);
                    //category already exists - add price
                    if (!category.Equals("אחר"))
                    {
                        if (categoryAndPrices.ContainsKey(category))
                        {
                            categoryAndPrices[category] += totalQuantity;
                        }

                        //category not exists - add category and price
                        else
                        {
                            categoryAndPrices.Add(category, totalQuantity);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(categoryAndPrices.ToList());
        }

        public string GetAllQuantitiesByNutrients(string familyID)
        {
            //string - nutrient
            //Double - quantity
            Dictionary<string, Double> nutrientAndQuantity = new Dictionary<string, Double>();
            string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID, rd.Quantity, rd.DescriptionQuantity FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + familyID + "'";
            List<string> resultsStatusApproved = AzureConnection.SelectQuery(queryStatusApproved);
            foreach (string record in resultsStatusApproved)
            {
                string[] recordSplit = record.Split(',');
                string marketID = recordSplit[0];
                string productID = recordSplit[1];
                double quantity = double.Parse(recordSplit[2]);
                double descriptionQuantity = double.Parse(recordSplit[3]);
                double totalQuantity = quantity * descriptionQuantity;
                List<string> nutrientsString = AzureConnection.SelectQuery("select * from Nutrients where Nutrients.SID = (select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "')");
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


        public string GetCompareByCost(string familyID)
        {
            //string - nutrient
            //Double - quantity
            Dictionary<string, Dictionary<string, Double>> FamilyPriceCompareCategories = new Dictionary<string, Dictionary<string, Double>>();
            List<string> AllFamilies = AzureConnection.getFamiliesWithApprovedData();
            //OtherFamiliesExceptMe.Remove(familyID);

            //foreach other family
            foreach (string family in AllFamilies)
            {
                string queryStatusApproved = "SELECT fu.MarketID, rd.ProductID,rd.Quantity, rd.Price FROM FamilyUploads as fu JOIN ReceiptData as rd ON fu.ReceiptID = rd.ReceiptID AND fu.ReceiptStatus = 1 WHERE fu.FamilyID='" + family + "'";
                List<string> resultsStatusApproved = AzureConnection.SelectQuery(queryStatusApproved);
                foreach (string record in resultsStatusApproved)
                {
                    string[] recordSplit = record.Split(',');
                    string marketID = recordSplit[0];
                    string productID = recordSplit[1];
                    string quantity = recordSplit[2];
                    double price = double.Parse(recordSplit[3]) * double.Parse(quantity);
                    List<string> sID = AzureConnection.SelectQuery("select SID from OptionalProducts AS OP WHERE OP.MarketID='" + marketID + "' AND OP.ProductID ='" + productID + "'");
                    if (sID.Count > 0)
                    {
                        string category = getCategoryByID(sID[0]);
                        string key = "";

                        //contains family
                        if (family.Equals(familyID))
                        {
                            key = "me";
                        }
                        else
                        {
                            key = "other";
                        }

                        if (FamilyPriceCompareCategories.ContainsKey(key))
                        {
                            var x = FamilyPriceCompareCategories[key];
                            //contains category
                            if (FamilyPriceCompareCategories[key].ContainsKey(category))
                            {
                                if (family.Equals(familyID))
                                {
                                    FamilyPriceCompareCategories[key][category] += price;
                                }
                                else
                                {
                                    FamilyPriceCompareCategories[key][category] += price / (AllFamilies.Count-1);
                                }
                            }
                            //not contains category
                            else
                            {
                                if (family.Equals(familyID))
                                {
                                    FamilyPriceCompareCategories[key].Add(category, price);
                                }
                                else
                                {
                                    FamilyPriceCompareCategories[key].Add(category, price / (AllFamilies.Count - 1)); 
                                }
                            }
                        }

                        //not contains family
                        else
                        {
                            Dictionary<string, double> toInsert = new Dictionary<string, double>();
                            if (key.Equals("me"))
                            {

                                FamilyPriceCompareCategories.Add("me", new Dictionary<string, double>());
                                //insert new category and price
                                toInsert.Add(category, price);
                                FamilyPriceCompareCategories[key] = toInsert;
                            }
                            else
                            {
                                //create family 
                                FamilyPriceCompareCategories.Add("other", new Dictionary<string, double>());
                                //insert new category and price
                                toInsert.Add(category, price);
                                FamilyPriceCompareCategories[key] = toInsert;
                            }
                        }
                    }

                }
            }
            Dictionary<string, string> myList = new Dictionary<string,string>();
            myList.Add("other", JsonConvert.SerializeObject(FamilyPriceCompareCategories["other"].Values.ToList()));
            myList.Add("me", JsonConvert.SerializeObject(FamilyPriceCompareCategories["me"].Values.ToList()));
            return JsonConvert.SerializeObject(myList.ToList()); 
        }

        private string getCategoryByID(string id)
        {
            // 1 + 3 + 811- מוצרי חלב וביצים  
            // 2 - בשר, עוף ודגים
            // 4 - קיטניות
            // 5 - חטיפים, מאפים ובצקים
            // 6 + 7 - פירות וירקות
            // 821 + 83 - שמנים ורטבים
            // 910-912 - תבלינים (סוכר\מלח)
            // 913-918 - ממתקים וממתיקים
            // 92 + 94 - תה, קפה ומשקאות לא אלכוהוליים 
            // 93 - משקאות אלכוהוליים

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
            else if (id.StartsWith("910") || id.StartsWith("912"))
            {
                return "תבלינים";
            }
            else if (id.StartsWith("913") || id.StartsWith("918"))
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