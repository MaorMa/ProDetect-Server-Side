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
    public class StatisticsMngrTests
    {
        StatisticsMngr sm = new StatisticsMngr();
        [TestMethod()]
        public void GetAllPricesByCategoriesNullTest()
        {
            string result = sm.GetAllPricesByCategories(null);
            Assert.AreEqual("[]", result);
        }

        [TestMethod()]
        public void GetAllQuantitiesByCategoriesTest()
        {
            string result = sm.GetAllQuantitiesByCategories(null);
            Assert.AreEqual("[]", result);
        }

        [TestMethod()]
        public void GetCompareByCostTest()
        {
            string result = sm.GetCompareByCost(null);
            Assert.AreEqual("[]", result);
        }
    }
}