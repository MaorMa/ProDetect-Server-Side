using System;

namespace ImageRecognition.Objects
{
    public class MetaData
    {
        public string sID;
        public string description;//item description-name
        public string quantity;
        public string price;//per unit from DB
        private double yCoordinate;

        public MetaData(string sID,string description, string quantity, string price,double yCoordinate)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
        }
        
        /*
         * Getters
         */ 

        public String getDescription()
        {
            return this.description;
        }

        public String getQuantity()
        {
            return this.quantity;
        }

        public String getPrice()
        {
            return this.price;
        }

        public double getyCoordinate()
        {
            return this.yCoordinate;
        }

        /*
         * Setters
         */

        public void setDescription(String description)
        {
            this.description = description;
        }

        public void setQuantity(String quantity)
        {
            this.quantity = quantity;
        }

        public void setPrice(String price)
        {
            this.price = price;
        }

        public void setYcoordinate(double ycoordiante)
        {
            this.yCoordinate = ycoordiante;
        }

        public string getsID()
        {
            return this.sID;
        }
    }
}
