using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.Sofortueberweisung
{
	public class RouteProvider : IRouteProvider
	{
		public int Priority { get { return 0; } }

		public void RegisterRoutes(RouteCollection routes) {
			routes.MapRoute("Plugin.Payments.Sofortueberweisung.Configure",
					"Plugins/PaymentSofortueberweisung/Configure",
					new { controller = "PaymentSofortueberweisung", action = "Configure" },
					new[] { "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }
			);

			routes.MapRoute("Plugin.Payments.Sofortueberweisung.PaymentInfo",
					"Plugins/PaymentSofortueberweisung/PaymentInfo",
					new { controller = "PaymentSofortueberweisung", action = "PaymentInfo" },
					new[] { "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }
			);

			routes.MapRoute("Plugin.Payments.Sofortueberweisung.Success",
					"Plugins/PaymentSofortueberweisung/Success",
					new { controller = "PaymentSofortueberweisung", action = "Success" },
					new[] { "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }
			);

			routes.MapRoute("Plugin.Payments.Sofortueberweisung.Abort",
					"Plugins/PaymentSofortueberweisung/Abort",
					new { controller = "PaymentSofortueberweisung", action = "Abort" },
					new[] { "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }
			);

			routes.MapRoute("Plugin.Payments.Sofortueberweisung.Notification",
					"Plugins/PaymentSofortueberweisung/Notification",
					new { controller = "PaymentSofortueberweisung", action = "Notification" },
					new[] { "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }
			);
		}
	}	// class
}