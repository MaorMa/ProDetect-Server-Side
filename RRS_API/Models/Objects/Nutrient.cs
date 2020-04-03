using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Objects
{
    //This class represent nutrient, each nutrient contains code and value
    public class Nutrient
    {
        private string code;
        private double value;

        //C'tor
        public Nutrient(string code, double value)
        {
            this.Code = code;
            this.Value = value;
        }

        //Getters and Setters
        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public double Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }
    }
}