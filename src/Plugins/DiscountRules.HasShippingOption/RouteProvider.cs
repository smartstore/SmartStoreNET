using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.DiscountRules.HasShippingOption
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("Plugin.DiscountRules.HasShippingOption.Configure",
				 "Plugins/DiscountRulesHasShippingOption/Configure",
				 new { controller = "DiscountRulesHasShippingOption", action = "Configure" },
				 new[] { "SmartStore.Plugin.DiscountRules.HasShippingOption.Controllers" }
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
