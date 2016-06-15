using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Spots.App_Start {

    class WebApiConfig {

        public static void Register (HttpConfiguration config) {

            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}/{format}",
                defaults: new {
                    id = RouteParameter.Optional,
                    format = RouteParameter.Optional
                }
            );
        }
    }
}