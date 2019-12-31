using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RRS_API.Models.Parsers
{
    public class ProductDescriptionParser
    {
        private List<Regex> Regexes = new List<Regex>();

        public ProductDescriptionParser()
        {
            this.Regexes.Add(new Regex(@"\d+\*+\d+ג"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+ג"));
            this.Regexes.Add(new Regex(@"\d+גר"));
            this.Regexes.Add(new Regex(@"\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+קג"));
            this.Regexes.Add(new Regex(@"\d+ " + "ק\""));
            this.Regexes.Add(new Regex(@"\d+ קג"));
            this.Regexes.Add(new Regex(@"\d+" + "ק\"ג"));
            this.Regexes.Add(new Regex(@"\d+ ליטר"));
            this.Regexes.Add(new Regex(@"\d+ " + "ק\"ג"));
            this.Regexes.Add(new Regex(@"\d+%"));
            this.Regexes.Add(new Regex(@"\d+"));
        }
        /*
         * This function return the quantity of a given product includes in his description
         */
        public string getQuantityFromDescription(string productDescription)
        {
            string toReturnInGrams = "";
            foreach (Regex Regex in this.Regexes)
            {
                toReturnInGrams = Regex.Match(productDescription).Value;
                if (!toReturnInGrams.Equals(""))
                {
                    if (toReturnInGrams.Contains("%"))
                    {
                        return "";
                    }
                    if (toReturnInGrams.Contains("קג") || toReturnInGrams.Contains("ק\"ג") || toReturnInGrams.Contains("ק\"") || toReturnInGrams.Contains("ליטר"))
                    {
                        return new String(toReturnInGrams.Where(Char.IsDigit).ToArray()) + "000";
                    }
                    else if (toReturnInGrams.Contains("*"))
                    {
                        string[] splitted = toReturnInGrams.Split('*');
                        string clean1 = new String(splitted[0].Where(Char.IsDigit).ToArray());
                        string clean2 = new String(splitted[1].Where(Char.IsDigit).ToArray());
                        return Double.Parse(clean1) * Double.Parse(clean2) + "";
                    }
                    return new String(toReturnInGrams.Where(Char.IsDigit).ToArray());
                }
            }
            return toReturnInGrams;
        }
    }
}