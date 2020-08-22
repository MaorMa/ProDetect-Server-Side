using Microsoft.VisualStudio.TestTools.UnitTesting;
using RRS_API.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRS_API.Models.Tests
{
    [TestClass()]
    public class PreProcessingTests
    {
        static Bitmap original = new Bitmap(@"C:\Users\Maor\PycharmProjects\checkBlurry\high\DONE\22\22.jpeg");
        PreProcessing pp = new PreProcessing(original);
        [TestMethod()]
        public void FlipTest()
        {
            var transformed = pp.Flip(original);
            Assert.IsTrue(transformed.Height > transformed.Width);
        }

        [TestMethod()]
        public void GetMode1Test()
        {
            var mode_1_transformed = pp.GetMode1();
            Assert.IsNotNull(mode_1_transformed);
        }

        [TestMethod()]
        public void GetMode2Test()
        {
            var mode_2_transformed = pp.GetMode2();
            Assert.IsNotNull(mode_2_transformed);
        }

        [TestMethod()]
        public void GetMode3Test()
        {
            var mode_3_transformed = pp.GetMode3();
            Assert.IsNotNull(mode_3_transformed);
        }

        [TestMethod()]
        public void GetMode4Test()
        {
            var mode_4_transformed = pp.GetMode4();
            Assert.IsNotNull(mode_4_transformed);
        }

        [TestMethod()]
        public void GetMode5Test()
        {
            var mode_5_transformed = pp.GetMode5();
            Assert.IsNotNull(mode_5_transformed);
        }

        [TestMethod()]
        public void GetMode6Test()
        {
            var mode_6_transformed = pp.GetMode6();
            Assert.IsNotNull(mode_6_transformed);
        }

        [TestMethod()]
        public void GetImageInNewResolutionTest()
        {
            var new_resolution_transformed = pp.GetImageInNewResolution();
            Assert.IsNotNull(new_resolution_transformed);
        }
    }
}