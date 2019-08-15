using System;
using System.Collections.Generic;
using System.Drawing;
using ImageRecognition.Objects;
using OcrProject.Parser;
using RRS_API.Models;
using Server;

namespace RRS_API.Controllers
{
    public class ProcessMngr
    {
        AzureConnection az = AzureConnection.getInstance();
        ocrResult ocrResult = new ocrResult();
        textParser tp = new textParser();
        /**
         *  Handle single POST of photos 
         */
        public void processPhotos(string selectedFamilyID, string selectedMarket, Dictionary<string, Image> imgNameAndImg)
        {
            List<receipt> receipts = ocrResult.fromImagesToText(imgNameAndImg);
            receipts = tp.getAllRecieptsData(receipts);
            receipts = getSidAndDesc(receipts, selectedMarket);
        }

        public List<receipt> getSidAndDesc(List<receipt> receipts, string selectedMarket)
        {
            //iterate over all the receipts
            foreach (receipt receipt in receipts)
            {
                int fontSize = getfontSize(receipt.getImage().Height);
                Font arialFont = new Font("Arial", fontSize);
                //iterate over all the item in receipt
                foreach (KeyValuePair<String, List<metaData>> sid in receipt.getIdToMetadata())
                {
                    //List<metaData> data = new List<metaData>();
                    foreach (metaData obj in sid.Value)
                    {
                        String[] productInfoFromDb = az.getName(sid.Key, selectedMarket);
                        if (productInfoFromDb[0] != null && !productInfoFromDb[0].Equals(""))
                        {
                            foreach (String s in productInfoFromDb)//if short and long sid exist
                            {
                                if (s != null && !s.Equals(""))
                                {
                                    String[] descAndPrice = s.Split(',');
                                    String description = descAndPrice[0];
                                    String price = descAndPrice[1];
                                    obj.setDescription(description);
                                    obj.setPrice(price);
                                }
                            }
                            List<ocrWord> words = receipt.getWord(sid.Key);
                            foreach (ocrWord word in words)
                            {
                                Graphics graphics = Graphics.FromImage(receipt.getImage());
                                graphics.DrawString("✔", arialFont, Brushes.Green, new PointF((float)(((double)word.getX() / receipt.getWidth()) * (double)receipt.getImage().Width), (float)(((double)word.getY() / receipt.getHeight()) * (double)receipt.getImage().Height)));
                            }
                        }
                    }
                }
            }
            return receipts;
        }

        /*
        * This method calculate font size according to given height
        */
        private int getfontSize(int height)
        {
            return (int)(double)(height / 1000) * 20;
        }
    }
}