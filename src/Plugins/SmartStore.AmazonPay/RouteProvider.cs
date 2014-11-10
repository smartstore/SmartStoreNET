using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.AmazonPay.Services;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.AmazonPay
{
	public class RouteProvider : IRouteProvider
	{
		public void RegisterRoutes(RouteCollection routes)
		{
			// for backward compatibility (IPN!)
			routes.MapRoute("SmartStore.AmazonPay",
					"Plugins/PaymentsAmazonPay/{action}",
					new { controller = "AmazonPay" },
					new[] { "SmartStore.AmazonPay.Controllers" }
			)
			.DataTokens["area"] = AmazonPayCore.SystemName;

			routes.MapRoute("SmartStore.AmazonPay.Checkout",
					"Plugins/SmartStore.AmazonPay.Checkout/{controller}/{action}",
					new { controller = "AmazonPayCheckout" },
					new[] { "SmartStore.AmazonPay.Controllers" }
			)
			.DataTokens["area"] = AmazonPayCore.SystemName;

			routes.MapRoute("SmartStore.AmazonPay.ShoppingCart",
					"Plugins/SmartStore.AmazonPay.ShoppingCart/{controller}/{action}",
					new { controller = "AmazonPayShoppingCart" },
					new[] { "SmartStore.AmazonPay.Controllers" }
			)
			.DataTokens["area"] = AmazonPayCore.SystemName;
		}

		public int Priority { get { return 0; } }
	}
}