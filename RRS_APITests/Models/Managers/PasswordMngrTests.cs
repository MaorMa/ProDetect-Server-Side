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
    public class PasswordMngrTests
    {
        PasswordMngr pm = new PasswordMngr();
        [TestMethod()]
        public void GenerateSaltTest()
        {
            try { 
                byte[] randomSalt = pm.GenerateSalt();
            }
            catch(Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod()]
        public void Sha256EncriptionTest()
        {
            try
            {
                byte[] pass = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
                byte[] salt = new byte[] { 0x21, 0x22, 0x23, 0x20, 0x20, 0x20, 0x20 };
                string result = pm.Sha256Encription(pass, salt);
            }catch(Exception e)
            {
                Assert.Fail();
            }
        }
    }
}