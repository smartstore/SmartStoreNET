using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Widgets.TrustedShopsCustomerReviews.Configure",
                 "Plugins/TrustedShopsCustomerReviews/Configure",
                 new { controller = "TrustedShopsCustomerReviews", action = "Configure" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Controllers" }
            );

            routes.MapRoute("Plugin.Widgets.TrustedShopsCustomerReviews.PublicInfo",
                 "Plugins/TrustedShopsCustomerReviews/PublicInfo",
                 new { controller = "TrustedShopsCustomerReviews", action = "PublicInfo" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Controllers" }
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
