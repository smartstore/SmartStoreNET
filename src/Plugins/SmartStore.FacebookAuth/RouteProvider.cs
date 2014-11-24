using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

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
			.DataTokens["area"] = "SmartStore.FacebookAuth";
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
