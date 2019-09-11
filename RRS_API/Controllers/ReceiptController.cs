using RRS_API.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Receipt")]
    public class ReceiptController : ApiController
    {
        ProcessMngr mg = new ProcessMngr();

        /**
         * POST- post photos from specific family for a specific market
         */
        [Route("UploadReceipts")]
        [HttpPost]
        public HttpResponseMessage UploadReceipts()
        {
            Dictionary<String, Image> imgNameAndImg = new Dictionary<string, Image>();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var selectedFamilyID = httpRequest.Form["familyID"];//2 arg
                var selectedMarket = httpRequest.Form["market"];//2 arg
                var postedFiles = httpRequest.Files;//3 arg

                foreach (var fileKey in postedFiles.AllKeys)
                {
                    //Copy stream to proceed working with it after response to user
                    MemoryStream destination = new MemoryStream();
                    postedFiles[fileKey].InputStream.CopyTo(destination);
                    Image image = Image.FromStream(destination);
                    imgNameAndImg.Add(fileKey, image);
                }
                new System.Threading.Tasks.Task(() =>
                {
                    this.mg.processPhotos(selectedFamilyID, selectedMarket, imgNameAndImg);
                }).Start();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetTotalUploadsDetails")]
        [HttpGet]
        public HttpResponseMessage GetTotalUploadsDetails()
        {
            try
            {
                string result = mg.GetTotalUploadsDetails();
                return Request.CreateResponse(HttpStatusCode.Created, result);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetAllRecognizedData")]
        [HttpGet]
        public HttpResponseMessage GetAllRecognizedData()
        {
            try
            {
                string result = mg.GetAllRecognizedData();
                return Request.CreateResponse(HttpStatusCode.Created, result);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("UpdateReceiptData/{familyID}")]
        [HttpPut]
        public HttpResponseMessage UpdateReceiptData([FromBody] ReceiptToReturn receiptToUpdate)
        {
            try
            {
                mg.UpdateReceiptData(receiptsToUpdate);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetProductInfo/{marketID}/{productID}")]
        [HttpGet]
        public HttpResponseMessage GetProductInfo(string marketID, string productID)
        {
            //need to fix sid - 729000....
            try
            {
                List<string> result = mg.GetProductInfo(productID, marketID);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
