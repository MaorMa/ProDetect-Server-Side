using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RRS_API.Models
{
    public class ReceiptToReturn
    {
        public string receiptID { get; set; }
        public string image { get; set; }
        public List<MetaData> products { get; set; }


        public ReceiptToReturn(string receiptID, string image)
        {
            this.receiptID = receiptID;
            this.image = image;
            this.products = new List<MetaData>();
        }

        public void updateProducts(List<MetaData> products)
        {
            this.products = products;
        }
        public void addProduct(string productID,string Description, string Quantity, string price, double yCoordinate)
        {
            products.Add(new MetaData(productID,Description,Quantity,price,yCoordinate));
        }
    }
}