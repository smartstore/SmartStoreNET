using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HadSpentAmount.Configure",
                 "Plugins/DiscountRulesHadSpentAmount/Configure",
                 new { controller = "DiscountRulesHadSpentAmount", action = "Configure" },
                 new[] { "SmartStore.Plugin.DiscountRules.HadSpentAmount.Controllers" }
            )
			.DataTokens["area"] = "DiscountRules.HadSpentAmount";
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
