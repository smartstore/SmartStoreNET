using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.ShippingByWeight
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.ShippingByWeight",
                 "Plugins/ShippingByWeight/{action}",
                 new { controller = "ShippingByWeight", action = "Configure" },
                 new[] { "SmartStore.ShippingByWeight.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.ShippingByWeight";
        }

        public int Priority => 0;
    }
}
