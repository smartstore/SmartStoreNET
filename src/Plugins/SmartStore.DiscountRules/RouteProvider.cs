using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.DiscountRules
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("SmartStore.DiscountRules",
				 "Plugins/SmartStore.DiscountRules/{controller}/{action}",
                 new { controller = "DiscountRules", action = "Index" },
                 new[] { "SmartStore.DiscountRules.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.DiscountRules";
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
