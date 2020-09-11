using System;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.PayPal.Filters
{
    public class PayPalInstalmentsCheckoutFilter : IActionFilter
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IPayPalService> _payPalService;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
        private readonly Lazy<IWidgetProvider> _widgetProvider;

        public PayPalInstalmentsCheckoutFilter(
            ICommonServices services,
            Lazy<IPayPalService> payPalService,
            Lazy<IGenericAttributeService> genericAttributeService,
            Lazy<IWidgetProvider> widgetProvider)
        {
            _services = services;
            _payPalService = payPalService;
            _genericAttributeService = genericAttributeService;
            _widgetProvider = widgetProvider;
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
                var store = _services.StoreContext.CurrentStore;
                var customer = _services.WorkContext.CurrentCustomer;
                var selectedMethod = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, store.Id);

                if (!selectedMethod.IsCaseInsensitiveEqual(PayPalInstalmentsProvider.SystemName))
                {
                    return;
                }

                var urlHelper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
                var settings = _services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                var session = filterContext.HttpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);

                var protocol = store.SslEnabled ? "https" : "http";
                var returnUrl = urlHelper.Action("CheckoutReturn", "PayPalInstalments", new { area = Plugin.SystemName }, protocol);
                var cancelUrl = urlHelper.Action("CheckoutCancel", "PayPalInstalments", new { area = Plugin.SystemName }, protocol);
                var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

                // PayPal review: start payment flow again if address has changed.
                var paymentData = _payPalService.Value.CreatePaymentData(settings, session, cart, returnUrl, cancelUrl);
                var serializedData = JsonConvert.SerializeObject(paymentData);
                var dataHash = serializedData.Hash(Encoding.ASCII, false);
                var hasEqualPaymentData = session.PaymentDataHash.HasValue() && session.PaymentDataHash == dataHash;

                if (session.ApprovalUrl.IsEmpty() || !hasEqualPaymentData)
                {
                    // Create payment and redirect to PayPal.
                    session.PayerId = session.PaymentId = session.ApprovalUrl = session.PaymentDataHash = null;
                    session.FinancingCosts = session.TotalInclFinancingCosts = decimal.Zero;

                    var result = _payPalService.Value.EnsureAccessToken(session, settings);
                    if (result.Success)
                    {
                        result = _payPalService.Value.CreatePayment(settings, session, paymentData);
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
                                    session.ApprovalUrl = link.href;
                                    session.PaymentDataHash = dataHash;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _services.Notifier.Error(result.ErrorMessage);
                        }
                    }

                    if (session.ApprovalUrl.HasValue())
                    {
                        filterContext.Result = new RedirectResult(session.ApprovalUrl, false);
                    }
                    else
                    {
                        _genericAttributeService.Value.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, store.Id);

                        _services.Notifier.Error(_services.Localization.GetResource("Plugins.SmartStore.PayPal.PaymentImpossible"));

                        filterContext.Result = new RedirectResult(urlHelper.Action("PaymentMethod", "Checkout", new { area = "" }), false);
                    }
                }
                else
                {
                    // Show instalments total and fees.
                    _widgetProvider.Value.RegisterAction("order_summary_totals_after", "OrderSummaryTotals", "PayPalInstalments", new { area = Plugin.SystemName });
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}