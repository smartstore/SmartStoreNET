using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Orders;

namespace SmartStore.Clickatell
{
	public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
		private readonly ICommonServices _services;
		private readonly ClickatellSettings _clickatellSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderService _orderService;

        public OrderPlacedEventConsumer(
			ICommonServices services,
			ClickatellSettings clickatellSettings,
            IPluginFinder pluginFinder,
			IOrderService orderService)
        {
			_services = services;
            _clickatellSettings = clickatellSettings;
            _pluginFinder = pluginFinder;
            _orderService = orderService;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            if (!_clickatellSettings.Enabled)
                return;

            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(ClickatellSmsProvider.SystemName);
            if (descriptor == null)
                return;

			var storeId = _services.StoreContext.CurrentStore.Id;

			if (!(storeId == 0 || _services.Settings.GetSettingByKey<string>(descriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true)))
				return;

            var plugin = descriptor.Instance() as ClickatellSmsProvider;
            if (plugin == null)
                return;

			try
			{
				plugin.SendSms(T("Plugins.Sms.Clickatell.OrderPlacedMessage", eventMessage.Order.GetOrderNumber()));

				_orderService.AddOrderNote(eventMessage.Order, T("Plugins.Sms.Clickatell.SmsSentNote"));
			}
			catch { }
        }
    }
}