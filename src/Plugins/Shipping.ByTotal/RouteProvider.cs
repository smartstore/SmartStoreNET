using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Shipping.ByTotal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.ByTotal",
                 "Plugins/ShippingByTotal/{action}",
                 new { controller = "ShippingByTotal", action = "Configure" },
                 new[] { "SmartStore.Plugin.Shipping.ByTotal.Controllers" }
            )
			.DataTokens["area"] = "Shipping.ByTotal";
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
