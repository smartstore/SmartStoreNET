using System;
using System.Collections.Generic;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework;

namespace SmartStore.AmazonPay.Events
{
    public class EventConsumer : IConsumer
    {
        private readonly IPluginFinder _pluginFinder;
        private readonly ICommonServices _services;
        private readonly IOrderService _orderService;
        private readonly Lazy<IAmazonPayService> _amazonPayService;

        public EventConsumer(
            IPluginFinder pluginFinder,
            ICommonServices services,
            IOrderService orderService,
            Lazy<IAmazonPayService> amazonPayService)
        {
            _pluginFinder = pluginFinder;
            _services = services;
            _orderService = orderService;
            _amazonPayService = amazonPayService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void HandleEvent(MessageModelCreatedEvent message)
        {
            if (message.MessageContext.MessageTemplate.Name != MessageTemplateNames.OrderPlacedCustomer)
                return;

            var storeId = _services.StoreContext.CurrentStore.Id;

            if (!_pluginFinder.IsPluginReady(_services.Settings, AmazonPayPlugin.SystemName, storeId))
                return;

            dynamic model = message.Model;

            if (model.Order == null)
                return;

            var orderId = model.Order.ID;

            if (orderId is int id)
            {
                var order = _orderService.GetOrderById(id);

                var isAmazonPayment = (order != null && order.PaymentMethodSystemName.IsCaseInsensitiveEqual(AmazonPayPlugin.SystemName));
                var tokenValue = (isAmazonPayment ? _services.Localization.GetResource("Plugins.Payments.AmazonPay.BillingAddressMessageNote") : "");

                model.AmazonPay = new Dictionary<string, object>
                {
                    { "BillingAddressMessageNote", tokenValue }
                };
            }
        }

        public void HandleEvent(OrderPaidEvent eventMessage)
        {
            if (eventMessage?.Order == null)
                return;

            if (!eventMessage.Order.PaymentMethodSystemName.IsCaseInsensitiveEqual(AmazonPayPlugin.SystemName))
                return;

            if (!_pluginFinder.IsPluginReady(_services.Settings, AmazonPayPlugin.SystemName, eventMessage.Order.StoreId))
                return;

            try
            {
                var settings = _services.Settings.LoadSetting<AmazonPaySettings>(eventMessage.Order.StoreId);

                _amazonPayService.Value.CloseOrderReference(settings, eventMessage.Order);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }
    }
}