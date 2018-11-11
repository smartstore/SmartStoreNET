using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.GoogleAnalytics
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.GoogleAnalytics",
				 "Plugins/SmartStore.GoogleAnalytics/{action}",
                 new { controller = "WidgetsGoogleAnalytics", action = "Configure" },
                 new[] { "SmartStore.GoogleAnalytics.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.GoogleAnalytics";
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
