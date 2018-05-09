using System.Linq;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework;
using System;

namespace SmartStore.AmazonPay.Events
{
	public class MessageTokenEventConsumer : IConsumer<MessageTokensAddedEvent<Token>>
	{
		private readonly IPluginFinder _pluginFinder;
		private readonly ICommonServices _services;
		private readonly IOrderService _orderService;

		public MessageTokenEventConsumer(
			IPluginFinder pluginFinder,
			ICommonServices services,
			IOrderService orderService)
		{
			_pluginFinder = pluginFinder;
			_services = services;
			_orderService = orderService;
		}

		public void HandleEvent(MessageTokensAddedEvent<Token> messageTokenEvent)
		{
			if (!messageTokenEvent.Message.Name.IsCaseInsensitiveEqual("OrderPlaced.CustomerNotification"))
				return;

			var storeId = _services.StoreContext.CurrentStore.Id;

			if (!_pluginFinder.IsPluginReady(_services.Settings, AmazonPayCore.SystemName, storeId))
				return;

            var orderId = messageTokenEvent.Tokens.Where(x => x.Key.Equals("Order.ID")).FirstOrDefault();
            var order = _orderService.GetOrderById(Convert.ToInt32(orderId.Value));

            var isAmazonPayment = (order != null && order.PaymentMethodSystemName.IsCaseInsensitiveEqual(AmazonPayCore.SystemName));
			var tokenValue = (isAmazonPayment ? _services.Localization.GetResource("Plugins.Payments.AmazonPay.BillingAddressMessageNote") : "");

			messageTokenEvent.Tokens.Add(new Token("SmartStore.AmazonPay.BillingAddressMessageNote", tokenValue));
		}
	}
}