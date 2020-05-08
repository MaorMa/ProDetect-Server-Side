using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

/// <summary>
/// This class represents receipt object
/// </summary>
public class Receipt
{
    //Fields
    private string name;
    private int width;
    private int height;
    private string date;
    private double xAverage;
    private double yAverage;
    private Dictionary<string, List<OcrWord>> words;
    private Dictionary<string, List<MetaData>> idToMetadata;
    private List<string> rows;
    private Image manipulatedImage;
    private Image originalImage;
    private string marketID;

    //C'tor
    public Receipt(int width, int height, string name, Image manipulatedImage, Image originalImage)
    {
        this.width = width;
        this.height = height;
        this.name = name;
        this.words = new Dictionary<string, List<OcrWord>>();
        this.idToMetadata = new Dictionary<string, List<MetaData>>();
        this.manipulatedImage = manipulatedImage;
        this.originalImage = originalImage;
        this.rows = new List<string>();
    }

    //Setters and Getters
    public void SetAverageCoordinates(double xAverage, double yAverage)
    {
        this.xAverage = xAverage;
        this.yAverage = yAverage;
    }

    public string GetMarketID()
    {
        return this.marketID;
    }

    public void SetMarketID(string marketID)
    {
        this.marketID = marketID;
    }

    public double GetXAverage()
    {
        return this.xAverage;
    }

    public double GetYAverage()
    {
        return this.yAverage;
    }

    public void AddWord(OcrWord word)
    {
        if (!words.Keys.Contains(word.getText()))
        {
            this.words[word.getText()] = new List<OcrWord>();
        }
        this.words[word.getText()].Add(word);
    }

    public void AddRow(string rows)
    {
        this.rows.Add(rows);
    }

    public List<OcrWord> GetWord(string word)
    {
        return words[word];
    }

    public List<string> getRows()
    {
        return rows;
    }

    public Dictionary<string, List<OcrWord>> GetWordsList()
    {
        return this.words;
    }

    public int GetHeight()
    {
        return this.height;
    }

    public int GetWidth()
    {
        return this.width;
    }

    public void SetDate(string date)
    {
        this.date = date;
    }
    public string getDate()
    {
        return this.date;
    }

    public Dictionary<string, List<MetaData>> GetIdToMetadata()
    {
        return this.idToMetadata;
    }

    public void SetIdToMetadata(Dictionary<string, List<MetaData>> idToMetadata)
    {
        this.idToMetadata = idToMetadata;
    }

    public Image GetOriginalImage()
    {
        return this.originalImage;
    }

    public Image GetManipulatedImage()
    {
        return this.manipulatedImage;
    }

    public string GetName()
    {
        return this.name;
    }
}
