using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using log4net;
using System.Reflection;
using AForge.Imaging.Filters;
using System.Linq;

namespace RRS_API.Models
{
     /// <summary>
     /// This class responsilbe for image pre processing to optimize products detection by ocr. 
     /// </summary>
    public class PreProcessing
    {
        private Bitmap imgInNewResolution;
        List<Bitmap> modes; //mode1 - improve resolution, mode2 - grayscale, mode3 - sharpen image
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PreProcessing(Image image)
        {
            _logger.Debug($"Creating ImageProccessing");
            try
            {
                modes = new List<Bitmap>();
                Flip(image);
                this.imgInNewResolution = ImproveResolution(new Bitmap(image));
                modes.Add(ConvertToGrayscale(new Bitmap(image)));
                modes.Add(BinarizeAndDilationWithMedian(new Bitmap(imgInNewResolution)));
                modes.Add(BinarizeAndDilationNoMedian(new Bitmap(image)));
                modes.Add(PosterizationAndBinarize(new Bitmap(image)));
                modes.Add(Binarize(new Bitmap(image)));
                modes.Add(Sharpen(new Bitmap(image)));
            }
            catch (Exception e)
            {
                _logger.Error($"Error - creating ImageProccessing", e);
            }
        }

        /// <summary>
        /// This method improve image resolution to 300X300.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap ImproveResolution(Bitmap image)
        {
            //improve resolution
            Bitmap b = new Bitmap(image);
            b.SetResolution(300, 300);
            Median median = new Median();
            median.ApplyInPlace(b);
            //b.Save(@"C:\Users\Maor\Desktop\mode\ImproveResolution.jpeg");
            return b;
        }

        /// <summary>
        /// This method convert image to grayscale.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap ConvertToGrayscale(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
           //grayScale.Save(@"C:\Users\Maor\Desktop\mode\grayScale.jpeg");
            return grayScale;
        }

        /// <summary>
        /// This method binarize dnd dilate image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap BinarizeAndDilationWithMedian(Bitmap image)
        {
            int[,] kernel = {
            { -2, -1,  0 },
            { -1,  1,  1 },
            {  0,  1,  2 } };
            // create filter
            Convolution Convolution = new Convolution(kernel);
            // apply the filter
            Convolution.ApplyInPlace(image);

            var bmp8bpp = Grayscale.CommonAlgorithms.BT709.Apply(image);

            Invert invert = new Invert();
            invert.ApplyInPlace(bmp8bpp);

            Dilatation dilatation = new Dilatation();
            dilatation.ApplyInPlace(bmp8bpp);

            invert.ApplyInPlace(bmp8bpp);

            Median median = new Median();
            median.ApplyInPlace(bmp8bpp);

            Closing closing = new Closing();
            closing.ApplyInPlace(bmp8bpp);

            OtsuThreshold OtsuThreshold = new OtsuThreshold();
            OtsuThreshold.ApplyInPlace(bmp8bpp);
            //bmp8bpp.Save(@"C:\Users\Maor\Desktop\mode\BinarizeAndDilationWithMedian.jpeg");
            return bmp8bpp;
        }


        private Bitmap BinarizeAndDilationNoMedian(Bitmap image)
        {
            int[,] kernel = {
            { -2, -1,  0 },
            { -1,  1,  1 },
            {  0,  1,  2 } };
            // create filter
            Convolution Convolution = new Convolution(kernel);
            // apply the filter
            Convolution.ApplyInPlace(image);

            var bmp8bpp = Grayscale.CommonAlgorithms.BT709.Apply(image);

            Invert invert = new Invert();
            invert.ApplyInPlace(bmp8bpp);

            Dilatation dilatation = new Dilatation();
            dilatation.ApplyInPlace(bmp8bpp);

            invert.ApplyInPlace(bmp8bpp);

            Closing closing = new Closing();
            closing.ApplyInPlace(bmp8bpp);

            OtsuThreshold OtsuThreshold = new OtsuThreshold();
            OtsuThreshold.ApplyInPlace(bmp8bpp);
            //bmp8bpp.Save(@"C:\Users\Maor\Desktop\mode\BinarizeAndDilationNoMedian.jpeg");
            return bmp8bpp;
        }

