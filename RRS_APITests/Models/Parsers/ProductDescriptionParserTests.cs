using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Parsers.Tests
{
    [TestClass()]
    public class ProductDescriptionParserTests
    {
        [TestMethod()]
        public void GetQuantityFromDescriptionGramsTest()
        {
            ProductDescriptionParser pdp = new ProductDescriptionParser();
            Assert.AreEqual(pdp.GetQuantityFromDescription("200 גרם"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200 ג"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200 גר"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200 גר'"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200 גר`"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200גרם"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200גר"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200גר'"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200גר`"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200ג"), "200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("6*200 ג"), "1200");
            Assert.AreEqual(pdp.GetQuantityFromDescription("200+50ג"), "250");
            Assert.AreEqual(pdp.GetQuantityFromDescription("במבה אסם (25x10 גרם)"), "250");
            Assert.AreEqual(pdp.GetQuantityFromDescription("שניצל תירס 750גרם%33+"), "750");
            Assert.AreEqual(pdp.GetQuantityFromDescription("עוגת בראוניס כשלפ430גרם"), "430");
            Assert.AreEqual(pdp.GetQuantityFromDescription("חטיפי טוויקס 4+1 50*5ג	"), "250");
            Assert.AreEqual(pdp.GetQuantityFromDescription("גבינה עיזים גדעז 20% גד (180 גרם)"), "180");


        }

        [TestMethod()]
        public void GetQuantityFromDescriptionKGTest()
        {
            ProductDescriptionParser pdp = new ProductDescriptionParser();
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ק\"ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ק`ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ק\"ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ק'ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ק`ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1קג'"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1קג`"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1קג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 קג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ק'ג"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 קג'"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 קג`"), "1000");
        }

        [TestMethod()]
        public void GetQuantityFromDescriptionByUnitTest()
        {
            ProductDescriptionParser pdp = new ProductDescriptionParser();
            Assert.AreEqual(pdp.GetQuantityFromDescription("4 יח"), "400");
            Assert.AreEqual(pdp.GetQuantityFromDescription("4 יח'"), "400");
            Assert.AreEqual(pdp.GetQuantityFromDescription("4 יח`"), "400");
        }

        [TestMethod()]
        public void GetQuantityFromDescriptionMLTest()
        {
            ProductDescriptionParser pdp = new ProductDescriptionParser();
            Assert.AreEqual(pdp.GetQuantityFromDescription("100 מ\"ל"), "100");
            Assert.AreEqual(pdp.GetQuantityFromDescription("100 מל"), "100");
            Assert.AreEqual(pdp.GetQuantityFromDescription("שמנת לקצפת השף 250 מל 32 % *"), "250");
            Assert.AreEqual(pdp.GetQuantityFromDescription("לחם שיפון פרוס כפרי אחדות (750 גרם)"), "750");
            Assert.AreEqual(pdp.GetQuantityFromDescription("100מ\"ל"), "100");
            Assert.AreEqual(pdp.GetQuantityFromDescription("100מל"), "100");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ליטר"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ל"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ל'"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1 ל`"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ליטר"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ל'"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ל`"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1ל"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1.5*6 לי"), "9000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1.5*6 ל"), "9000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1.5*6 ל'"), "9000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("1.5*6 ל`"), "9000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2*6ל"), "12000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2*6ל'"), "12000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2*220מל"), "440");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2*220מל'"), "440");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2* 220מל'"), "440");
            Assert.AreEqual(pdp.GetQuantityFromDescription("2*6ל`"), "12000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("דיאט פאנטה אורנג. 1.50ליטר"),"1500");
            Assert.AreEqual(pdp.GetQuantityFromDescription("4* 1.5 לי"), "6000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("חלב תנובה טרי מהדרין כד %3 2ל"), "2000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("חלב תנובה טרי הומוגני 3% 1.5 ליטר"), "1500");
            Assert.AreEqual(pdp.GetQuantityFromDescription("שמן קנולה שופרסל 1 ליטר"), "1000");
            Assert.AreEqual(pdp.GetQuantityFromDescription("דיאט פאנטה אורנג. 1.50ליטר"), "1500");
            Assert.AreEqual(pdp.GetQuantityFromDescription("דיאט פאנטה אורנג. 1.50ליטר"), "1500");
        }
    }
}