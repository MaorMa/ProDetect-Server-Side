using RRS_API.Models.Mangagers;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Statistics")]
    public class StatisticsController : ApiController
    {
        private TokenMngr TokenMngr = new TokenMngr();
        private StatisticsMngr StatisticsMngr = new StatisticsMngr();

        [Route("GetAllPricesByCategories/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetAllPricesByCategories(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                //TokenMngr.isTokenValid(token)
                if (TokenMngr.isTokenValid(token))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, StatisticsMngr.GetAllPricesByCategories(familyID));   
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
                if (TokenMngr.isTokenValid(token))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, StatisticsMngr.GetAllQuantitiesByCategories(familyID));
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


        [Route("GetAllQuantitiesByNutrients/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetAllQuantitiesByNutrients(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                //TokenMngr.isTokenValid(token)
                if (TokenMngr.isTokenValid(token))
                {
                    return Request.CreateResponse(HttpStatusCode.Created, StatisticsMngr.GetAllQuantitiesByNutrients(familyID));
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

        [Route("GetCompareByCost/{familyID}")]
        [HttpPost]
        public HttpResponseMessage GetCompareByCost(string familyID)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                //TokenMngr.isTokenValid(token)
                if (true)
                {
                    return Request.CreateResponse(HttpStatusCode.Created, StatisticsMngr.GetCompareByCost(familyID));
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
    }
}
