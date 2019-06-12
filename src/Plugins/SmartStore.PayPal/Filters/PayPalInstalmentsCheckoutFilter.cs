using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;

namespace SmartStore.PayPal.Filters
{
    public class PayPalInstalmentsCheckoutFilter : IActionFilter
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IPayPalService> _payPalService;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;

        public PayPalInstalmentsCheckoutFilter(
            ICommonServices services,
            Lazy<IPayPalService> payPalService,
            Lazy<IGenericAttributeService> genericAttributeService)
        {
            _services = services;
            _payPalService = payPalService;
            _genericAttributeService = genericAttributeService;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.ActionDescriptor == null || filterContext?.HttpContext?.Request == null)
            {
                return;
            }

            var action = filterContext.ActionDescriptor.ActionName;
            if (action.IsCaseInsensitiveEqual("Confirm"))
            {
                CreatePayment(filterContext);
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        private void CreatePayment(ActionExecutingContext context)
        {
            var store = _services.StoreContext.CurrentStore;
            var settings = _services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);

            var session = context.HttpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);
            session.PaymentId = null;

            var result = _payPalService.Value.EnsureAccessToken(session, settings);
            if (result.Success)
            {
                var urlHelper = new UrlHelper(context.HttpContext.Request.RequestContext);
                var customer = _services.WorkContext.CurrentCustomer;
                var protocol = store.SslEnabled ? "https" : "http";
                var returnUrl = urlHelper.Action("CheckoutReturn", "PayPalInstalments", new { area = Plugin.SystemName }, protocol);
                var cancelUrl = urlHelper.Action("CheckoutCancel", "PayPalInstalments", new { area = Plugin.SystemName }, protocol);
                var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

                result = _payPalService.Value.CreatePayment(settings, session, cart, returnUrl, cancelUrl);
                if (result == null)
                {
                    // No payment required.
                }
                else if (result.Success && result.Json != null)
                {
                    foreach (var link in result.Json.links)
                    {
                        if (((string)link.rel).IsCaseInsensitiveEqual("approval_url"))
                        {
                            session.PaymentId = result.Id;

                            context.Result = new RedirectResult(link.href, false);
                            break;
                        }
                    }
                }
                else
                {
                    _genericAttributeService.Value.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, store.Id);

                    var url = urlHelper.Action("PaymentMethod", "Checkout", new { area = "" });
                    context.Result = new RedirectResult(url, false);

                    _services.Notifier.Error(result.ErrorMessage);
                }
            }
        }
    }
}