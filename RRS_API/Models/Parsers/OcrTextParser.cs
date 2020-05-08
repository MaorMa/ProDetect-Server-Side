using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OcrProject.Parser
{
    /// <summary>
    /// This class responsible for parsing the text of ocr result for a given receipt
    /// </summary>
    public class OcrTextParser
    {

        /// <summary>
        /// This method responsible for parsing the given receipt object
        /// </summary>
        /// <param name="receipt"></param>
        public void Parsing(Receipt receipt)
        {
            List<string> lines = receipt.getRows();
            for (int i = 0; i < lines.Count; i++)
            {
                string sID = lines[i];
                string nextsID;
                if (i + 1 < lines.Count)
                    nextsID = lines[i + 1];
                else
                    nextsID = "";
                AddSid(sID, nextsID, receipt);
            }
        }

        private void AddSid(string firstLine, string secondLine, Receipt receipt)
        {
            Dictionary<string, List<MetaData>> receiptsIdToMetadata = receipt.GetIdToMetadata();
            //bool dateFlag = false;
            string[] firstLineSeperate = firstLine.Split(' ');
            string weight = GetQuantity(secondLine);
            foreach (string s in firstLineSeperate)
            {
                /*
                using (var tw = new StreamWriter(@"C:\Users\Maor\Desktop\test2.txt", true))
                {
                    tw.WriteLine(s);
                }
                */
                double num;
                if (double.TryParse(s, out num) && !s.Contains(",") && !s.StartsWith("0"))//check if number
                {
                    string id = num.ToString();

                    if (id.Length == 13 && (id.StartsWith("7") || id.StartsWith("129")) && !id.StartsWith("780") && !id.StartsWith("761") && !id.StartsWith("762") && !id.StartsWith("729"))
                    {
                        id = id.Substring(Math.Max(0, id.Length - 10));
                    }

                    if(id.Length == 3 && id.StartsWith("1") && weight == "1")
                    {
                        continue;
                    }
                    //IdToMetadata not contains product
                    if (!receipt.GetIdToMetadata().ContainsKey(id))
                    {
                        receiptsIdToMetadata.Add(id, new List<MetaData>());
                        receiptsIdToMetadata[id].Add(new MetaData(id, "", weight, "", 0, true));
                    }
                    else
                    {
                        //allready exists
                        foreach (MetaData value in receiptsIdToMetadata[id])
                        {
                            try
                            {
                                //if ((Convert.ToDouble(weight) - Convert.ToDouble(value.getQuantity())) >= 1)
                                if (Convert.ToDouble(value.getQuantity()) == 1 && Convert.ToDouble(weight) != 1)
                                {
                                    receiptsIdToMetadata[id].ElementAt(0).setQuantity(weight);
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                /*
                else if (!dateFlag && s.Contains('/'))//if date
                {
                    string[] dateCheck = s.Split('/');
                    if (dateCheck.Length == 3 && IsDate(dateCheck))
                    {
                        receipt.SetDate(s);
                    }
                }
                */
            }
            //set the full dict' into the receipt
            receipt.SetIdToMetadata(receiptsIdToMetadata);
        }

        /// <summary>
        /// Check if line contains Hebrew KG word or *
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool HasKG(string line)
        {
            bool containsDot = false;
            bool containsKG = false;
            string[] seperate = line.Split(' ');
            foreach (string word in seperate)
            {
                //Console.WriteLine(word);

                if (word.Contains("ק\"ג") || word.Contains("\"ג") || word.Contains("ק\""))
                {
                    containsKG = true;
                }

                if (word.Contains(",") || word.Contains("."))
                {
                    containsDot = true;
                }
            }
            return containsDot && containsKG;
        }


        /// <summary>
        /// This method get quantity of product from line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetQuantity(string line)
        {
            string[] seperate = line.Split(' '); //split

            if (seperate.Length > 0)
            {
                //has KG
                if (HasKG(line))
                {
                    return GetSmaller(line);
                }

                //hasn't KG - need to check (int and double we take int, double and double we take smaller)
                else
                {
                    int intValue = HasInt(line);
                    string decimalValue = HasDecimalNumber(line).ToString();

                    if (!(intValue == 1)) //contains int
                    {
                        if (!(decimalValue.Equals("1"))) //int and double
                        {
                            if (!(intValue > 10 || intValue <= 0))
                            {
                                return intValue.ToString();
                            }
                            else
                            {
                                return "1";
                            }
                        }
                        return "1";
                    }

                    //contains decimal
                    else
                    {
                        if (!(decimalValue.Equals("1")))
                        {
                            return GetSmaller(line);
                        }
                        return "1";
                    }
                }
            }

            //default - return 1
            return "1";
        }

        /// <summary>
        /// This method checks if line contains integer.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int HasInt(string line)
        {
            int check,num = 1;
            string[] seperate = line.Split(' ');
            int count = 0;
            foreach (string word in seperate)
            {
                if (!word.StartsWith("0"))
                {
                    //Console.WriteLine(word);
                    if (int.TryParse(word, out check))
                    {
                        if (check <= 10 && check != 1)
                        {
                            num = check;
                            count++;
                        }

                    }
                }
            }

            if(count == 1)
            {
                return num;
            }
            return 1;
        }

        /// <summary>
        /// This method checks if line contains decimal number.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private decimal HasDecimalNumber(string line)
        {
            decimal toReturn = 1;
            decimal check;
            int count = 0;
            string[] seperate = line.Split(' ');
            foreach (string word in seperate)
            {
                    if (word.Replace(",", ".").Contains("."))
                    {
                        if (decimal.TryParse(word, out check))
                        {
                            count++;
                            toReturn = check;
                        }
                    }
            }

            if (count == 2)
            {
                return toReturn;
            }
            return 1;
        }

        /// <summary>
        /// This method returns the qunatity of a product in the line with the KG word in it 
        /// if not found, default number is 1
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetSmaller(string line)
        {
            string[] seperate = line.Split(' ');
            double num;
            double smallest = 2.9;
            foreach (string s in seperate)
            {
                if ((s.Contains(".") || s.Contains(",")) && !s.EndsWith(".") && !s.StartsWith("."))
                {
                    if (double.TryParse(s.Replace(",", "."), out num))
                    {
                        if (num <= smallest && num > 0)
                        {
                            return num.ToString();
                        }
                    }
                }
            }
            return "1";
        }

        public List<Receipt> GetAllRecieptsData(List<Receipt> receipts)
        {
            foreach (var receipt in receipts)
            {
                Parsing(receipt);
            }
            return receipts;
        }
    }
}
