using RRS_API.Models;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {
        private SettingsMngr SettingsMngr = new SettingsMngr();
        private TokenMngr TokenMngr = new TokenMngr();

        #region GET Requests
        [Route("GetFamilies")]
        [HttpGet]
        public HttpResponseMessage GetFamilies()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                var jwtToken = new JwtSecurityToken(token);
                if (TokenMngr.isTokenValid(token))
                {
                    List<String> Families = SettingsMngr.getFamilies();
                    return Request.CreateResponse(HttpStatusCode.OK, Families);
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

        [Route("GetMarkets")]
        [HttpGet]
        public HttpResponseMessage GetMarkets()
        {
            try
            {
                List<String> Markets = SettingsMngr.getMarkets();
                return Request.CreateResponse(HttpStatusCode.OK, Markets);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("isAdmin")]
        [HttpPost]
        /*
         * return true/false for a given valid token
         */
        public HttpResponseMessage isAdmin()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                return Request.CreateResponse(HttpStatusCode.OK, TokenMngr.isAdmin(token));
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
