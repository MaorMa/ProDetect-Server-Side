using RRS_API.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RRS_API.Controllers
{
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {
        AzureConnection az = AzureConnection.getInstance();
        String query = "";

        [Route("GetFamilies")]
        [HttpGet]
        public HttpResponseMessage GetFamilies()
        {
            try
            {
                query = "SELECT * FROM Families";
                List<String> result = az.SelectQuery(query);
                return Request.CreateResponse(HttpStatusCode.OK,result);
            }
            catch (Exception e)
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
                query = "SELECT * FROM Markets";
                List<String> result = az.SelectQuery(query);
                //Get all families from DB
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
