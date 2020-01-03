﻿using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;

namespace ImageRecognition.Objects
{
    public class MetaData
    {
        public string sID;
        public string description;//item description-name
        public string quantity;
        public string price;//per unit from DB
        public Boolean validProduct;
        private double yCoordinate;
        public List<Nutrient> nutrients;
        public List<ResearchProduct> optionalProducts;
        public ResearchProduct optionalProductsChosen;

        /*public MetaData(string sID, string description, string quantity, string price, double yCoordinate, bool validProduct, List<ResearchProduct> optionalProducts, ResearchProduct optionalProductsChosen)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
            this.validProduct = validProduct;
            this.optionalProducts = optionalProducts;
            this.optionalProductsChosen = optionalProductsChosen;
        }*/

        public MetaData(string sID, string description, string quantity, string price, double yCoordinate, bool validProduct)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
            this.validProduct = validProduct;
        }

        /*public MetaData(string sID,string description, string quantity, string price,double yCoordinate, bool validProduct, List<ResearchProduct> optionalProducts)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
            this.validProduct = validProduct;
            this.optionalProducts = optionalProducts;
        }

        public MetaData(string sID, string description, string quantity, string price, double yCoordinate, bool validProduct, List<string> nutrients)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
            this.sID = sID;
            this.yCoordinate = yCoordinate;
            this.validProduct = validProduct;
                
        }/*

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

        /*public void setNutrients(List<string> nutrients)
        {
            this.nutrients = nutrients;
        }*/
    }
}
