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
        UsersMngr UsersMngr = new UsersMngr();

        [Route("AddFamily")]
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
                //var jwtToken = new JwtSecurityToken(token);

                //only global admin can add users
                if (UsersMngr.IsGlobalAdmin(token))
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
            catch (Exception ex)
            {
                _logger.Error($"Error while trying to add family", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        /*
        * given username, password
        * return token if valid
        */
        [Route("tokenIsValid")]
        [HttpPost]
        public HttpResponseMessage tokenIsValid()
        {
            //get data from UI
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Form["token"];
                bool isValid = UsersMngr.IsUserTokenValid(token.Trim());
                return Request.CreateResponse(HttpStatusCode.OK, isValid);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        /*
        * given username, password
        * return token if valid
        */
        [Route("Login")]
        [HttpPost]
        public HttpResponseMessage Login()
        {
            //get data from UI
            var username = HttpContext.Current.Request.Form["username"];
            _logger.Debug($"{username} is trying to login");
            var password = HttpContext.Current.Request.Form["password"];
            try
            {
                string token = UsersMngr.CheckLoginCredentials(username, password);
                _logger.Debug($"{username} connected sucessfully");
                return Request.CreateResponse(HttpStatusCode.OK, token);
            }
            catch (Exception e)
            {
                _logger.Error("Error login", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /*
        * return true if admin, otherwise false according to given token
        */
        [Route("isGlobalAdmin")]
        [HttpPost]

        public HttpResponseMessage isGlobalAdmin()
        {
            //_logger.Debug("Check if isGlobalAdmin");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Form["token"];
                return Request.CreateResponse(HttpStatusCode.OK, UsersMngr.IsGlobalAdmin(token));
            }
            catch (Exception e)
            {
                //_logger.Error("Error isGlobalAdmin", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /*
        * return true if admin, otherwise false according to given token
        */
        [Route("isAdmin")]
        [HttpPost]

        public HttpResponseMessage isAdmin()
        {
            //_logger.Debug("Check if isLocalAdmin");
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Form["token"];
                return Request.CreateResponse(HttpStatusCode.OK, UsersMngr.IsGlobalAdmin(token) || UsersMngr.IsLocalAdmin(token));
            }
            catch (Exception e)
            {
                //_logger.Error("Error isLocalAdmin", e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
