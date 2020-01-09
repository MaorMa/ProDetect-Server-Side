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
            this.Regexes.Add(new Regex(@"\d+ יח"));
            this.Regexes.Add(new Regex(@"\d+יח"));
            this.Regexes.Add(new Regex(@"\d+ קלוריות"));
            this.Regexes.Add(new Regex(@"\d+x+\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ג"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ ג"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ גר"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+ \* +\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+ג"));
            this.Regexes.Add(new Regex(@"\d+גר"));
            this.Regexes.Add(new Regex(@"\d+גרם"));
            this.Regexes.Add(new Regex(@"\d+ ג"));
            this.Regexes.Add(new Regex(@"\d+ גר"));
            this.Regexes.Add(new Regex(@"\d+ גרם"));
            this.Regexes.Add(new Regex(@"\d+\.+\d+קג"));
            this.Regexes.Add(new Regex(@"\d+\.+\d+ ק"));
            this.Regexes.Add(new Regex(@"\d+קג"));
            this.Regexes.Add(new Regex(@"\d+" + "ק"));
            this.Regexes.Add(new Regex(@"\d+ " + "ק`ג"));
            this.Regexes.Add(new Regex(@"\d+ " + "ק\""));
            this.Regexes.Add(new Regex(@"\d+ קג"));
            this.Regexes.Add(new Regex(@"\d+\* +\d+\.+\d+ לי"));
            this.Regexes.Add(new Regex(@"\d+" + "ק\"ג"));
            this.Regexes.Add(new Regex(@"\d+\.+\d" + " ל"));
            this.Regexes.Add(new Regex(@"\d+\.+\d" + " ליטר"));
            this.Regexes.Add(new Regex(@"\d+\.+\d+\*+\d+ל"));
            this.Regexes.Add(new Regex(@"\d+\.+\d+\*+\d+ ל"));
            this.Regexes.Add(new Regex(@"\d+\.+\d+\*+\d+ ליטר"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ל"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ לי"));
            this.Regexes.Add(new Regex(@"\d+\*+ \d+ לי"));
            this.Regexes.Add(new Regex(@"\d+\*+\d+ ל"));
            this.Regexes.Add(new Regex(@"\d+ " + "מ\"ל"));
            this.Regexes.Add(new Regex(@"\d+" + "מל"));
            this.Regexes.Add(new Regex(@"\d" + " ליטר"));
            this.Regexes.Add(new Regex(@"\d" + "ליטר"));
            this.Regexes.Add(new Regex(@"\d" + "ל"));
            this.Regexes.Add(new Regex(@"\d " + "ל"));
            this.Regexes.Add(new Regex(@"\d" + "ל`"));
            this.Regexes.Add(new Regex(@"\d " + "ל`"));
            this.Regexes.Add(new Regex(@"\d+ " + "ק\"ג"));
            this.Regexes.Add(new Regex(@"\d+\*+\d"));
            this.Regexes.Add(new Regex(@"\d+%"));
            this.Regexes.Add(new Regex(@"\d+"));
        }

        /*
         * This function return the quantity of a given product includes in his description in grams
         */
        public string getQuantityFromDescription(string productDescription)
        {
            string toReturnInGrams = "";
            foreach (Regex Regex in this.Regexes)
            {
                toReturnInGrams = Regex.Match(productDescription).Value;
                if (!toReturnInGrams.Equals(""))
                {
                    //if contains % return ""
                    //check it at the end
                    if (toReturnInGrams.Contains("%") || toReturnInGrams.Contains("קלוריות"))
                    {
                        return "";
                    }


                    //multiple num of product * 100
                    if (toReturnInGrams.Contains("יח"))
                    {
                        return Double.Parse(Regex.Replace(toReturnInGrams, "[^0-9.]", "")) * 100 + "";
                    }
                    //if is kg/liter
                    if (!toReturnInGrams.Contains("*") && (toReturnInGrams.Contains("קג") || toReturnInGrams.Contains("ק") || toReturnInGrams.Contains("ק\"ג") || toReturnInGrams.Contains("ק`ג") || toReturnInGrams.Contains("ק\"") || toReturnInGrams.Contains("ליטר") || ((toReturnInGrams.Contains("ל") || toReturnInGrams.Contains("ליטר") || toReturnInGrams.Contains("ל'") || toReturnInGrams.Contains("ל`")) && (!toReturnInGrams.Contains("מל") && !toReturnInGrams.Contains("מ\"ל")))))
                    {
                        //if contains "." than we need to multiple by 1000 to get correct result
                        if (toReturnInGrams.Contains("."))
                        {
                            return Double.Parse(Regex.Replace(toReturnInGrams, "[^0-9.]", "")) * 1000 + "";
                        }

                        //kg without "."
                        return new String(toReturnInGrams.Where(Char.IsDigit).ToArray()) + "000";
                    }

                    //contains "*"
                    else if (toReturnInGrams.Contains("*") || toReturnInGrams.Contains("x"))
                    {
                        //we multiple the two numbers
                        bool isNumber = false;
                        double cleanNumber1 = 0.0, cleanNumber2 = 0.0; ;
                        string[] splitted;
                        if (toReturnInGrams.Contains("*"))
                        {
                            splitted = toReturnInGrams.Split('*');
                        }
                        else
                        {
                            splitted = toReturnInGrams.Split('x');
                        }

                        isNumber = double.TryParse(Regex.Replace(splitted[0], "[^0-9.]", ""), out cleanNumber1);

                        if (!isNumber)
                        {
                            cleanNumber1 = Double.Parse(new String(splitted[0].Where(Char.IsDigit).ToArray()));
                        }

                        isNumber = double.TryParse(Regex.Replace(splitted[1], "[^0-9.]", ""), out cleanNumber2);

                        if (!isNumber)
                        {
                            cleanNumber2 = Double.Parse(new String(splitted[1].Where(Char.IsDigit).ToArray()));
                        }

                        //if kg/liter add "000"
                        if (toReturnInGrams.Contains("קג") || toReturnInGrams.Contains("ק\"ג") || toReturnInGrams.Contains("ק\"ג") || toReturnInGrams.Contains("ק\"") || toReturnInGrams.Contains("ליטר") || ((toReturnInGrams.Contains("ל") || toReturnInGrams.Contains("ליטר") || toReturnInGrams.Contains("ל'") || toReturnInGrams.Contains("ל`")) && (!toReturnInGrams.Contains("מל") && !toReturnInGrams.Contains("מ\"ל"))))
                        {
                            return cleanNumber1 * cleanNumber2 + "000";
                        }

                        //if not kg/liter
                        return cleanNumber1 * cleanNumber2 + "";
                    }
                    //not contains "." and not kg/liter
                    return new String(toReturnInGrams.Where(Char.IsDigit).ToArray());
                }
            }

            return toReturnInGrams;
        }
    }
}