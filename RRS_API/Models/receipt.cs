using ImageRecognition.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class receipt
{
    private int width;
    private int height;
    private string name;
    private string date;
    private Dictionary<String, List<ocrWord>> words;
    private Dictionary<String, List<metaData>> idToMetadata;
    private string[] rows;
    public Image image;

    public receipt(int width, int height, String name, Image image)
    {
        this.width = width;
        this.height = height;
        this.name = name;
        this.words = new Dictionary<String, List<ocrWord>>();
        this.idToMetadata = new Dictionary<String, List<metaData>>();
        this.image = image;
    }

    public void addWord(ocrWord word)
    {
        if (!words.Keys.Contains(word.getText()))
        {
            this.words[word.getText()] = new List<ocrWord>();
        }
        this.words[word.getText()].Add(word);
    }

    public void addRows(string[] rows)
    {
        this.rows = rows;
    }

    public List<ocrWord> getWord(String word)
    {
        return words[word];
    }

    public string[] getRows()
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

    public Dictionary<String, List<metaData>> getIdToMetadata()
    {
        return this.idToMetadata;
    }

    public void setIdToMetadata(Dictionary<String, List<metaData>> idToMetadata)
    {
        this.idToMetadata = idToMetadata;
    }

    public Image getImage()
    {
        return this.image;
    }
}
