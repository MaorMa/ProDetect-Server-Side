using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Controllers;
using RRS_API.Models;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Controllers.Tests
{
    [TestClass()]
    public class ReceiptMngrTests
    {
        ReceiptMngr rm = new ReceiptMngr();

        [TestMethod()]
        public void GetProductInfoTest()
        {
            List<string> answer = rm.GetProductInfo("22", "Shupersal");
            string[] description = answer[0].Split(',');
            Assert.AreEqual("עגבניה", description[1]);
        }

        [TestMethod()]
        public void GetProductInfoNotExistsTest()
        {
            List<string> answer = rm.GetProductInfo("1", "Shupersal");
            Assert.AreEqual(0, answer.Count);
        }

        [TestMethod()]
        public void GetAllNotApprovedFamilyDataTest()
        {
            string result = rm.GetAllNotApprovedFamilyData("blabla");
            Assert.AreEqual("[]", result);
        }


        [TestMethod()]
        public void ReturnReceiptToAcceptTest()
        {
            ReceiptToReturn rtr = new ReceiptToReturn("123","Shupersal","blabla","0","1212");
            rm.ReturnReceiptToAccept("Test", rtr);
        }

        [TestMethod()]
        public void GetAllApprovedFamilyNotExistDataTest()
        {
            string result = rm.GetAllNotApprovedFamilyData("blabla");
            Assert.AreEqual("[]", result);
        }

        [TestMethod()]
        public void GetProductDataWithOptionalNamesTest()
        {
            string result = rm.GetProductDataWithOptionalNames("22","Shupersal");
            string expected = "[{\"Key\":[\"7290000000022,עגבניה,5.90\"],\"Value\":[{\"sID\":\"74101000\",\"name\":\"עגבניה\",\"similarity\":\"1.3\"}]}]";
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void GetProductDataWithOptionalNames2Test()
        {
            string result = rm.GetProductDataWithOptionalNames("1", "Shupersal");
            string expected = "[]";
            Assert.AreEqual(expected, result);
        }


        [TestMethod()]
        public void DeleteReceiptTest()
        {
            bool result = rm.DeleteReceipt("2ABA57762954858E0557085981F3FC4C7024B1E8");
            Assert.IsTrue(result);
        }


        [TestMethod()]
        public void GetAllFamiliesByReceiptStatusTest()
        {
            string result = rm.GetAllFamiliesByReceiptStatus("Acc","Test",true,false);
            Assert.AreEqual("[\"Maor\"]", result);
        }


        /*





        [TestMethod()]
        public void UpdateReceiptDataTest()
        {
            Assert.Fail();
        }




        [TestMethod()]
        public void GetProductInfoTest()
        {
            Assert.Fail();
        }



        */




        /*
        * Integration
        [TestMethod()]
        public void GetAndInsertOptionalNamesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOptionalNamesTest()
        {
            List<ResearchProduct> l = rm.GetOptionalNames("עגבניה");
            Assert.AreNotEqual(0,l.Count);
        }

        [TestMethod()]
        public void ProcessReceiptsTest()
        {
            Assert.Fail();
        }
        */
    }
}