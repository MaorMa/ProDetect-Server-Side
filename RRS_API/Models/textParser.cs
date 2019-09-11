using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OcrProject.Parser
{
    /*
     * This class responsible for parsing the converted file (conveted by ocr engine)
     */
    class TextParser
    {
        //fields
        //String= receipt name, String=Product ID, List=metaData (for multi products with same name\id)
        private Dictionary<String, Dictionary<String, List<MetaData>>> receiptAndIdsMetaData;
        private String date;

        //C'tor
        public TextParser()
        {
            this.receiptAndIdsMetaData = new Dictionary<String, Dictionary<String, List<MetaData>>>();
        }

        //parsing function
        //get all numbers from text and insert to catalogID
        public void parsing(Receipt receipt)
        {
            date = "No Date";
            List<String> lines = receipt.getRows();
            for (int i = 0; i < lines.Count; i++)
            {
                String sID = lines[i];
                String nextsID;
                if (i + 1 < lines.Count)
                    nextsID = lines[i + 1];
                else
                    nextsID = "";
                addSid(sID, nextsID, receipt);
            }
        }

        public String getDate()
        {
            return this.date;
        }

        private void addSid(String firstLine, String secondLine, Receipt receipt)
        {
            Dictionary<String, List<MetaData>> receiptsIdToMetadata = receipt.getIdToMetadata();
            bool dateFlag = false;
            String[] firstLineSeperate = firstLine.Split(' ');
            String weight = "";
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
                weight = "";
            }
            double num;
            foreach (String s in firstLineSeperate)
            {
                if (double.TryParse(s, out num) && !s.Contains(","))//if number
                {
                    if (!receipt.getIdToMetadata().ContainsKey(s))
                    {
                        receiptsIdToMetadata.Add(s, new List<MetaData>());
                        receiptsIdToMetadata[s].Add(new MetaData(s, "", weight, "",0));
                    }
                    else
                    {
                        receiptsIdToMetadata[s].Add(new MetaData(s, "", weight, "",0));
                    }
                }
                else if (!dateFlag && s.Contains('/'))//if date
                {
                    String[] dateCheck = s.Split('/');
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
        private bool isDate(String[] date)
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
                                String yearS = DateTime.Now.Year.ToString();
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
        private bool hasKG(String line)
        {
            String[] seperate = line.Split(' ');
            foreach (String word in seperate)
            {
                //Console.WriteLine(word);
                if (word.Contains("ק\"ג"))
                {
                    return true;
                }
            }
            return false;
        }

        //Check if line has Integer
        private String hasInt(String line)
        {
            int check;
            String[] seperate = line.Split(' ');
            foreach (String word in seperate)
            {
                //Console.WriteLine(word);
                if (int.TryParse(word, out check))
                {
                    if (check <= 10)
                        return word;
                }
            }
            return "0";
        }

        //returns smallest number in the line with the KG word in it 
        private String getSmaller(String line)
        {
            String[] seperate = line.Split(' ');
            double num;
            double smallest = 10000.0; //default num
            foreach (String s in seperate)
            {
                if (double.TryParse(s.Replace(",", "."), out num))
                {
                    if (num < smallest && num != 0)
                    {
                        smallest = num;
                    }
                }
            }
            if (smallest == 10000.0)
            {
                return "";
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
