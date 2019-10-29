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

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Users")]

    public class UsersController : ApiController
    {
        TokenMngr TokenMngr = new TokenMngr();
        UsersMngr UsersMngr = new UsersMngr();
        private PasswordMngr PasswordMngr = new PasswordMngr();

        [Route("AddFamilyUser")]
        [HttpPost]
        public HttpResponseMessage addUser()
        {
            var httpRequest = HttpContext.Current.Request;
            string token = httpRequest.Headers["Authorization"];
            string username = httpRequest.Form["username"];//2 arg
            string password = httpRequest.Form["password"];//2
            var jwtToken = new JwtSecurityToken(token);
            if (TokenMngr.isTokenValid(token)) //check if token is valid
            {
                //only admin can add users
                if (TokenMngr.isAdmin(token))
                { 
                    UsersMngr.AddNewFamilyUser(username, password);
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
            //get data from UI
            var username = HttpContext.Current.Request.Form["username"];
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
            catch (Exception)
            {
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
    }
}
