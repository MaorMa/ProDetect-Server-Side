using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models;
using RRS_API.Models.Mangagers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Tests
{
    [TestClass()]
    public class SettingsMngrTests
    {
        SettingsMngr sm = new SettingsMngr();

        [TestMethod()]
        public void GetMarketsTest()
        {
            List<string> markets = sm.GetMarkets();
            List<string> expected = new string[] { "אושר עד", "ויקטורי", "חצי חינם", "טיב טעם", "יוחננוף", "ינות ביתן", "יש", "מגה", "מחסני השוק", "מחסני להב", "פרש מרקט", "קינג סטור", "קשת טעמים", "רמי לוי", "שופרסל", "שוק העיר" }.ToList();
            CollectionAssert.AreEqual(expected, markets, StructuralComparisons.StructuralComparer);
        }

        [TestMethod()]
        public void GetFamiliesTest()
        {
            List<string> families = sm.GetFamilies();
            Assert.IsTrue(families.Count > 1);
        }
    }
}