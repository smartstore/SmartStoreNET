using System;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;

namespace SmartStore.PayPal.Filters
{
    public class PayPalPlusWidgetZoneFilter : IResultFilter
    {
        private readonly Lazy<IWidgetProvider> _widgetProvider;

        public PayPalPlusWidgetZoneFilter(Lazy<IWidgetProvider> widgetProvider)
        {
            _widgetProvider = widgetProvider;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            // Should only run on a full view rendering result.
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
                return;

            var controller = filterContext.RouteData.Values["controller"] as string;
            var action = filterContext.RouteData.Values["action"] as string;

            if (action.IsCaseInsensitiveEqual("Completed") && controller.IsCaseInsensitiveEqual("Checkout"))
            {
                _widgetProvider.Value.RegisterAction("checkout_completed_top", "CheckoutCompleted", "PayPalPlus", new { area = Plugin.SystemName });
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
