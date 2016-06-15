using System;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;
using Spots.App_Start;
using Umbraco.Web;

namespace Spots
{
    public class CustomGlobal : UmbracoApplication
    {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FormatterConfig.RegisterFormatters(GlobalConfiguration.Configuration.Formatters);

            var formatters = GlobalConfiguration.Configuration.Formatters;
            formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}