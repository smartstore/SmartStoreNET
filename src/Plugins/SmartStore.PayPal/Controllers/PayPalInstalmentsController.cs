using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.PayPal.Controllers
{
    public class PayPalInstalmentsController : PayPalRestApiControllerBase<PayPalInstalmentsSettings>
    {
        private readonly HttpContextBase _httpContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;

        public PayPalInstalmentsController(
            HttpContextBase httpContext,
            IPayPalService payPalService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter) 
            : base(PayPalInstalmentsProvider.SystemName, payPalService)
        {
            _httpContext = httpContext;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            return new List<string>();
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest
            {
                OrderGuid = Guid.NewGuid()
            };

            return paymentInfo;
        }

        public ActionResult PaymentInfo()
        {
            return PartialView();
        }

        // Widget zone on order details (page and print).
        [ChildActionOnly]
        public ActionResult OrderDetails(int orderId, bool print)
        {
            try
            {
                var order = _orderService.GetOrderById(orderId);
                if (order != null && order.PaymentMethodSystemName.IsCaseInsensitiveEqual(PayPalInstalmentsProvider.SystemName))
                {
                    var str = order.GetAttribute<string>(PayPalInstalmentsOrderAttribute.Key, _genericAttributeService, order.StoreId);
                    if (str.HasValue())
                    {
                        // Get additional order values in primary store currency.
                        var orderAttribute = JsonConvert.DeserializeObject<PayPalInstalmentsOrderAttribute>(str);

                        // Convert into order currency.
                        var language = Services.WorkContext.WorkingLanguage;
                        var store = Services.StoreService.GetStoreById(order.StoreId) ?? Services.StoreContext.CurrentStore;
                        var currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode) ?? store.PrimaryStoreCurrency;

                        var financingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(orderAttribute.FinancingCosts, currency, store);
                        var totalInclFinancingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(orderAttribute.TotalInclFinancingCosts, currency, store);

                        ViewBag.FinancingCosts = _priceFormatter.FormatPrice(financingCosts, true, currency, language, false, false);
                        ViewBag.TotalInclFinancingCosts = _priceFormatter.FormatPrice(totalInclFinancingCosts, true, currency, language, false, false);

                        if (print)
                        {
                            return PartialView("OrderDetails.Print");
                        }

                        return PartialView();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new EmptyResult();
        }

        // Redirect from PayPal.
        public ActionResult CheckoutReturn(string paymentId, string PayerID)
        {
            // Request.QueryString:
            // paymentId: PAY-0TC88803RP094490KK4KM6AI, token (not the access token): EC-5P379249AL999154U, PayerID: 5L9K773HHJLPN

            var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);

            if (paymentId.HasValue() && session.PaymentId.IsEmpty())
            {
                session.PaymentId = paymentId;
            }

            session.PayerId = PayerID;

            return RedirectToAction("Confirm", "Checkout", new { area = "" });
        }

        // Redirect from PayPal.
        public ActionResult CheckoutCancel()
        {
            return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
        }

        #region Admin

        [ChildActionOnly, AdminAuthorize, LoadSetting, AdminThemed]
        public ActionResult Configure(PayPalInstalmentsSettings settings, int storeScope)
        {
            var model = new PayPalInstalmentsConfigModel();
            MiniMapper.Map(settings, model);
            PrepareConfigurationModel(model, storeScope);

            return View(model);
        }

        [HttpPost, ChildActionOnly, AdminAuthorize, AdminThemed]
        public ActionResult Configure(PayPalInstalmentsConfigModel model, FormCollection form)
        {
            if (!SaveConfigurationModel<PayPalInstalmentsSettings>(model, form))
            {
                var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
                var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(storeScope);

                return Configure(settings, storeScope);
            }

            return RedirectToConfiguration(PayPalInstalmentsProvider.SystemName, false);
        }

        #endregion
    }
}