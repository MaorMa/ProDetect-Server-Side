using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcrProject.Parser
{
    /*
     * This class responsible for parsing the converted image
     */
    class TextParser
    {
        //fields
        //String= receipt name, String=Product ID, List=metaData (for multi products with same name\id)
        private Dictionary<string, Dictionary<string, List<MetaData>>> receiptAndIdsMetaData;
        private string date;

        //C'tor
        public TextParser()
        {
            this.receiptAndIdsMetaData = new Dictionary<string, Dictionary<string, List<MetaData>>>();
        }

        //parsing function
        //get all numbers from text and insert to catalogID
        public void parsing(Receipt receipt)
        {
            date = "No Date";
            List<string> lines = receipt.getRows();
            for (int i = 0; i < lines.Count; i++)
            {
                string sID = lines[i];
                string nextsID;
                if (i + 1 < lines.Count)
                    nextsID = lines[i + 1];
                else
                    nextsID = "";
                addSid(sID, nextsID, receipt);
            }
        }

        public string getDate()
        {
            return this.date;
        }

        private void addSid(string firstLine, string secondLine, Receipt receipt)
        {
            Dictionary<string, List<MetaData>> receiptsIdToMetadata = receipt.getIdToMetadata();
            bool dateFlag = false;
            string[] firstLineSeperate = firstLine.Split(' ');
            string weight = "";
            if (hasKG(secondLine))
            {
                weight = getSmaller(secondLine);
            }
            else if (!hasInt(secondLine).Equals("0"))
            {
                weight = hasInt(secondLine);
            }
            else
            {
                weight = "1";
            }
            double num;
            foreach (string s in firstLineSeperate)
            {
                if (double.TryParse(s, out num) && !s.Contains(",") && !s.StartsWith("0"))//if number
                {
                    if (!receipt.getIdToMetadata().ContainsKey(s))
                    {
                        receiptsIdToMetadata.Add(s, new List<MetaData>());
                        receiptsIdToMetadata[s].Add(new MetaData(s, "", weight, "",0,true));
                    }
                    else
                    {
                        //allready exists
                        foreach(MetaData value in receiptsIdToMetadata[s])
                        {
                            try
                            {
                                if (Convert.ToDouble(value.getQuantity()) > Convert.ToDouble(weight) + 5)
                                {
                                    receiptsIdToMetadata[s].ElementAt(0).setQuantity(weight);
                                }
                            }catch(Exception) { }
                        }
                    }
                }
                else if (!dateFlag && s.Contains('/'))//if date
                {
                    string[] dateCheck = s.Split('/');
                    if (dateCheck.Length == 3 && isDate(dateCheck))
                    {
                        receipt.setDate(s);
                    }
                }
            }
            //set the full dict' into the receipt
            receipt.setIdToMetadata(receiptsIdToMetadata);
        }

        //Check if array of 3 numbers is date
        private bool isDate(string[] date)
        {
            int check;
            if (int.TryParse(date[0], out check))//day
            {
                if (check >= 1 && check <= 31)
                {
                    if (int.TryParse(date[1], out check))//month
                    {
                        if (check >= 1 && check <= 12)
                        {
                            if (int.TryParse(date[2], out check))//year
                            {
                                string yearS = DateTime.Now.Year.ToString();
                                int year;
                                int.TryParse(yearS, out year);
                                year = year % 100;
                                if (check >= 0 && check <= year)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        //Check if line has Hebrew KG word
        private bool hasKG(string line)
        {
            bool containsDot = false;
            bool containsKG = false;
            string[] seperate = line.Split(' ');
            foreach (string word in seperate)
            {
                //Console.WriteLine(word);
                if (word.Contains("ק\"ג") || word.Contains("\"ג") || word.Contains("ק\"") || word.Contains("\""))
                {
                    containsKG = true;
                }
                else if (word.Contains(",") || word.Contains("."))
                {
                    containsDot = true;
                }
            }
            return containsDot && containsKG;
        }

        //Check if line has Integer
        private string hasInt(string line)
        {
            int check;
            string[] seperate = line.Split(' ');
            foreach (string word in seperate)
            {
                if (!word.StartsWith("0"))
                {
                    //Console.WriteLine(word);
                    if (int.TryParse(word, out check))
                    {
                        if (check <= 10)
                            return word;
                    }
                }
            }
            return "1";
        }

        //returns smallest number in the line with the KG word in it 
        private string getSmaller(string line)
        {
            string[] seperate = line.Split(' ');
            double num;
            double smallest = 10000.0; //default num
            foreach (string s in seperate)
            {
                if ((s.Contains(".") || s.Contains(",")) && !s.EndsWith(".") && !s.StartsWith("."))
                {
                    if (double.TryParse(s.Replace(",", "."), out num))
                    {
                        if (num < smallest && num != 0)
                        {
                            smallest = num;
                        }
                    }
                }
            }
            if (smallest == 10000.0)
            {
                return "1";
            }
            return smallest.ToString();
        }

        public List<Receipt> getAllRecieptsData(List<Receipt> receipts)
        {
            foreach (var receipt in receipts)
            {
                parsing(receipt);
            }
            return receipts;
        }
    }
}
