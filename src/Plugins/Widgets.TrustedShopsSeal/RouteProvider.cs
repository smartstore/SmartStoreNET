using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Widgets.TrustedShopsSeal",
                 "Plugins/TrustedShopsSeal/{action}",
                 new { controller = "TrustedShopsSeal", action = "Configure" },
                 new[] { "SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers" }
            )
			.DataTokens["area"] = "Widgets.TrustedShopsSeal";
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
