﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models;
using RRS_API.Models.Mangagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Tests
{
    [TestClass()]
    public class DBConnectionTests
    {
        DBConnection DB = DBConnection.GetInstance();

        [TestMethod()]
        public void UpdateStatusTest()
        {
            DB.UpdateStatus("Maor", "2ABA57762954858E0557085981F3FC4C7024B1E8", "-1");
            DB.UpdateStatus("Maor", "2ABA57762954858E0557085981F3FC4C7024B1E8", "0");
            List<string> updatedStatus = DB.SelectQuery("select ReceiptStatus from FamilyUploads where ReceiptID='2ABA57762954858E0557085981F3FC4C7024B1E8'");
            Assert.AreEqual("0", updatedStatus[0]);
        }


        [TestMethod()]
        public void UpdateFamilyUploadsTest()
        {
            DB.SelectQuery("delete from FamilyUploads where ReceiptID='111'");
            DB.UpdateFamilyUploads("Maor", "Shupersal", "111", -1, "2020-05-07 16:30:03.000");
            List<string> receipt = DB.SelectQuery("select ReceiptID from FamilyUploads where ReceiptID='111'");

            Assert.AreEqual("111",receipt[0]);
        }


        [TestMethod()]
        public void InsertReceiptDataTest()
        {
            DB.DeleteReceiptData("111");
            DB.InsertReceiptData("Maor","111","22","עגבניה","1","1","3.90",100,true);
            List<string> result = DB.SelectQuery("select ProductID from ReceiptData where ReceiptID='111'");
            Assert.AreEqual("22", result[0]);
        }


        [TestMethod()]
        public void DeleteReceiptDataTest()
        {
            DB.DeleteReceiptData("111");
            List<string> result = DB.SelectQuery("select ReceiptID from ReceiptData where ReceiptID='111'");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod()]
        public void DeleteFamilyReceiptTest()
        {
            DB.DeleteFamilyReceipt("111");
            List<string> receipt = DB.SelectQuery("select ReceiptID from FamilyUploads where ReceiptID='111'");

            Assert.AreEqual(0, receipt.Count);
        }

        [TestMethod()]
        public void InsertOptionalProductTest()
        {
            DB.SelectQuery("delete from OptionalProducts where ProductID='1'");
            DB.InsertOptionalProduct("Shufersal","1", new Objects.ResearchProduct("22","עגבניה","1"));
            List<string> receipt = DB.SelectQuery("select ProductID from OptionalProducts where ProductID='1'");
            Assert.AreEqual(1, receipt.Count);
        }

        [TestMethod()]
        public void DeleteOptionalDataTest()
        {
            DB.DeleteOptionalData("Shufersal", "1");
            List<string> receipt = DB.SelectQuery("select ProductID from OptionalProducts where ProductID='1'");
            Assert.AreEqual(0, receipt.Count);
        }

        [TestMethod()]
        public void GetSaltValueTest()
        {
            string result = DB.GetSaltValue("Maor");
            Assert.AreEqual(result, "62tZNRNU/52fjHqAojKnrSDOLJrbBjzmWNWZkHFG7HY=");
        }

        [TestMethod()]
        public void GetGroupIDTest()
        {
            string res = DB.GetGroupID("Test");
            Assert.AreEqual("True",res);
        }


        /*
        [TestMethod()]
        public void GetInstanceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SelectQueryTest()
        {
            Assert.Fail();
        }








        [TestMethod()]
        public void DeleteOptionalDataTest()
        {
            Assert.Fail();
        }



        [TestMethod()]
        public void CheckUsernameAndPasswordTest()
        {
            Assert.Fail();
        }



        [TestMethod()]
        public void GetFamiliesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetFamiliesWithApprovedDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddNewFamilyUserTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSimiliarProductNamesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertOptionalProductsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertOptionalProductTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteNutrientsDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertNewNutrientTest()
        {
            Assert.Fail();
        }
        */


        /*
         * Integration
         */
        [TestMethod()]
        public void CheckUsernameAndPasswordTest()
        {
            PasswordMngr pm = new PasswordMngr();
            var salt_value = DB.GetSaltValue("Test");
            var salt_bytes = Encoding.UTF8.GetBytes(salt_value);
            var password_bytes = Encoding.UTF8.GetBytes("Test666666");
            string hashedPass = pm.Sha256Encription(password_bytes, salt_bytes);
            bool t = DB.CheckUsernameAndPassword("Test", hashedPass);
        }
    }
}