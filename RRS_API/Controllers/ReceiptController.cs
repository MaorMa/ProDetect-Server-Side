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
using log4net;
using System.Reflection;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Receipt")]
    public class ReceiptController : ApiController
    {
        ReceiptMngr ReceiptMngr = new ReceiptMngr();
        TokenMngr TokenMngr = new TokenMngr();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

            /*
        [Route("GetAllRecognizedData")]
        [HttpPost]
        public HttpResponseMessage GetAllRecognizedData()
        {
            _logger.Debug("GetAllRecognizedData started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    _logger.Info("Succesful GetAllRecognizedData");
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllRecognizedData());
                }
                else
                {
                    _logger.Debug("GetAllRecognizedData Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetAllRecognizedData", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        */

        [Route("GetAllFamilies/{accView}")]
        [HttpPost]
        public HttpResponseMessage GetAllFamilies(string accView)
        {
            _logger.Debug("GetAllFamilies started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    _logger.Info("Succesful GetAllFamilies");
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllFamilies(accView));
                }
                else
                {
                    _logger.Debug("GetAllFamilies Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetAllFamilies", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetAllFamilyData/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetAllFamilyData(string familyID)
        {
            _logger.Debug("GetAllFamilyData started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    _logger.Info("Succesful GetAllFamilyData");
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllFamilyData(familyID));
                }
                else
                {
                    _logger.Debug("GetAllFamilyData Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetAllFamilyData", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        [Route("DeleteReceipt/{receiptID}")]
        [HttpPost]
        public HttpResponseMessage DeleteReceipt(string receiptID)
        {
            _logger.Debug("DeleteReceipt started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    _logger.Info("Succesful DeleteReceipt");
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.DeleteReceipt(receiptID));
                }
                else
                {
                    _logger.Debug("DeleteReceipt Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - DeleteReceipt", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetAllApprovedData/{familyId}")]
        [HttpPost]
        public HttpResponseMessage GetAllApprovedData(string familyId)
        {
            _logger.Debug("GetAllApprovedData started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (TokenMngr.isTokenValid(token) && TokenMngr.isAdmin(token))
                {
                    _logger.Info("Succesful GetAllApprovedData");
                    return Request.CreateResponse(HttpStatusCode.Created, ReceiptMngr.GetAllApprovedData(familyId));
                }
                else
                {
                    _logger.Debug("GetAllApprovedData Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetAllApprovedData", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetProductInfo/{marketID}/{productID}")]
        [HttpGet]
        public HttpResponseMessage GetProductInfo(string marketID, string productID)
        {
            _logger.Debug($"GetProductInfo started, marketID: {marketID}, productID: {productID}");
            try
            {
                _logger.Info("Succesful GetProductInfo");
                return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetProductInfo(productID, marketID));
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetProductInfo", e);
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
            _logger.Debug($"UploadReceipts started");
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
                        _logger.Debug($"UploadReceipts FamilyID: {selectedFamilyID}, market: {selectedMarket}, files: {postedFiles.Count}");

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
                    catch (Exception e)
                    {
                        _logger.Error("Error UploadReceipts working of photos", e);
                        return Request.CreateResponse(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error UploadReceipts", e);
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        //need to send token
        [Route("UpdateReceiptData/{familyID}")]
        [HttpPost]
        public HttpResponseMessage UpdateReceiptData(string familyID, [FromBody] ReceiptToReturn receiptToUpdate)
        {
            _logger.Debug($"UpdateReceiptData started, familyID: {familyID}, receipt: {receiptToUpdate.receiptID}");
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
            catch (Exception e)
            {
                _logger.Error($"Error - UpdateReceiptData, familyID: {familyID}, receipt: {receiptToUpdate.receiptID}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}