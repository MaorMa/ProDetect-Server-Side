using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models.Mangagers;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Mangagers.Tests
{
    [TestClass()]
    public class NutrientMngrTests
    {
        NutrientMngr nm = new NutrientMngr();
        [TestMethod()]
        public void ToNutListTest()
        {
            List<string> test = new List<string>() {"22","22","1","1"};
            var expected = new List<Nutrient>() {new Nutrient("nut203",1),new Nutrient("nut204",1)};
            var result = nm.ToNutList(test);
            Assert.AreEqual(expected[0].Code, result[0].Code);
            Assert.AreEqual(expected[0].Value, result[0].Value);
            Assert.AreEqual(expected[1].Code, result[1].Code);
            Assert.AreEqual(expected[1].Value, result[1].Value);
        }
    }
}