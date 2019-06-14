using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Stores;
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
using SmartStore.Web.Framework.UI;

namespace SmartStore.PayPal.Controllers
{
    public class PayPalInstalmentsController : PayPalRestApiControllerBase<PayPalInstalmentsSettings>
    {
        private readonly HttpContextBase _httpContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly Lazy<IPaymentService> _paymentService;
        private readonly IWidgetProvider _widgetProvider;
        private readonly IPriceFormatter _priceFormatter;
        private readonly Lazy<IPluginFinder> _pluginFinder;

        public PayPalInstalmentsController(
            HttpContextBase httpContext,
            IPayPalService payPalService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            ICurrencyService currencyService,
            Lazy<IPaymentService> paymentService,
            IWidgetProvider widgetProvider,
            IPriceFormatter priceFormatter,
            Lazy<IPluginFinder> pluginFinder) 
            : base(PayPalInstalmentsProvider.SystemName, payPalService)
        {
            _httpContext = httpContext;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _widgetProvider = widgetProvider;
            _priceFormatter = priceFormatter;
            _pluginFinder = pluginFinder;
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

        // Widget zone on checkout confirm page.
        [ChildActionOnly]
        public ActionResult FinancingDetails()
        {
            try
            {
                var store = Services.StoreContext.CurrentStore;
                var language = Services.WorkContext.WorkingLanguage;
                var currency = Services.WorkContext.WorkingCurrency;
                var session = _httpContext.GetPayPalState(PayPalInstalmentsProvider.SystemName);

                if (session.FinancingCosts == decimal.Zero || session.TotalInclFinancingCosts == decimal.Zero)
                {
                    var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);
                    var result = PayPalService.GetPayment(settings, session);
                    if (result.Success)
                    {
                        result.Json.ToString().Dump();
                        // TODO: get details.
                        //var total = (string)result.Json.....;
                        //session.FinancingCosts = ;
                        //session.TotalInclFinancingCosts = total.Convert<decimal>(CultureInfo.InvariantCulture);
                    }
                }

                var financingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(session.FinancingCosts, currency, store);
                var totalInclFinancingCosts = _currencyService.ConvertFromPrimaryStoreCurrency(session.TotalInclFinancingCosts, currency, store);

                ViewBag.FinancingCosts = _priceFormatter.FormatPrice(financingCosts, true, currency, language, false, false);
                ViewBag.TotalInclFinancingCosts = _priceFormatter.FormatPrice(totalInclFinancingCosts, true, currency, language, false, false);

                return PartialView();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new EmptyResult();
        }

        // Widget zone on product detail page.
        [ChildActionOnly]
        public ActionResult ProductPagePromotion(decimal price)
        {
            try
            {
                var store = Services.StoreContext.CurrentStore;
                var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(store.Id);

                if (settings.ProductPagePromotion.HasValue && settings.ClientId.HasValue() && settings.Secret.HasValue() && settings.IsAmountFinanceable(price))
                {
                    if (_pluginFinder.Value.IsPluginReady(Services.Settings, Plugin.SystemName, store.Id))
                    {
                        if (_paymentService.Value.IsPaymentMethodActive(PayPalInstalmentsProvider.SystemName, store.Id))
                        {
                            var model = new PromotionModel
                            {
                                Promotion = settings.ProductPagePromotion.Value
                            };

                            return PartialView("Promotion", model);
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

        // Widget zones for promotion.
        //[ChildActionOnly]
        //public ActionResult Promote()
        //{
        //    return new EmptyResult();
        //}

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
            //model.PromotionWidgetZones = settings.PromotionWidgetZones.SplitSafe(",");

            model.ProductPagePromotions = settings.ProductPagePromotion.HasValue
                ? settings.ProductPagePromotion.Value.ToSelectList(true).ToList()
                : PayPalPromotion.FinancingExample.ToSelectList(false).ToList();

            model.CartPagePromotions = settings.CartPagePromotion.HasValue
                ? settings.CartPagePromotion.Value.ToSelectList(true).ToList()
                : PayPalPromotion.FinancingExample.ToSelectList(false).ToList();

            PrepareConfigurationModel(model, storeScope);

            return View(model);
        }

        [HttpPost, ChildActionOnly, AdminAuthorize, AdminThemed]
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