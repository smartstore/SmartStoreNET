using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.FacebookAuth.Core;
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
			.DataTokens["area"] = Provider.SystemName;
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
