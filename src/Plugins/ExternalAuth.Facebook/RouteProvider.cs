using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.ExternalAuth.Facebook
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("ExternalAuth.Facebook",
				 "Plugins/ExternalAuthFacebook/{action}",
				 new { controller = "ExternalAuthFacebook", action = "Login" },
				 new[] { "SmartStore.Plugin.ExternalAuth.Facebook.Controllers" }
			)
			.DataTokens["area"] = "ExternalAuth.Facebook";
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
