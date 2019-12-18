using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;

namespace RRS_API
{
    public static class WebApiConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Register(HttpConfiguration config)
        {
            _logger.Debug("*_*-*_* WebApi initializing *_*-*_*");
            // Web API configuration and services
            //config.EnableCors(new EnableCorsAttribute("https://maorma.github.io", headers: "*", methods: "*"));
            config.EnableCors(new EnableCorsAttribute("http://localhost:4200", headers: "*", methods: "*"));
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            _logger.Debug("*_*-*_* WebApi initialized *_*-*_*");
        }
    }
}
