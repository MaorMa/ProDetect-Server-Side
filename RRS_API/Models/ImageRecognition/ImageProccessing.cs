using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
namespace RRS_API.Models
{
    /*
     * This class responsible for image quality improvments
     */
    public class ImageProccessing
    {
        private Bitmap imgInNewResolution;
        List<Bitmap> modes; //mode1 - improve resolution, mode2 - grayscale, mode3 - sharpen image

        public ImageProccessing(Image image, Size size)
        {
            modes = new List<Bitmap>();
            Bitmap Bitimage = new Bitmap(image);
            imgInNewResolution = improveResolution(Bitimage, size);
            modes.Add(convertToGrayscaleV1(Bitimage));
            modes.Add(convertToGrayscaleV2(Bitimage));
            modes.Add(convertToBW(Bitimage));
            modes.Add(Sharpen(Bitimage));

        }

        /*
         * This method improve image resolution to 300X300
         */
        private Bitmap improveResolution(Bitmap image, Size size)
        {
            Bitmap b = new Bitmap(image);
            b.SetResolution(300, 300);
            return new Bitmap(b, size);
        }

        /*
         * mode 1
         */
        private Bitmap convertToGrayscaleV1(Bitmap image)
        {
            Image<Gray, Byte> img = new Image<Gray, Byte>(image);
            return img.ToBitmap();
        }

        /*
         * mode 2
         */
        private Bitmap convertToGrayscaleV2(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image.Width, image.Height);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
            return grayScale;
        }

        /*
         * mode 3
         */
        private Bitmap convertToBW(Bitmap image)
        {
            Image<Gray, Byte> newImage;
            Image<Bgr, byte> image_pass = new Image<Bgr, byte>(image);
            newImage = image_pass.Convert<Gray, Byte>().ThresholdBinaryInv(new Gray(190), new Gray(255));
            return newImage.ToBitmap();
        }


        public static Bitmap Sharpen(Bitmap image)
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

            return sharpenImage;
        }
        /*
         * get first mode
         */
        public Bitmap getMode1()
        {
            return this.modes[0];
        }

        /*
         * get second mode
         */
        public Bitmap getMode2()
        {
            return this.modes[1];
        }

        /*
         * get 3rd mode
         */
        public Bitmap getMode3()
        {
            return this.modes[2];
        }

        /*
         * get 4th mode
         */
        public Bitmap getMode4()
        {
            return this.modes[3];
        }



        public Bitmap getImageInNewResolution()
        {
            return this.imgInNewResolution;
        }
    }
}