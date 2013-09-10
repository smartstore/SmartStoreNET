using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.DiscountRules.HasPaymentMethod
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("Plugin.DiscountRules.HasPaymentMethod.Configure",
				 "Plugins/DiscountRulesHasPaymentMethod/Configure",
				 new { controller = "DiscountRulesHasPaymentMethod", action = "Configure" },
				 new[] { "SmartStore.Plugin.DiscountRules.HasPaymentMethod.Controllers" }
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
