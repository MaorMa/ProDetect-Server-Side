using System;

public class OcrWord
{
    private double x;
    private double y;
    private double width;
    private double height;

    private String text;

    public OcrWord(double x, double y, double width, double height, String text)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.text = text;
    }

    public double getX()
    {
        return this.x;
    }

    public double getY()
    {
        return this.y;
    }

    public double getHeight()
    {
        return this.height;
    }

    public double getWidth()
    {
        return this.width;
    }

    public string getText()
    {
        return this.text;
    }
}
