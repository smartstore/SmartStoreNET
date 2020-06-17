using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Templating;
using SmartStore.Utilities;

namespace SmartStore.PayPal.Events
{
    public class InstalmentsMessageZoneEventConsumer : IConsumer
    {
        private static string[] _templateNames = new string[]
        {
            MessageTemplateNames.OrderCancelledCustomer,
            MessageTemplateNames.OrderCompletedCustomer,
            MessageTemplateNames.OrderPlacedCustomer,
            MessageTemplateNames.OrderPlacedStoreOwner
        };

        private readonly ICommonServices _services;
        private readonly HttpContextBase _httpContext;
        private readonly Lazy<IOrderService> _orderService;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<IPriceFormatter> _priceFormatter;

        public InstalmentsMessageZoneEventConsumer(
            ICommonServices services,
            HttpContextBase httpContext,
            Lazy<IOrderService> orderService,
            Lazy<IGenericAttributeService> genericAttributeService,
            Lazy<ICurrencyService> currencyService,
            Lazy<ILanguageService> languageService,
            Lazy<IPriceFormatter> priceFormatter)
        {
            _services = services;
            _httpContext = httpContext;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
            _currencyService = currencyService;
            _languageService = languageService;
            _priceFormatter = priceFormatter;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void HandleEvent(MessageModelCreatedEvent message)
        {
            if (!_templateNames.Contains(message.MessageContext.MessageTemplate.Name))
            {
                return;
            }

            try
            {
                dynamic model = message.Model;
                if (model?.Order == null)
                {
                    return;
                }

                var paymentMethod = (string)model.Order.PaymentMethodSystemName;
                if (!paymentMethod.IsCaseInsensitiveEqual(PayPalInstalmentsProvider.SystemName))
                {
                    return;
                }

                var orderId = (int)model.Order.ID;
                var storeId = (int)model.Order.StoreId;

                var order = _orderService.Value.GetOrderById(orderId);
                if (order == null)
                {
                    return;
                }

                // Note, the order attribute has probably not been created at this time!
                decimal? costs = null;
                decimal? total = null;
                var state = _httpContext.Session.SafeGetValue<CheckoutState>(CheckoutState.CheckoutStateSessionKey);

                if (state != null && state.CustomProperties.ContainsKey(PayPalInstalmentsProvider.SystemName))
                {
                    var session = state.CustomProperties.Get(PayPalInstalmentsProvider.SystemName) as PayPalSessionData;
                    costs = session.FinancingCosts;
                    total = session.TotalInclFinancingCosts;
                }
                else
                {
                    var str = _genericAttributeService.Value.GetAttribute<string>(nameof(Order), orderId, PayPalInstalmentsOrderAttribute.Key, storeId);
                    if (str.HasValue())
                    {
                        var orderAttribute = JsonConvert.DeserializeObject<PayPalInstalmentsOrderAttribute>(str);
                        costs = orderAttribute.FinancingCosts;
                        total = orderAttribute.TotalInclFinancingCosts;
                    }
                }

                if (costs.HasValue && total.HasValue)
                {
                    var store = _services.StoreService.GetStoreById(storeId);
                    var language = _languageService.Value.GetLanguageById(message.MessageContext.LanguageId ?? order.CustomerLanguageId);
                    var targetCurrency = _currencyService.Value.GetCurrencyByCode(order.CustomerCurrencyCode) ?? store.PrimaryStoreCurrency;

                    var convertedCosts = _currencyService.Value.ConvertFromPrimaryStoreCurrency(costs.Value, targetCurrency, store);
                    var convertedTotal = _currencyService.Value.ConvertFromPrimaryStoreCurrency(total.Value, targetCurrency, store);

                    var formattedFinancingCosts = _priceFormatter.Value.FormatPrice(convertedCosts, true, targetCurrency, language, false, false);
                    var formattedTotalInclFinancingCosts = _priceFormatter.Value.FormatPrice(convertedTotal, true, targetCurrency, language, false, false);

                    model.PayPalInstalments = new HybridExpando();
                    model.PayPalInstalments = new Dictionary<string, object>
                    {
                        ["FormattedFinancingCosts"] = formattedFinancingCosts,
                        ["FormattedTotalInclFinancingCosts"] = formattedTotalInclFinancingCosts,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void HandleEvent(ZoneRenderingEvent message)
        {
            if (!message.ZoneName.IsCaseInsensitiveEqual("order_summary_after"))
            {
                return;
            }

            try
            {
                var ppInstalments = message.Evaluate("PayPalInstalments");
                if (ppInstalments != null)
                {
                    var liquidPath = CommonHelper.MapPath("~/Plugins/SmartStore.PayPal/Views/PayPalInstalments/OrderDetails.liquid");
                    var content = File.ReadAllText(liquidPath);

                    message.InjectContent(content.EmptyNull().Trim());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}