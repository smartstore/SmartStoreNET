using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.FacebookAuth
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.FacebookAuth",
                 "Plugins/SmartStore.FacebookAuth/{action}",
                 new { controller = "ExternalAuthFacebook" },
                 new[] { "SmartStore.FacebookAuth.Controllers" }
            )
            .DataTokens["area"] = FacebookExternalAuthMethod.SystemName;
        }
        public int Priority => 0;
    }
}
