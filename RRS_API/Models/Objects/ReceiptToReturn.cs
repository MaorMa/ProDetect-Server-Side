using ImageRecognition.Objects;
using RRS_API.Models.Objects;
using System.Collections.Generic;

namespace RRS_API.Models
{
    public class ReceiptToReturn
    {
        public string receiptID { get; set; }
        public string marketID { get; set; }
        public string image { get; set; }
        public string status { get; set; }
        public string uploadTime { get; set; }

        public List<MetaData> products { get; set; }


        public ReceiptToReturn(string receiptID, string marketID, string image, string status,string uploadTime)
        {
            this.receiptID = receiptID;
            this.marketID = marketID;
            this.image = image;
            this.products = new List<MetaData>();
            this.status = status;
            this.uploadTime = uploadTime;
        }

        public void updateProducts(List<MetaData> products)
        {
            this.products = products;
        }
        
        public void addProduct(string productID, string Description, string Quantity, string price, double yCoordinate, bool validProduct, List<string> nutrient)
        {
            var meta = new MetaData(productID, Description, Quantity, price, yCoordinate, validProduct);
            meta.nutrients = nutrient;
            products.Add(meta);
        }

        public void addProduct(string productID,string Description, string Quantity, string price, double yCoordinate,bool validProduct, List<ResearchProduct> optionalProducts)
        {
            var meta = new MetaData(productID, Description, Quantity, price, yCoordinate, validProduct);
            meta.optionalProducts = optionalProducts;
            products.Add(meta);
        }
    }
}