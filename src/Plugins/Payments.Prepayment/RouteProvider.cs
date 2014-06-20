using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.Prepayment
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Prepayment.Configure",
                 "Plugins/PaymentPrepayment/Configure",
                 new { controller = "PaymentPrepayment", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.Prepayment.Controllers" }
            )
			.DataTokens["area"] = "Payments.Prepayment";

            routes.MapRoute("Plugin.Payments.Prepayment.PaymentInfo",
                 "Plugins/PaymentPrepayment/PaymentInfo",
                 new { controller = "PaymentPrepayment", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.Prepayment.Controllers" }
            )
			.DataTokens["area"] = "Payments.Prepayment";
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
