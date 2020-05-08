using ExcelDataReader;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Mangagers
{
    /// <summary>
    /// This class responsible for manage nutrients.
    /// </summary>
    public class NutrientMngr
    {
        private string[] codes = {"nut203","nut204","nut205","nut208","nut221","nut255","nut291","nut301","nut303","nut304",
        "nut305","nut306","nut307","nut309","nut312","nut318","nut320","nut321","nut323","nut401","nut404","nut405","nut406",
        "nut415","nut417","nut418","nut601","nut606","nut607","nut608","nut609","nut610","nut611","nut612","nut613","nut614",
        "nut617","nut618","nut619","nut620","nut621","nut622","nut623","nut628","nut625","nut630","nut631","nut645","nut646",
        "nut324","nut269","nut605"};

        /// <summary>
        /// This method create list of nutrients from given list of string.
        /// </summary>
        /// <param name="nuts"></param>
        /// <returns> List of Nutrients. </returns>
        public List<Nutrient> ToNutList(List<string> nuts)
        {
            List<Nutrient> toReturn = new List<Nutrient>();
            for(int i=2; i< nuts.Count; i++)
            {
                toReturn.Add(new Nutrient(codes[i-2], double.Parse(nuts[i])));
            }
            return toReturn;
        }

        /*
        public void updateNutrients(string path)
        {
            //delete old nutritnes
            DBConnection.DeleteNutrientsData();
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int i = 0;
                    do
                    {

                        while (reader.Read())
                        {
                            string nutrients = "";
                            if (i == 0)
                            {
                                i++; 
                                continue;
                            }
                            string foodCode = ((int)reader.GetDouble(0)).ToString();
                            string foodName = reader.GetString(1);
                            int j = 2;
                            for(; j < 53; j++)
                            {
                                nutrients += reader.GetDouble(j) + ",";
                            }
                            nutrients += reader.GetDouble(j);
                            DBConnection.InsertNewNutrient(foodCode,foodName,nutrients);
                        }
                    } while (reader.NextResult());

                }
            }

        }
        */
    }
}