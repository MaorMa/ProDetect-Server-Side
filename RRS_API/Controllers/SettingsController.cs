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
        [HttpPost]
        public HttpResponseMessage GetFamilies()
        {
            try
            {
                List<string> Families = new List<string>();
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                if (TokenMngr.isTokenValid(token))
                {
                    //if admin
                    if (TokenMngr.isAdmin(token))
                    {
                        Families = SettingsMngr.getFamilies();
                    }
                    //user
                    else
                    {
                        var jwtToken = new JwtSecurityToken(token);
                        object username = "";
                        jwtToken.Payload.TryGetValue("unique_name", out username);
                        Families.Add(username.ToString());
                    }
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

        #endregion

        #region POST Requests
        #endregion
    }
}