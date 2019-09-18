using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class Receipt
{
    private int width;
    private int height;
    private string name;
    private string date;
    private double xAverage;
    private Dictionary<String, List<ocrWord>> words;
    private Dictionary<String, List<MetaData>> idToMetadata;
    private List<String> rows;
    private Image ManipulatedImage;
    private Image OriginalImage;
    private string marketID;

    public Receipt(int width, int height, string name, Image ManipulatedImage, Image OriginalImage)
    {
        this.width = width;
        this.height = height;
        this.name = name;
        this.words = new Dictionary<String, List<ocrWord>>();
        this.idToMetadata = new Dictionary<String, List<MetaData>>();
        this.ManipulatedImage = ManipulatedImage;
        this.OriginalImage = OriginalImage;
        this.rows = new List<string>();
    }

    public string getMarketID()
    {
        return this.marketID;
    }

    public void setMarketID(string marketID)
    {
        this.marketID = marketID;
    }


    public void setxAverage(int average)
    {
        this.xAverage = average;
    }

    public double getxAverage()
    {
        return this.xAverage;
    }

    public void addWord(ocrWord word)
    {
        if (!words.Keys.Contains(word.getText()))
        {
            this.words[word.getText()] = new List<ocrWord>();
        }
        this.words[word.getText()].Add(word);
    }

    public void addRow(string rows)
    {
        this.rows.Add(rows);
    }

    public List<ocrWord> getWord(String word)
    {
        return words[word];
    }

    public List<String> getRows()
    {
        return rows;
    }

    public Dictionary<String, List<ocrWord>> getWordsList()
    {
        return this.words;
    }

    public int getHeight()
    {
        return this.height;
    }

    public int getWidth()
    {
        return this.width;
    }

    public void setDate(string date)
    {
        this.date = date;
    }
    public string getDate()
    {
        return this.date;
    }

    public Dictionary<String, List<MetaData>> getIdToMetadata()
    {
        return this.idToMetadata;
    }

    public void setIdToMetadata(Dictionary<String, List<MetaData>> idToMetadata)
    {
        this.idToMetadata = idToMetadata;
    }

    public Image getOriginalImage()
    {
        return this.OriginalImage;
    }

    public Image getManipulatedImage()
    {
        return this.ManipulatedImage;
    }

    public String getName()
    {
        return this.name;
    }
}
