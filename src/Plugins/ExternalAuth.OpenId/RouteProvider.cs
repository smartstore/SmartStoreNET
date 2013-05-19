using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.ExternalAuth.OpenId
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.ExternalAuth.OpenId.Configure",
                 "Plugins/ExternalAuthOpenId/Configure",
                 new { controller = "ExternalAuthOpenId", action = "Configure" },
                 new[] { "SmartStore.Plugin.ExternalAuth.OpenId.Controllers" }
            );

            routes.MapRoute("Plugin.ExternalAuth.OpenId.PublicInfo",
                 "Plugins/ExternalAuthOpenId/PublicInfo",
                 new { controller = "ExternalAuthOpenId", action = "PublicInfo" },
                 new[] { "SmartStore.Plugin.ExternalAuth.OpenId.Controllers" }
            );

            routes.MapRoute("Plugin.ExternalAuth.OpenId.Login",
                 "Plugins/ExternalAuthOpenId/Login",
                 new { controller = "ExternalAuthOpenId", action = "Login" },
                 new[] { "SmartStore.Plugin.ExternalAuth.OpenId.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
