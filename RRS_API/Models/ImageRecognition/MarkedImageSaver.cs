using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace RRS_API.Models.ImageRecognition
{
    public class MarkedImageSaver
    {
        private string MarkedImagesPath = System.Web.HttpContext.Current.Server.MapPath("~/Images/");

        public void saveMarkedImage(Receipt receipt, string selectedFamilyID)
        {
            Image markedImage = receipt.getOriginalImage();
            var stream = ToStream(markedImage, ImageFormat.Jpeg);
            Image image = System.Drawing.Image.FromStream(stream);
            ImageCodecInfo jpgCodec = ImageCodecInfo.GetImageEncoders().Where(codec => codec.FormatID.Equals(ImageFormat.Jpeg.Guid)).FirstOrDefault();
            if (jpgCodec != null)
            {
                EncoderParameters parameters = new EncoderParameters();
                parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 24); //8, 16, 24, 32 (base on your format)
                image.Save(stream, jpgCodec, parameters);
            }
            string path = MarkedImagesPath + "\\" + selectedFamilyID;
            System.IO.Directory.CreateDirectory(path);
            Bitmap bm = new Bitmap(stream);
            bm.Save(path + "\\" + receipt.getName());
        }

        //move to FilesHandler class
        private Stream ToStream(Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public string getMarkedImagesPath()
        {
            return this.MarkedImagesPath;
        }
    }
}