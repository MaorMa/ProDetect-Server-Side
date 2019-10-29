using RRS_API.Models;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Receipt")]
    public class ReceiptController : ApiController
    {
        ReceiptMngr ReceiptMngr = new ReceiptMngr();
        TokenMngr TokenMngr = new TokenMngr();

        #region Get requests
        /*[Route("GetTotalUploadsDetails")]
        [HttpGet]
        public HttpResponseMessage GetTotalUploadsDetails()
        {
            try
            {
                string result = ReceiptMngr.GetTotalUploadsDetails();
                return Request.CreateResponse(HttpStatusCode.Created, result);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }*/

        [Route("GetAllRecognizedData")]
        [HttpPost]
        public HttpResponseMessage GetAllRecognizedData()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllRecognizedData());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetAllApprovedData")]
        [HttpPost]
        public HttpResponseMessage GetAllApprovedData()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllApprovedData());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
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
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetProductInfo(productID, marketID));
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region POST Requests
        /*
         * POST- post photos from specific family for a specific market
         */
        [Route("UploadReceipts")]
        [HttpPost]
        public HttpResponseMessage UploadReceipts()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token))
                {
                    Dictionary<String, Image> imgNameAndImg = new Dictionary<string, Image>();
                    try
                    {
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
                            this.ReceiptMngr.processPhotos(selectedFamilyID, selectedMarket, imgNameAndImg);
                        }).Start();
                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    catch (Exception)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }
        #endregion

        #region PUT Requests
        //need to send token
        [Route("UpdateReceiptData/{familyID}")]
        [HttpPut]
        public HttpResponseMessage UpdateReceiptData(string familyID, [FromBody] ReceiptToReturn receiptToUpdate)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    ReceiptMngr.UpdateReceiptData(familyID, receiptToUpdate);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}