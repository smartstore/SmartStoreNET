using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework;
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
        private readonly Lazy<IPaymentService> _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly Lazy<IPluginFinder> _pluginFinder;

        public PayPalInstalmentsController(
            HttpContextBase httpContext,
            IPayPalService payPalService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            ICurrencyService currencyService,
            Lazy<IPaymentService> paymentService,
            IPriceFormatter priceFormatter,
            Lazy<IPluginFinder> pluginFinder)
            : base(payPalService)
        {
            _httpContext = httpContext;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _pluginFinder = pluginFinder;
        }

        protected override string ProviderSystemName => PayPalInstalmentsProvider.SystemName;

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
            FinancingOptions model = null;

            try
            {
                var store = Services.StoreContext.CurrentStore;
                var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);
                model = PayPalService.GetFinancingOptions(settings, session, "paymentinfo", decimal.Zero);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return PartialView(model ?? new FinancingOptions("paymentinfo"));
        }

        // Widget zone on checkout confirm page.
        [ChildActionOnly]
        public ActionResult OrderSummaryTotals()
        {
            try
            {
                var store = Services.StoreContext.CurrentStore;
                var language = Services.WorkContext.WorkingLanguage;
                var targetCurrency = Services.WorkContext.WorkingCurrency;
                var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);

                if (session.FinancingCosts == decimal.Zero || session.TotalInclFinancingCosts == decimal.Zero)
                {
                    var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                    var result = PayPalService.GetPayment(settings, session);
                    if (result.Success)
                    {
                        var rawCosts = (string)result.Json.credit_financing_offered.total_interest.value;
                        var rawTotal = (string)result.Json.credit_financing_offered.total_cost.value;

                        session.FinancingCosts = rawCosts.Convert<decimal>(CultureInfo.InvariantCulture);
                        session.TotalInclFinancingCosts = rawTotal.Convert<decimal>(CultureInfo.InvariantCulture);
                    }
                }

                var financingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(session.FinancingCosts, targetCurrency, store);
                var totalInclFinancingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(session.TotalInclFinancingCosts, targetCurrency, store);

                ViewBag.FinancingCosts = _priceFormatter.FormatPrice(financingCosts, true, targetCurrency, language, false, false);
                ViewBag.TotalInclFinancingCosts = _priceFormatter.FormatPrice(totalInclFinancingCosts, true, targetCurrency, language, false, false);

                return PartialView();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new EmptyResult();
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
                        var targetCurrency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode) ?? store.PrimaryStoreCurrency;

                        var financingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(orderAttribute.FinancingCosts, targetCurrency, store);
                        var totalInclFinancingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(orderAttribute.TotalInclFinancingCosts, targetCurrency, store);

                        ViewBag.FinancingCosts = _priceFormatter.FormatPrice(financingCosts, true, targetCurrency, language, false, false);
                        ViewBag.TotalInclFinancingCosts = _priceFormatter.FormatPrice(totalInclFinancingCosts, true, targetCurrency, language, false, false);

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

        // Redirect from PayPal to the shop.
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

        // Redirect from PayPal to the shop.
        public ActionResult CheckoutCancel()
        {
            var store = Services.StoreContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;
            var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);

            session.PayerId = session.PaymentId = session.ApprovalUrl = null;
            session.FinancingCosts = session.TotalInclFinancingCosts = decimal.Zero;

            _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, null, store.Id);

            return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
        }

        #region Promotion

        // Widget zone on product detail or cart page.
        [ChildActionOnly]
        public ActionResult Promotion(string origin, decimal amount)
        {
            try
            {
                var store = Services.StoreContext.CurrentStore;

                if (_pluginFinder.Value.IsPluginReady(Services.Settings, Plugin.SystemName, store.Id))
                {
                    if (_paymentService.Value.IsPaymentMethodActive(PayPalInstalmentsProvider.SystemName, store.Id))
                    {
                        var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                        var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);
                        var model = PayPalService.GetFinancingOptions(settings, session, origin, amount);

                        if (model != null)
                        {
                            return PartialView(model);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new EmptyResult();
        }

        // Ajax.
        public ActionResult PromotionPopup(string origin, decimal amount)
        {
            try
            {
                var store = Services.StoreContext.CurrentStore;

                if (!_pluginFinder.Value.IsPluginReady(Services.Settings, Plugin.SystemName, store.Id) ||
                    !_paymentService.Value.IsPaymentMethodActive(PayPalInstalmentsProvider.SystemName, store.Id))
                {
                    return HttpNotFound();
                }

                var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);
                var model = PayPalService.GetFinancingOptions(settings, session, origin, amount, PayPalPromotion.FinancingExample);

                if (model != null)
                {
                    return PartialView(model);
                }
                else
                {
                    return Content("<div class='alert alert-info'>{0}</div>".FormatInvariant(T("Plugins.Payments.PayPalInstalments.NoFinancingDetails")));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Content("<div class='alert alert-error'>{0}</div>".FormatInvariant(ex.ToAllMessages().HtmlEncode()));
            }
        }

        // Widget zones for promotion.
        //[ChildActionOnly]
        //public ActionResult Promote()
        //{
        //    return new EmptyResult();
        //}

        #endregion

        #region Admin

        [ChildActionOnly, AdminAuthorize, LoadSetting, AdminThemed]
        public ActionResult Configure(PayPalInstalmentsSettings settings, int storeScope)
        {
            var model = new PayPalInstalmentsConfigModel();
            MiniMapper.Map(settings, model);
            //model.PromotionWidgetZones = settings.PromotionWidgetZones.SplitSafe(",");

            model.ProductPagePromotions = settings.ProductPagePromotion.HasValue
                ? settings.ProductPagePromotion.Value.ToSelectList(true).ToList()
                : PayPalPromotion.FinancingExample.ToSelectList(false).ToList();

            model.CartPagePromotions = settings.CartPagePromotion.HasValue
                ? settings.CartPagePromotion.Value.ToSelectList(true).ToList()
                : PayPalPromotion.FinancingExample.ToSelectList(false).ToList();

            model.PaymentListPromotions = PayPalPromotion.FinancingExample.ToSelectList(false).ToList();

            PrepareConfigurationModel(model, storeScope);

            if (model.Lender == null)
            {
                var cs = Services.Settings.LoadSetting<CompanyInformationSettings>();
                model.Lender = $"{cs.CompanyName.EmptyNull()}, {cs.Street.EmptyNull()}, {cs.ZipCode.EmptyNull()} {cs.City.EmptyNull()}";
            }

            return View(model);
        }

        [HttpPost, ChildActionOnly, AdminAuthorize, AdminThemed]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(PayPalInstalmentsConfigModel model, FormCollection form)
        {
            Action<PayPalInstalmentsSettings> additionalMapping = (x) =>
            {
                //x.PromotionWidgetZones = string.Join(",", model.PromotionWidgetZones ?? new string[0]).NullEmpty();
            };

            if (!SaveConfigurationModel(model, form, additionalMapping))
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