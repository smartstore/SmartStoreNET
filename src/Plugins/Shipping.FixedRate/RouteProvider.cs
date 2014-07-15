using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Shipping.FixedRate
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.FixedRate.Action",
                 "Plugins/ShippingFixedRate/{action}",
                 new { controller = "ShippingFixedRate" },
                 new[] { "SmartStore.Plugin.Shipping.FixedRate.Controllers" }
            )
			.DataTokens["area"] = "Shipping.FixedRate";
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
