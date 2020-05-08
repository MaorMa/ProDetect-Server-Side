using log4net;
using RRS_API.Models.Mangagers;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Statistics")]
    public class StatisticsController : ApiController
    {
        private UsersMngr UsersMngr = new UsersMngr();
        private StatisticsMngr StatisticsMngr = new StatisticsMngr();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Route("GetAllPricesByCategories/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetAllPricesByCategories(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                if (UsersMngr.IsUserTokenValid(token))
                {
                    string username = UsersMngr.getUsernameByToken(token);
                    _logger.Info($"USER:{username}     |ACTION: GetAllPricesByCategories     |NAVIGATION: Statistics Page");
                    return Request.CreateResponse(HttpStatusCode.OK, StatisticsMngr.GetAllPricesByCategories(familyID));   
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


        [Route("GetAllQuantitiesByCategories/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetAllQuantitiesByCategories(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                //TokenMngr.isTokenValid(token)
                if (UsersMngr.IsUserTokenValid(token))
                {
                    string username = UsersMngr.getUsernameByToken(token);
                    _logger.Info($"USER:{username}     |ACTION: GetAllQuantitiesByCategories     |NAVIGATION: Statistics Page");
                    return Request.CreateResponse(HttpStatusCode.OK, StatisticsMngr.GetAllQuantitiesByCategories(familyID));
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

        [Route("GetCompareByCost/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetCompareByCost(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                if (UsersMngr.IsUserTokenValid(token))
                {
                    string username = UsersMngr.getUsernameByToken(token);
                    _logger.Info($"USER:{username}     |ACTION: GetCompareByCost     |NAVIGATION: Statistics Page");
                    return Request.CreateResponse(HttpStatusCode.OK, StatisticsMngr.GetCompareByCost(familyID));
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
    }
}
