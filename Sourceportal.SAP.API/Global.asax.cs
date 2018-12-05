using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Sourceportal.SAP.API.DependencyResolution;
using WebApi.StructureMap;


namespace Sourceportal.SAP.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.UseStructureMap(x =>
            {
                x.AddRegistry<DefaultRegistry>();
            });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
