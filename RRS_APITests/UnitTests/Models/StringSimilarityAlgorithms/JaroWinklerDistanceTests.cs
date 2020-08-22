using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models.StringSimilarityAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.StringSimilarityAlgorithms.Tests
{
    [TestClass()]
    public class JaroWinklerDistanceTests
    {
        JaroWinklerDistance jwd = new JaroWinklerDistance();

        [TestMethod()]
        public void GetTopFiveSimilarProductsTest()
        {
            var result = jwd.GetTopFiveSimilarProducts(null);
            Assert.AreEqual(0,result.Count);
        }

        [TestMethod()]
        public void GetTopFiveSimilarProducts2Test()
        {
            var result = jwd.GetTopFiveSimilarProducts("גבינה בולגרית 5% משקל");
            Assert.IsTrue(result.Count > 0);
        }
    }
}