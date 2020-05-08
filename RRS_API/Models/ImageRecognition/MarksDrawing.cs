using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RRS_API.Models
{
    /// <summary>
    /// This class responsilbe for drawing detected products in receipt image.
    /// </summary>
    public class MarksDrawing
    {
        private int padding = 14;
        Pen greenPen = new Pen(Color.Green, 4);
        Pen redPen = new Pen(Color.Red, 4);
        Graphics graphics;
        Rectangle rectangle;

        /// <summary>
        /// This method drawing detected products.
        /// </summary>
        /// <param name="wordsToDraw"></param>
        /// <param name="receipt"></param>
        public void Draw(List<OcrWord> wordsToDraw, Receipt receipt)
        {
            try
            {
                graphics = Graphics.FromImage(receipt.GetOriginalImage());
                int normalizedX, normalizedY, normalizedWidth, normalizedHeight;
                double xAverage = receipt.GetXAverage();
                double yAverage = receipt.GetYAverage();
                bool isValid = false;
                Dictionary <string,List<MetaData>> idToMetaData = receipt.GetIdToMetadata();
                //iterate over all recognized products we need to mark on the receipt
                foreach (OcrWord word in wordsToDraw)
                {
                    isValid = idToMetaData[word.getText()][0].getvalidProduct();
                    //create rectangle boundries
                    normalizedX = (int)((word.getX() / receipt.GetWidth()) * receipt.GetOriginalImage().Width);
                    normalizedY = (int)((word.getY() / receipt.GetHeight()) * receipt.GetOriginalImage().Height);
                    normalizedWidth = (int)((word.getWidth() / receipt.GetWidth()) * receipt.GetOriginalImage().Width + padding);
                    normalizedHeight = (int)((word.getHeight() / receipt.GetHeight()) * receipt.GetOriginalImage().Height);
                    rectangle = new Rectangle(normalizedX, normalizedY, normalizedWidth, normalizedHeight);
                    if (isValid)
                    {
                        graphics.DrawRectangle(greenPen, rectangle);
                    }
                    else
                    {
                        graphics.DrawRectangle(redPen, rectangle);
                    }
                }
            } catch(Exception e) {
            }
        }
    }
}