using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Orders;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.ShoppingCart;

namespace SmartStore.PayPal.Filters
{
    public class PayPalExpressWidgetZoneFilter : IResultFilter
    {
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private readonly Lazy<IPaymentService> _paymentService;
        private readonly Lazy<ICommonServices> _services;
        private readonly Lazy<PayPalExpressPaymentSettings> _payPalExpressSettings;

        public PayPalExpressWidgetZoneFilter(
            Lazy<IWidgetProvider> widgetProvider,
            Lazy<IPaymentService> paymentService,
            Lazy<ICommonServices> services,
            Lazy<PayPalExpressPaymentSettings> payPalExpressSettings)
        {
            _widgetProvider = widgetProvider;
            _paymentService = paymentService;
            _services = services;
            _payPalExpressSettings = payPalExpressSettings;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            // Should only run on a full view rendering result.
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
            {
                return;
            }

            var controller = filterContext.RouteData.Values["controller"] as string;
            var action = filterContext.RouteData.Values["action"] as string;

            if (action.IsCaseInsensitiveEqual("OffCanvasShoppingCart") && controller.IsCaseInsensitiveEqual("ShoppingCart"))
            {
                var model = filterContext.Controller.ViewData.Model as MiniShoppingCartModel;

                if (model != null && model.DisplayCheckoutButton && _payPalExpressSettings.Value.ShowButtonInMiniShoppingCart)
                {
                    var store = _services.Value.StoreContext.CurrentStore;
                    var customer = _services.Value.WorkContext.CurrentCustomer;
                    var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

                    if (_paymentService.Value.IsPaymentMethodActive(PayPalExpressProvider.SystemName, customer, cart, store.Id))
                    {
                        _widgetProvider.Value.RegisterAction("offcanvas_cart_summary", "MiniShoppingCart", "PayPalExpress", new { area = "SmartStore.PayPal" });
                    }
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}