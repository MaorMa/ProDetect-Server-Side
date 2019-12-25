using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RRS_API.Models.Objects
{
    public class ResearchProduct
    {
        private string sID;
        private string name;

        public string SID
        {
            get
            {
                return sID;
            }

            set
            {
                sID = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public ResearchProduct(string sID, string name)
        {
            this.SID = sID;
            this.Name = name;
        }

        /*public int CompareTo(ResearchProduct other)
        {
            if (this.SID.Equals(other.SID)) return 0;
            return 1;
        }*/

        public override bool Equals(object y)
        {
            return this.SID.Equals(((ResearchProduct)y).SID);
        }

        public override int GetHashCode()
        {
            return this.SID.GetHashCode();
        }
    }
}