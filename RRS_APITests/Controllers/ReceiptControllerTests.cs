using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RRS_API.Controllers.Tests
{
    [TestClass()]
    public class ReceiptControllerTests
    {
        ReceiptController receiptController = new ReceiptController
        {
            Request = new System.Net.Http.HttpRequestMessage(),
            Configuration = new HttpConfiguration()
        };


        [TestMethod()]
        public void GetProductInfoTest()
        {
            HttpResponseMessage result = receiptController.GetProductInfo("Shupersal", "22");
            var status = result.StatusCode;
            Assert.AreEqual(HttpStatusCode.OK, status);
        }
    }
}