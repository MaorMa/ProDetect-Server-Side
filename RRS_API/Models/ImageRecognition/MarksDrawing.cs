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
        int normalizedX, normalizedY, normalizedWidth, normalizedHeight;
        Graphics graphics;
        Rectangle rect;
        double lowerYBound, upperYBound;
        bool xRuleDeviation = false, yRuleDeviation = false;

        public void draw(List<ocrWord> wordsToDraw, Receipt receipt, double averageX, double averageY)
        {
            graphics = Graphics.FromImage(receipt.getOriginalImage());

            //iterate over all recognized products we need to mark on the receipt
            foreach (ocrWord word in wordsToDraw)
            {
                normalizedX = (int)((word.getX() / receipt.getWidth()) * receipt.getOriginalImage().Width);
                normalizedY = (int)((word.getY() / receipt.getHeight()) * receipt.getOriginalImage().Height);
                normalizedWidth = (int)((word.getWidth() / receipt.getWidth()) * receipt.getOriginalImage().Width + padding);
                normalizedHeight = (int)((word.getHeight() / receipt.getHeight()) * receipt.getOriginalImage().Height);
                rect = new Rectangle(normalizedX, normalizedY, normalizedWidth, normalizedHeight);

                //rule for detect true negative (not products indeed)
                //draw red rectangle
                lowerYBound = averageY - (0.75 * averageY);
                upperYBound = averageY + (0.75 * averageY);
                xRuleDeviation = Math.Abs(word.getX() + word.getWidth() - (averageX)) > 350;
                yRuleDeviation = false;
                if (word.getY() < lowerYBound || word.getY() > upperYBound)
                {
                    if (word.getY() < lowerYBound)
                    {
                        yRuleDeviation = Math.Abs(word.getY() - lowerYBound) > 440;
                    }
                    else
                    {
                        yRuleDeviation = Math.Abs(word.getY() - upperYBound) > 440;
                    }
                }

                if (xRuleDeviation || yRuleDeviation)
                {
                    graphics.DrawRectangle(redPen, rect);
                }
                else
                {
                    graphics.DrawRectangle(greenPen, rect);
                }
            }
        }
    }
}