        /// <summary>
        /// This method posterize and binarize image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap PosterizationAndBinarize(Bitmap image)
        {
            image = SimplePosterization(image);
            image = BinarizeAndDilationWithMedian(image);
            //image.Save(@"C:\Users\Maor\Desktop\mode\PosterizationAndBinarize.jpeg");
            return image;
        }

        /// <summary>
        /// This method binarize image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap Binarize(Bitmap image)
        {
            Median median = new Median();
            median.ApplyInPlace(image);

            var bmp8bpp = Grayscale.CommonAlgorithms.BT709.Apply(image);

            OtsuThreshold OtsuThreshold = new OtsuThreshold();
            OtsuThreshold.ApplyInPlace(bmp8bpp);
            //bmp8bpp.Save(@"C:\Users\Maor\Desktop\mode\BinarizeWithoutDilation.jpeg");

            Closing closing = new Closing();
            closing.ApplyInPlace(bmp8bpp);
            return bmp8bpp;
        }

        /// <summary>
        /// This method shrpen image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Bitmap Sharpen(Bitmap image)
        {
            Bitmap sharpenImage = (Bitmap)image.Clone();

            int filterWidth = 3;
            int filterHeight = 3;
            int width = image.Width;
            int height = image.Height;

            // Create sharpening filter.
            double[,] filter = new double[filterWidth, filterHeight];
            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            double factor = 1.0;
            double bias = 0.0;

            Color[,] result = new Color[image.Width, image.Height];

            // Lock image bits for read/write.
            BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Declare an array to hold the bytes of the bitmap.
            int bytes = pbits.Stride * height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

            int rgb;
            // Fill the color array with the new sharpened color values.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + width) % width;
                            int imageY = (y - filterHeight / 2 + filterY + height) % height;

                            rgb = imageY * pbits.Stride + 3 * imageX;

                            red += rgbValues[rgb + 2] * filter[filterX, filterY];
                            green += rgbValues[rgb + 1] * filter[filterX, filterY];
                            blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }

            // Update the image with the sharpened pixels.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    rgb = y * pbits.Stride + 3 * x;

                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }

            // Copy the RGB values back to the bitmap.
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            // Release image bits.
            sharpenImage.UnlockBits(pbits);
            //sharpenImage.Save(@"C:\Users\Maor\Desktop\mode\sharpenImage.jpeg");
            return sharpenImage;
        }

        private Bitmap SimplePosterization(Bitmap image)
        {
            SimplePosterization filter = new SimplePosterization();
            // process image
            filter.ApplyInPlace(image);
            return image;
        }

        /// <summary>
        /// This method flip image if needed. (uploaded from phone)
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Image Flip(Image image)
        {
            const int exifOrientationID = 0x112; //274
            if (!image.PropertyIdList.Contains(exifOrientationID))
            {
                return image;
            }

            //change rotation manually
            else
            {
                _logger.Debug($"Receipt upload from Phone");
                var prop = image.GetPropertyItem(exifOrientationID);
                int val = BitConverter.ToUInt16(prop.Value, 0);
                var rot = RotateFlipType.RotateNoneFlipNone;
                if (val == 3 || val == 4)
                    rot = RotateFlipType.Rotate180FlipNone;
                else if (val == 5 || val == 6)
                    rot = RotateFlipType.Rotate90FlipNone;
                else if (val == 7 || val == 8)
                    rot = RotateFlipType.Rotate270FlipNone;

                if (val == 2 || val == 4 || val == 5 || val == 7)
                    rot |= RotateFlipType.RotateNoneFlipX;

                if (rot != RotateFlipType.RotateNoneFlipNone)
                    image.RotateFlip(rot);
            }
            return image;
        }

        /// <summary>
        /// Getters
        /// </summary>
        /// <returns></returns>
        public Bitmap GetMode1()
        {
            return this.modes[0];
        }

        public Bitmap GetMode2()
        {
            return this.modes[1];
        }

        public Bitmap GetMode3()
        {
            return this.modes[2];
        }

        public Bitmap GetMode4()
        {
            return this.modes[3];
        }

        public Bitmap GetMode5()
        {
            return this.modes[4];
        }

        public Bitmap GetMode6()
        {
            return this.modes[5];
        }

        public Bitmap GetImageInNewResolution()
        {
            return this.imgInNewResolution;
        }
    }
}