using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Widgets.GoogleAnalytics
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Widgets.GoogleAnalytics.Configure",
                 "Plugins/WidgetsGoogleAnalytics/Configure",
                 new { controller = "WidgetsGoogleAnalytics", action = "Configure" },
                 new[] { "SmartStore.Plugin.Widgets.GoogleAnalytics.Controllers" }
            )
			.DataTokens["area"] = "Widgets.GoogleAnalytics";
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
