using System;
using System.Collections.Generic;
using System.Drawing;

namespace RRS_API.Models
{
    public class MarksDrawing
    {
        private int padding = 14;
        Pen greenPen = new Pen(Color.Green, 4);
        Pen redPen = new Pen(Color.Red, 4);
        Graphics graphics;
        Rectangle rectangle;

        public void Draw(List<OcrWord> wordsToDraw, Receipt receipt)
        {
            try
            {
                graphics = Graphics.FromImage(receipt.GetOriginalImage());
                int normalizedX, normalizedY, normalizedWidth, normalizedHeight;
                bool xRuleDeviation = false, yRuleDeviation = false;
                double lowerYBound, upperYBound;
                double xAverage = receipt.GetXAverage();
                double yAverage = receipt.GetYAverage();

                //iterate over all recognized products we need to mark on the receipt
                foreach (OcrWord word in wordsToDraw)
                {
                    normalizedX = (int)((word.getX() / receipt.GetWidth()) * receipt.GetOriginalImage().Width);
                    normalizedY = (int)((word.getY() / receipt.GetHeight()) * receipt.GetOriginalImage().Height);
                    normalizedWidth = (int)((word.getWidth() / receipt.GetWidth()) * receipt.GetOriginalImage().Width + padding);
                    normalizedHeight = (int)((word.getHeight() / receipt.GetHeight()) * receipt.GetOriginalImage().Height);
                    rectangle = new Rectangle(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

                    //rule for detect true negative (not products indeed)
                    //draw red rectangle
                    lowerYBound = 0.5 * yAverage;
                    upperYBound = 1.25 * yAverage;
                    xRuleDeviation = Math.Abs(normalizedX + normalizedWidth - (xAverage)) > 0.08 * receipt.GetWidth();
                    yRuleDeviation = false;
                    if (word.getY() < lowerYBound || word.getY() > upperYBound)
                    {
                        if (word.getY() < lowerYBound)
                        {
                            yRuleDeviation = Math.Abs(word.getY() - lowerYBound) > 400;
                        }
                        else
                        {
                            yRuleDeviation = Math.Abs(word.getY() - upperYBound) > 400;
                        }
                    }

                    if (xRuleDeviation)
                    {
                        graphics.DrawRectangle(redPen, rectangle);
                    }
                    else
                    {
                        graphics.DrawRectangle(greenPen, rectangle);
                    }
                }
            } catch(Exception e) {
            }
        }
    }
}