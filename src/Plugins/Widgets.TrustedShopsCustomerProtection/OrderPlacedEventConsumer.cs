using System;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Stores;
using SmartStore.Core;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection
{
    public class OrderPlacedEventConsumer : IConsumer<MessageTokensAddedEvent<Token>>
    {
        private readonly TrustedShopsCustomerProtectionSettings _trustedShopsCustomerProtectionSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderService _orderService;
		private readonly IStoreContext _storeContext;
		private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly TrustedShopsCustomerProtection.com.trustedshops.protectionqa.ApplicationRequestService _applicationRequestServiceSandbox;
        private readonly TrustedShopsCustomerProtection.com.trustedshops.protection.ApplicationRequestService _applicationRequestServiceLive;

        public OrderPlacedEventConsumer(TrustedShopsCustomerProtectionSettings trustedShopsCustomerProtectionSettings,
            IPluginFinder pluginFinder,
			IOrderService orderService,
			IStoreContext storeContext,
			ISettingService settingService,
            IWorkContext workContext,
            IStoreService storeService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService)
        {
            _trustedShopsCustomerProtectionSettings = trustedShopsCustomerProtectionSettings;
            _pluginFinder = pluginFinder;
            _orderService = orderService;
			_storeContext = storeContext;
			_settingService = settingService;
            _workContext = workContext;
            _storeService = storeService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _applicationRequestServiceSandbox = new TrustedShopsCustomerProtection.com.trustedshops.protectionqa.ApplicationRequestService();
            _applicationRequestServiceLive = new TrustedShopsCustomerProtection.com.trustedshops.protection.ApplicationRequestService();
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(MessageTokensAddedEvent<Token> messageTokenEvent)
        {
            //is enabled?
            if (!_trustedShopsCustomerProtectionSettings.IsExcellenceMode)
                return;

            //is email for customer?
            if (!(messageTokenEvent.Message.Name == "OrderPlaced.CustomerNotification"))
                return;

            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Widgets.TrustedShopsCustomerProtection");
            if (pluginDescriptor == null)
                return;

			if (!(_storeContext.CurrentStore.Id == 0 ||
				_settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(_storeContext.CurrentStore.Id, true)))
				return;

            var plugin = pluginDescriptor.Instance() as TrustedShopsCustomerProtectionPlugin;
            if (plugin == null)
                return;

            var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
                null, null, null, null, null, null, null, 0, 1).FirstOrDefault();
            string buyerEmail = _workContext.CurrentCustomer.BillingAddress.Email;
            string amount = order.OrderSubtotalInclTax.ToString();
            string currency = order.CustomerCurrencyCode;
            string paymentType = TrustedShopsUtils.ConvertPaymentSystemNameToTrustedShopsCode(order.PaymentMethodSystemName);
            string customerId = _workContext.CurrentCustomer.Id.ToString();
            string orderId = order.Id.ToString();

            var tsProduct = order.OrderProductVariants.Where(x => x.ProductVariant.AdminComment == "TrustedShops-Product").FirstOrDefault();

            if (tsProduct != null)
            {
                long tsApplicationNumber = 0;
                if (_trustedShopsCustomerProtectionSettings.IsTestMode)
                {
                    tsApplicationNumber = _applicationRequestServiceSandbox.requestForProtectionV2(
                        _trustedShopsCustomerProtectionSettings.TrustedShopsId,
                        tsProduct.ProductVariant.Sku, order.OrderSubtotalInclTax, currency, paymentType, buyerEmail, customerId, orderId, DateTime.Now, "SmartStore.NET",
                        _trustedShopsCustomerProtectionSettings.UserName,
                        _trustedShopsCustomerProtectionSettings.Password
                    );
                }
                else
                {
                    tsApplicationNumber = _applicationRequestServiceLive.requestForProtectionV2(
                        _trustedShopsCustomerProtectionSettings.TrustedShopsId,
                        tsProduct.ProductVariant.Sku, order.OrderSubtotalInclTax, currency, paymentType, buyerEmail, customerId, orderId, DateTime.Now, "SmartStore.NET",
                        _trustedShopsCustomerProtectionSettings.UserName,
                        _trustedShopsCustomerProtectionSettings.Password
                    );
                }

                //scrape message for TrustedShops.CustomerProtection.ApplicationNUmber and add it if it doesn't exist
                var containsTrustedShopsToken = messageTokenEvent.Message.Body.IndexOf("%TrustedShops.CustomerProtection.ApplicationNumber%");

                //if token doesn't exist add it to message template
                if (containsTrustedShopsToken == -1 && messageTokenEvent.Message.Name == "OrderPlaced.CustomerNotification")
                {
                    //place token at end of body
                    messageTokenEvent.Message.Body = messageTokenEvent.Message.Body.Replace("</td>", "<p>%TrustedShops.CustomerProtection.ApplicationNumber%</p>\n</td>");
                }

                // add MessageToken 
                if (tsApplicationNumber > 0)
                {
                    messageTokenEvent.Tokens.Add(new Token("TrustedShops.CustomerProtection.ApplicationNumber", Convert.ToString(tsApplicationNumber), true));
                }
                else {
                    messageTokenEvent.Tokens.Add(new Token("TrustedShops.CustomerProtection.ApplicationNumber",
                        _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.Error.Heading") +
                        _localizationService.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.Error." + Convert.ToString(tsApplicationNumber))
                        , true));
                } 
            }
        }
    }
}