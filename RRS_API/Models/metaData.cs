using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition.Objects
{
    public class metaData
    {
        String description;//item description-name
        String quantity;
        String price;//per unit from DB

        public metaData(String description, String quantity, String price)
        {
            this.description = description;
            this.quantity = quantity;
            this.price = price;
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
    }
}
