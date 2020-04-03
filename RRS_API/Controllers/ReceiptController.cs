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
using RRS_API.Models.Mangagers;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Receipt")]
    public class ReceiptController : ApiController
    {
        private ReceiptMngr ReceiptMngr = new ReceiptMngr();
        private UsersMngr UsersMngr = new UsersMngr();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Get requests
        [Route("GetAllFamiliesByReceiptStatus/{accView}")]
        [HttpPost]
        public HttpResponseMessage GetAllFamiliesByReceiptStatus(string accView)
        {
            if(accView.Equals("Acc"))
                _logger.Debug("GetAllFamiliesByReceiptStatus started. [Navigate to: receipt accept page]");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (UsersMngr.IsUserTokenValid(token))
                {
                    var jwtToken = new JwtSecurityToken(token);
                    object username = "";
                    jwtToken.Payload.TryGetValue("unique_name", out username);
                    _logger.Info("Succesful GetAllFamilies");
                    return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetAllFamiliesByReceiptStatus(accView, username.ToString(), UsersMngr.IsGlobalAdmin(token), UsersMngr.IsLocalAdmin(token)));

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

                //just global or local admin can approve receipts 
                if (UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token))
                {
                    _logger.Info("Succesful GetAllFamilyData");
                    return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetAllNotApprovedFamilyData(familyID));
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

        [Route("UpsertSimilarProducts/{description}/{marketID}/{sID}")]
        [HttpPost]
        public HttpResponseMessage UpsertSimilarProducts(string description,string marketID,string sID)
        {
            _logger.Debug("UpsertSimilarProducts started");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token))
                {
                    _logger.Info("Succesful UpsertSimilarProducts");
                    return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetAndInsertOptionalNames(description,marketID,sID));
                }
                else
                {
                    _logger.Debug("UpsertSimilarProducts Forbidden");
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error - UpsertSimilarProducts", e);
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
                if (UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token))
                {
                    _logger.Info("Succesful DeleteReceipt");
                    return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.DeleteReceipt(receiptID));
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
            _logger.Debug("GetAllApprovedData started. [Navigate to: receipt view page]");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (UsersMngr.IsUserTokenValid(token))
                {
                    _logger.Info("Succesful GetAllApprovedData");
                    return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetAllApprovedFamilyData(familyId));
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

        [Route("GetSimilarProductsByDesc/{Description}")]
        [HttpGet]
        public HttpResponseMessage GetSimilarProductsByDesc(string Description)
        {
            _logger.Debug($"GetSimilarProductsByDesc started, Description: {Description}");
            try
            {
                _logger.Info("Succesful GetProductInfo");
                return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetOptionalNames(Description));
            }
            catch (Exception e)
            {
                _logger.Error("Error - GetSimilarProductsByDesc", e);
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
                return Request.CreateResponse(HttpStatusCode.OK, ReceiptMngr.GetProductDataWithOptionalNames(productID, marketID));
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
                if (UsersMngr.IsUserTokenValid(token))
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
                            this.ReceiptMngr.ProcessReceipts(selectedFamilyID, selectedMarket, imgNameAndImg);
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
                if (UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token))
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

        [Route("ReturnToAccept/{familyID}")]
        [HttpPost]
        public HttpResponseMessage ReturnToAccept(string familyID, [FromBody] ReceiptToReturn receipt)
        {
            _logger.Debug($"ReturnToAccept started, familyID: {familyID} receiptID: {receipt.receiptID}");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                if (UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token))
                {
                    ReceiptMngr.ReturnReceiptToAccept(familyID, receipt);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Error - ReturnToAccept, familyID: {familyID} receiptID: {receipt.receiptID}");
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}