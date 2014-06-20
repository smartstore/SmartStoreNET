using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.ExternalAuth.OpenId
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("ExternalAuth.OpenId",
                 "Plugins/ExternalAuthOpenId/{action}",
                 new { controller = "ExternalAuthOpenId", action = "Login" },
                 new[] { "SmartStore.Plugin.ExternalAuth.OpenId.Controllers" }
            )
			.DataTokens["area"] = "ExternalAuth.OpenId";
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
