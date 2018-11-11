using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

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

			routes.MapRoute("SmartStore.PayPalPlus",
				"Plugins/SmartStore.PayPal/{controller}/{action}",
				new { controller = "PayPalPlus", action = "Index" },
				new[] { "SmartStore.PayPal.Controllers" }
			)
			.DataTokens["area"] = Plugin.SystemName;



			//Legacay Routes
			routes.MapRoute("SmartStore.PayPalExpress.IPN",
                 "Plugins/PaymentPayPalExpress/IPNHandler",
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

            routes.MapRoute("SmartStore.PayPalStandard.IPN",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PayPalStandard", action = "IPNHandler" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalStandard.PDT",
                 "Plugins/PaymentPayPalStandard/PDTHandler",
                 new { controller = "PayPalStandard", action = "PDTHandler" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalExpress.RedirectFromPaymentInfo",
                 "Plugins/PaymentPayPalExpress/RedirectFromPaymentInfo",
                 new { controller = "PayPalExpress", action = "RedirectFromPaymentInfo" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";

            routes.MapRoute("SmartStore.PayPalStandard.CancelOrder",
                 "Plugins/PaymentPayPalStandard/CancelOrder",
                 new { controller = "PayPalStandard", action = "CancelOrder" },
                 new[] { "SmartStore.PayPal.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayPal";
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
