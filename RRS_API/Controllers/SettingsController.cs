using IronOcr;
using RRS_API.Models;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AForge.Imaging.Filters;
using RRS_API.Models.Mangagers;
using log4net;
using System.Reflection;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {
        private SettingsMngr SettingsMngr = new SettingsMngr();
        private UsersMngr UsersMngr = new UsersMngr();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        #region GET Requests

        [Route("GetFamilies")]
        [HttpPost]
        public HttpResponseMessage GetFamilies()
        {
            try
            {
                List<string> Families = new List<string>();
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];

                //if token valid
                if (UsersMngr.IsUserTokenValid(token))
                {
                    string username = UsersMngr.getUsernameByToken(token);
                    _logger.Info($"USER:{username}     |ACTION: GetFamilies     |NAVIGATION: Receipts Upload Page");
                    //if global admin
                    if (UsersMngr.IsGlobalAdmin(token))
                    {
                        Families = SettingsMngr.GetFamilies();
                    }
                    //user
                    else
                    {
                        Families.Add(username.ToString());
                    }
                    //_logger.Debug("[Navigate to: Upload Receipt Page]");
                    return Request.CreateResponse(HttpStatusCode.OK, Families);
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

        [Route("GetMarkets")]
        [HttpGet]
        public HttpResponseMessage GetMarkets()
        {
            try
            {
                List<String> Markets = SettingsMngr.GetMarkets();
                return Request.CreateResponse(HttpStatusCode.OK, Markets);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region POST Requests
        #endregion
    }
}