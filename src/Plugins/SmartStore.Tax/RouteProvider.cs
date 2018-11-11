using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Tax
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("SmartStore.Tax.FixedRate",
				 "Plugins/SmartStore.Tax/FixedRate/{action}",
				 new { controller = "TaxFixedRate", action = "Configure" },
				 new[] { "SmartStore.Tax.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.Tax";

			routes.MapRoute("SmartStore.Tax.ByRegion",
				 "Plugins/SmartStore.Tax/ByRegion/{action}",
				 new { controller = "TaxByRegion", action = "Configure" },
				 new[] { "SmartStore.Tax.Controllers" }
			)
			.DataTokens["area"] = "SmartStore.Tax";
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
