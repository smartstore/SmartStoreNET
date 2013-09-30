using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Widgets.TrustedShopsCustomerProtection.Configure",
                 "Plugins/TrustedShopsCustomerProtection/Configure",
                 new { controller = "TrustedShopsCustomerProtection", action = "Configure" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Controllers" }
            );

            routes.MapRoute("Plugin.Widgets.TrustedShopsCustomerProtection.PublicInfo",
                 "Plugins/TrustedShopsCustomerProtection/PublicInfo",
                 new { controller = "TrustedShopsCustomerProtection", action = "PublicInfo" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Controllers" }
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
