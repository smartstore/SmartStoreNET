using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.PayPalStandard
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PayPalStandard.Configure",
                 "Plugins/PaymentPayPalStandard/Configure",
                 new { controller = "PaymentPayPalStandard", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.PayPalStandard.PaymentInfo",
                 "Plugins/PaymentPayPalStandard/PaymentInfo",
                 new { controller = "PaymentPayPalStandard", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            );
            
            //PDT
            routes.MapRoute("Plugin.Payments.PayPalStandard.PDTHandler",
                 "Plugins/PaymentPayPalStandard/PDTHandler",
                 new { controller = "PaymentPayPalStandard", action = "PDTHandler" },
                 new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            );
            //IPN
            routes.MapRoute("Plugin.Payments.PayPalStandard.IPNHandler",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PaymentPayPalStandard", action = "IPNHandler" },
                 new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            );
            //Cancel
            routes.MapRoute("Plugin.Payments.PayPalStandard.CancelOrder",
                 "Plugins/PaymentPayPalStandard/CancelOrder",
                 new { controller = "PaymentPayPalStandard", action = "CancelOrder" },
                 new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
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
