using ImageRecognition.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcrProject.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrProject.Parser.Tests
{
    [TestClass()]
    public class OcrTextParserTests
    {
        OcrTextParser otp = new OcrTextParser();
        Receipt r = new Receipt(1000, 1500, "test", null, null);
        string kg = "ק\"ג";
        [TestMethod()]
        public void ParsingTest()
        {
            r.AddRow("22 עגבניה");
            r.AddRow("1.2 " + kg + " x 1.2 5.9 ");
            otp.Parsing(r);
            List<MetaData> value = r.GetIdToMetadata().ElementAt(0).Value;
            MetaData metaData = value.ElementAt(0);
            string quantity = metaData.getQuantity();
            string id = metaData.getsID();
            string expected = "ID=22,Quantity=1.2";
            string result = "ID="+ id + ",Quantity="+quantity;
            Assert.AreEqual(result,expected);
        }

        [TestMethod()]
        public void GetAllRecieptsDataTest()
        {
            r.AddRow("22 עגבניה");
            r.AddRow("1.2 " + kg + " x 1.2 5.9 ");
            List<Receipt> list = new List<Receipt>();
            list.Add(r);
            otp.GetAllRecieptsData(list);
            List<MetaData> value = r.GetIdToMetadata().ElementAt(0).Value;
            MetaData metaData = value.ElementAt(0);
            string quantity = metaData.getQuantity();
            string id = metaData.getsID();
            Assert.AreEqual(quantity, "1.2");
        }
    }
}