using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace RRS_API.Models.ImageRecognition
{
    /// <summary>
    /// This class responsible for saving the marked image in server. (images folder)
    /// </summary>
    public class MarkedImageSaver
    {
        private string MarkedImagesPath = System.Web.HttpContext.Current.Server.MapPath("~/Images/");

        /// <summary>
        /// This method save marked image in the server.
        /// using toStream auxilary method.
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="selectedFamilyID"></param>
        public void SaveMarkedImage(Receipt receipt, string selectedFamilyID)
        {
            Image markedImage = receipt.GetOriginalImage();
            var stream = ToStream(markedImage, ImageFormat.Jpeg);
            Image image = System.Drawing.Image.FromStream(stream);
            ImageCodecInfo jpgCodec = ImageCodecInfo.GetImageEncoders().Where(codec => codec.FormatID.Equals(ImageFormat.Jpeg.Guid)).FirstOrDefault();
            if (jpgCodec != null)
            {
                EncoderParameters parameters = new EncoderParameters();
                parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 24);
                image.Save(stream, jpgCodec, parameters);
            }
            string path = MarkedImagesPath + "\\" + selectedFamilyID;
            System.IO.Directory.CreateDirectory(path);
            Bitmap bm = new Bitmap(stream);
            bm.Save(path + "\\" + receipt.GetName());
        }

        /// <summary>
        /// This method create stream from image object.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private Stream ToStream(Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public string GetMarkedImagesPath()
        {
            return this.MarkedImagesPath;
        }
    }
}