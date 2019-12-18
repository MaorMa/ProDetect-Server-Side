using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Objects
{
    public class Nutrient
    {
        private string code;
        private double value;

        public Nutrient(string code, double value)
        {
            this.Code = code;
            this.Value = value;
        }

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