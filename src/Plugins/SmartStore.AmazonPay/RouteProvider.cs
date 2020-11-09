using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.AmazonPay
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.AmazonPay",
                    "Plugins/SmartStore.AmazonPay/{controller}/{action}",
                    new { controller = "AmazonPay" },
                    new[] { "SmartStore.AmazonPay.Controllers" }
            )
            .DataTokens["area"] = AmazonPayPlugin.SystemName;

            // for backward compatibility (IPN!)
            routes.MapRoute("SmartStore.AmazonPay.Legacy",
                    "Plugins/PaymentsAmazonPay/{action}",
                    new { controller = "AmazonPay" },
                    new[] { "SmartStore.AmazonPay.Controllers" }
            )
            .DataTokens["area"] = AmazonPayPlugin.SystemName;
        }

        public int Priority => 0;
    }
}