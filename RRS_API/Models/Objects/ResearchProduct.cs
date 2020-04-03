using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Objects
{
    public class ResearchProduct
    {
        //Fields
        public string sID { get; set; }
        public string name { get; set; }
        public string similarity { get; set; }

        //C'tor
        public ResearchProduct(string sID, string name, string similarity)
        {
            this.sID = sID;
            this.name = name;
            this.similarity = similarity;
        }

        public override bool Equals(object y)
        {
            return this.sID.Equals(((ResearchProduct)y).sID);
        }

        public override int GetHashCode()
        {
            return this.sID.GetHashCode();
        }
    }
}