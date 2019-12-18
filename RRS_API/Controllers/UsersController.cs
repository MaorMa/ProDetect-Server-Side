using RRS_API.Models;
using RRS_API.Models.Mangagers;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using log4net;
//using System.Reflection;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Users")]

    public class UsersController : ApiController
    {
        //private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ILog _logger = LogManager.GetLogger(typeof(UsersController));


        TokenMngr TokenMngr = new TokenMngr();
        UsersMngr UsersMngr = new UsersMngr();
        private PasswordMngr PasswordMngr = new PasswordMngr();

        [Route("AddFamilyUser")]
        [HttpPost]
        public HttpResponseMessage addUser()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                string username = httpRequest.Form["username"];//2 arg
                string password = httpRequest.Form["password"];//2
                _logger.Debug($"Adding new family username: {username}");
                var jwtToken = new JwtSecurityToken(token);
                if (TokenMngr.isTokenValid(token)) //check if token is valid
                {
                    //only admin can add users
                    if (TokenMngr.isAdmin(token))
                    {
                        UsersMngr.AddNewFamilyUser(username, password);
                        _logger.Info($"Succesful new family username: {username}");
                        return Request.CreateResponse(HttpStatusCode.Created);
                    }
                    //return forbidden
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Forbidden);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add family", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Route("tokenIsValid")]
        [HttpPost]
        /*
         * given username, password
         * return token if valid
         */
        public HttpResponseMessage tokenIsValid()
        {
            //get data from UI
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Form["token"];
                bool isValid = TokenMngr.isTokenValid(token.Trim());
                return Request.CreateResponse(HttpStatusCode.OK, isValid);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        [Route("Login")]
        [HttpPost]
        /*
         * given username, password
         * return token if valid
         */
        public HttpResponseMessage Login()
        {
            //return Request.CreateResponse(HttpStatusCode.OK, "123123yaniv");

            //get data from UI
            var username = HttpContext.Current.Request.Form["username"];
            _logger.Debug($"{username} is trying to login");
            var password = HttpContext.Current.Request.Form["password"];
            var salt_value = AzureConnection.getInstance().getSaltValue(username);
            var salt_bytes = Encoding.UTF8.GetBytes(salt_value);
            var password_bytes = Encoding.UTF8.GetBytes(password);
            string hashedPass = Convert.ToBase64String(PasswordMngr.ComputeHMAC_SHA256(password_bytes, salt_bytes));
            try
            {
                string token = TokenMngr.generateToken(username, password, hashedPass);
                return Request.CreateResponse(HttpStatusCode.OK, token);
            }
            catch (Exception e)
            {
                _logger.Error("Error login",e);
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        [Route("isAdmin")]
        [HttpPost]
        /*
         * return true/false for a given valid token
         */
        public HttpResponseMessage isAdmin()
        {
            _logger.Debug("Check if isAdmin");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                return Request.CreateResponse(HttpStatusCode.OK, TokenMngr.isAdmin(token));
            }
            catch (Exception e)
            {
                _logger.Error("Error isAdmin",e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
