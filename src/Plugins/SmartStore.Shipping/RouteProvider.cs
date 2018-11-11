using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Shipping
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.Shipping.ByTotal",
                 "Plugins/ShippingByTotal/{action}",
                 new { controller = "ByTotal", action = "Configure" },
                 new[] { "SmartStore.Shipping.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.Shipping";

            routes.MapRoute("SmartStore.Shipping.FixedRate",
                 "Plugins/FixedRate/{action}",
                 new { controller = "FixedRate", action = "Configure" },
                 new[] { "SmartStore.Shipping.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.Shipping";
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
