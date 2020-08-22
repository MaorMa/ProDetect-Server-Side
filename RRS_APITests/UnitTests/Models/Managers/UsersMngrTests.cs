using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models.Mangagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Mangagers.Tests
{
    [TestClass()]
    public class UsersMngrTests
    {
        UsersMngr um = new UsersMngr();
        [TestMethod()]

        public void AddNewFamilyUserExistsTest()
        {
            try
            {
                um.AddNewFamilyUser("Test", "Test666666");
            }
            catch (Exception exception)
            {
                Assert.AreEqual("User already exists", exception.Message);
            }
        }

        /*
        [TestMethod()]
        public void AddNewFamilyUserNotExistsTest()
        {
            try
            {
                um.AddNewFamilyUser("Test2", "Test666666");
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
        */

    }
}