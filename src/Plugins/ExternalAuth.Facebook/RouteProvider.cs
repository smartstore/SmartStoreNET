using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.ExternalAuth.Facebook
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.ExternalAuth.Facebook.Login",
                 "Plugins/ExternalAuthFacebook/Login",
                 new { controller = "ExternalAuthFacebook", action = "Login" },
                 new[] { "SmartStore.Plugin.ExternalAuth.Facebook.Controllers" }
            );

			routes.MapRoute("Plugin.ExternalAuth.Facebook.LoginCallback",
				 "Plugins/ExternalAuthFacebook/LoginCallback",
				 new { controller = "ExternalAuthFacebook", action = "LoginCallback" },
				 new[] { "SmartStore.Plugin.ExternalAuth.Facebook.Controllers" }
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
