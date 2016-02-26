using System;
using System.Web.Mvc;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.ShoppingCart;

namespace SmartStore.PayPal.Filters
{
	public class PayPalExpressWidgetZoneFilter : IActionFilter, IResultFilter
	{
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly Lazy<PayPalExpressPaymentSettings> _payPalExpressSettings;

		public PayPalExpressWidgetZoneFilter(
			Lazy<IWidgetProvider> widgetProvider,
			Lazy<PayPalExpressPaymentSettings> payPalExpressSettings)
		{
			_widgetProvider = widgetProvider;
			_payPalExpressSettings = payPalExpressSettings;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			// should only run on a full view rendering result
			var result = filterContext.Result as ViewResultBase;
			if (result == null)
				return;

			var controller = filterContext.RouteData.Values["controller"] as string;
			var action = filterContext.RouteData.Values["action"] as string;

			if (action.IsCaseInsensitiveEqual("FlyoutShoppingCart") && controller.IsCaseInsensitiveEqual("ShoppingCart"))
			{
				var model = filterContext.Controller.ViewData.Model as MiniShoppingCartModel;

				if (model != null && model.DisplayCheckoutButton && _payPalExpressSettings.Value.ShowButtonInMiniShoppingCart)
				{
					_widgetProvider.Value.RegisterAction("mini_shopping_cart_bottom", "MiniShoppingCart", "PayPalExpress", new { area = "SmartStore.PayPal" });
				}
			}
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}
}