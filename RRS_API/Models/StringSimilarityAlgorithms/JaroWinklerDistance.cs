using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.StringSimilarityAlgorithms
{
    public class JaroWinklerDistance
    {
        protected DBConnection AzureConnection = DBConnection.getInstance();
        protected HashSet<string> wordsToIgnoreInProductDesc = new HashSet<string>();

        public JaroWinklerDistance()
        {
            this.wordsToIgnoreInProductDesc.Add("גרם");
            this.wordsToIgnoreInProductDesc.Add("גר");
            this.wordsToIgnoreInProductDesc.Add("ג");
            this.wordsToIgnoreInProductDesc.Add("קג");
            this.wordsToIgnoreInProductDesc.Add("ליטר");
            this.wordsToIgnoreInProductDesc.Add("ל");
            this.wordsToIgnoreInProductDesc.Add("מל");
            this.wordsToIgnoreInProductDesc.Add("יח");
            this.wordsToIgnoreInProductDesc.Add("יח'");
            this.wordsToIgnoreInProductDesc.Add("יחידה");
            this.wordsToIgnoreInProductDesc.Add("ק\"ג");
        }
        private readonly double mWeightThreshold = 0.7;

        /* Size of the prefix to be concidered by the Winkler modification. 
         * Winkler's paper used a default value of 4
         */
        private readonly int mNumChars = 4;

        /*
         * Return top 5 similar products (List of ResearchProduct objects) to productName
         */
        public List<ResearchProduct> getTopFiveSimilarProducts(string productName)
        {
            string[] words = productName.Split(' ', '.', '-');
            List<string> newProductName = new List<string>();

            for (int i = 0; i < words.Length && i < 2; i++)
            {
                newProductName.Add(words[i]);
            }

            string firstWord = words[0];
            string secondWord = "";
            if (words.Length > 1)
            {
                secondWord = words[1];
            }
            //Dictionary<ResearchProduct, double> topProductDic = new Dictionary<ResearchProduct, double>();
            HashSet<ResearchProduct> topProductSet= new HashSet<ResearchProduct>();
            foreach (string word in newProductName)
            {
                if (this.wordsToIgnoreInProductDesc.Contains(word))
                    continue;
                Dictionary<string, string> similarProducts = AzureConnection.getSimiliarProductNames(word);
                foreach (KeyValuePair<string, string> entry in similarProducts)
                {
                    double bonus = 0;
                    if (checkIfContainsWord(entry.Value,firstWord) || checkIfContainsWord(entry.Value, secondWord))
                    {
                        if (checkIfContainsWord(entry.Value, firstWord))
                        {
                            bonus += 0.3;
                        }

                        if (!secondWord.Equals("") && checkIfContainsWord(entry.Value, secondWord))
                        {
                            bonus += 0.2;
                        }
                        double similarity = distance(productName, entry.Value) + bonus;
                        var rp = new ResearchProduct(entry.Key, entry.Value, similarity+"");
                        if (topProductSet.Contains(rp) || similarity < 0.55)
                            continue;
                        topProductSet.Add(rp);
                    }
                }
            }
            List<ResearchProduct> topProductList = topProductSet.ToList();
            topProductList.Sort((element1,element2) => Double.Parse(element2.similarity).CompareTo(Double.Parse(element1.similarity)));
            List<ResearchProduct> toReturn = new List<ResearchProduct>();
            for (int i = 0; i < Math.Min(5, topProductList.Count); i++)
            {
                toReturn.Add(topProductList.ElementAt(i));
            }
            return toReturn;
        }


        private bool checkIfContainsWord(string value,string word)
        {
            /*
            string[] seperated = value.Split(' ');
            foreach(string s in seperated)
            {
                if (s.Equals(word))
                {
                    return true;
                }
            }
            return false;
            */
            return value.Contains(word);
        }


        /// <summary>
        /// Returns the Jaro-Winkler distance between the specified  
        /// strings. The distance is symmetric and will fall in the 
        /// range 0 (perfect match) to 1 (no match). 
        /// </summary>
        /// <param name="aString1">First String</param>
        /// <param name="aString2">Second String</param>
        /// <returns></returns>
        public double distance(string aString1, string aString2)
        {
            return proximity(aString1, aString2);
        }

        public double proximity(string aString1, string aString2)
        {
            int lLen1 = aString1.Length;
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;

            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];

            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;
            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                             + lNumCommonD / lLen2
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            if (lWeight <= mWeightThreshold) return lWeight;
            int lMax = Math.Min(mNumChars, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);
        }
    }
}