using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests
{
    [TestClass()]
    public class OcrProcessingTests
    {

        [TestMethod()]
        public void FromImagesToTextTest()
        {

            OcrProcessing op = new OcrProcessing();
            Dictionary<string, Image> dic = new Dictionary<string, Image>();
            dic.Add("1", new Bitmap(@"C:\\Users\\Maor\\Desktop\\test\\test.png"));
            List<Receipt> l = op.FromImagesToText(dic);
            Assert.AreNotEqual(0,l[0].GetWordsList().Count);
        }

    }
}