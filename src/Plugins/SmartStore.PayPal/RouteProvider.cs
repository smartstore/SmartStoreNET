using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.PayPal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.PayPalExpress",
                "Plugins/SmartStore.PayPal/{controller}/{action}",
                new { controller = "PayPalExpress", action = "Index" },
                new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalDirect",
                "Plugins/SmartStore.PayPal/{controller}/{action}",
                new { controller = "PayPalDirect", action = "Index" },
                new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalStandard",
                "Plugins/SmartStore.PayPal/{controller}/{action}",
                new { controller = "PayPalStandard", action = "Index" },
                new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            //Legacay Routes
            routes.MapRoute("SmartStore.PayPalExpress.IPN",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PayPalExpress", action = "IPNHandler" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalDirect.IPN",
                 "Plugins/PaymentPayPalDirect/IPNHandler",
                 new { controller = "PayPalDirect", action = "IPNHandler" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("Plugin.Payments.PayPalStandard.IPNHandler",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PayPalStandard", action = "IPNHandler" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";



            //TODO: Check whether these Routes needs to stay
            //routes.MapRoute("Payments.PayPal.RedirectFromPaymentInfo",
            //     "Plugins/PayPalExpress/RedirectFromPaymentInfo",
            //     new { controller = "PayPal", action = "RedirectFromPaymentInfo" },
            //     new[] { "SmartStore.PayPal.Controllers" }
            //)
            //.DataTokens["area"] = "SmartStore.PayPal";

            //routes.MapRoute("Plugin.Payments.PayPalStandard.PDTHandler",
            //     "Plugins/PaymentPayPalStandard/PDTHandler",
            //     new { controller = "PaymentPayPalStandard", action = "PDTHandler" },
            //     new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            //)
            //.DataTokens["area"] = "Payments.PayPalStandard";

            //routes.MapRoute("Plugin.Payments.PayPalStandard.CancelOrder",
            //     "Plugins/PaymentPayPalStandard/CancelOrder",
            //     new { controller = "PaymentPayPalStandard", action = "CancelOrder" },
            //     new[] { "SmartStore.Plugin.Payments.PayPalStandard.Controllers" }
            //)
            //.DataTokens["area"] = "Payments.PayPalStandard";

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
