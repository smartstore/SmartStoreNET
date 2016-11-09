using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;

namespace SmartStore.PayPal.Filters
{
	public class PayPalPlusWidgetZoneFilter : IActionFilter, IResultFilter
	{
		private readonly Lazy<HttpContextBase> _httpContext;
		private readonly Lazy<IWidgetProvider> _widgetProvider;

        public PayPalPlusWidgetZoneFilter(
			Lazy<HttpContextBase> httpContext,
			Lazy<IWidgetProvider> widgetProvider)
		{
			_httpContext = httpContext;
			_widgetProvider = widgetProvider;
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

			if (action.IsCaseInsensitiveEqual("Completed") && controller.IsCaseInsensitiveEqual("Checkout"))
			{
				var instruct = _httpContext.Value.Session[PayPalPlusProvider.CheckoutCompletedKey] as string;

				if (instruct.HasValue())
				{
					_widgetProvider.Value.RegisterAction("checkout_completed_top", "CheckoutCompleted", "PayPalPlus", new { area = Plugin.SystemName });
				}
			}
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}
}
