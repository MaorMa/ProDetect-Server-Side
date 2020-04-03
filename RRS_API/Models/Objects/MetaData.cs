using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;

namespace ImageRecognition.Objects
{
    //This class responsibe for represent all the metadata of each product
    public class MetaData
    {
        //Fields
        public string sID;
        public string description;
        public string quantity;
        public string price; //per unit
        public bool validProduct;
        private double yCoordinate;
        public List<Nutrient> nutrients;
        public List<ResearchProduct> optionalProducts;
        public ResearchProduct optionalProductsChosen;

        //C'tor
        public MetaData(string sID, string description, string quantity, string price, double yCoordinate, bool validProduct)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
            this.validProduct = validProduct;
        }

        // Getters

        public string getDescription()
        {
            return this.description;
        }

        public string getQuantity()
        {
            return this.quantity;
        }

        public string getPrice()
        {
            return this.price;
        }

        public double getyCoordinate()
        {
            return this.yCoordinate;
        }

        public bool getvalidProduct()
        {
            return this.validProduct;
        }

        public List<ResearchProduct> getOptionalProducts()
        {
            return this.optionalProducts;
        }

        public ResearchProduct getOptionalProductsChosen()
        {
            return this.optionalProductsChosen;
        }

        // Setters

        public void setDescription(string description)
        {
            this.description = description;
        }

        public void setQuantity(string quantity)
        {
            this.quantity = quantity;
        }

        public void setPrice(string price)
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

        public void setvalidProduct(bool validProduct)
        {
            this.validProduct = validProduct;
        }

        public void setOptionalProducts(List<ResearchProduct> optionalProducts)
        {
            this.optionalProducts = optionalProducts;
        }

        public void setOptionalProductsChosen(ResearchProduct optionalProducts)
        {
            this.optionalProductsChosen = optionalProducts;
        }
    }
}
