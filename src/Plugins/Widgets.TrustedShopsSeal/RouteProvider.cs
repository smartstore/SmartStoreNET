using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Widgets.TrustedShopsSeal.Configure",
                 "Plugins/TrustedShopsSeal/Configure",
                 new { controller = "TrustedShopsSeal", action = "Configure" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers" }
            );

            routes.MapRoute("Plugin.Widgets.TrustedShopsSeal.PublicInfo",
                 "Plugins/TrustedShopsSeal/PublicInfo",
                 new { controller = "TrustedShopsSeal", action = "PublicInfo" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers" }
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
