using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Controllers;
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

        /*


        [TestMethod()]
        public void GetAllFamiliesByReceiptStatusTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllNotApprovedFamilyDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateReceiptDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ReturnReceiptToAcceptTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAllApprovedFamilyDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetProductInfoTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetProductDataWithOptionalNamesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteReceiptTest()
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