using System;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Orders;
using SmartStore.Core;
using SmartStore.Services.Configuration;

namespace SmartStore.Clickatell
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly ClickatellSettings _clickatellSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderService _orderService;
		private readonly IStoreContext _storeContext;
		private readonly ISettingService _settingService;

        public OrderPlacedEventConsumer(ClickatellSettings clickatellSettings,
            IPluginFinder pluginFinder,
			IOrderService orderService,
			IStoreContext storeContext,
			ISettingService settingService)
        {
            this._clickatellSettings = clickatellSettings;
            this._pluginFinder = pluginFinder;
            this._orderService = orderService;
			this._storeContext = storeContext;
			this._settingService = settingService;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            //is enabled?
            if (!_clickatellSettings.Enabled)
                return;

            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.Clickatell");
            if (pluginDescriptor == null)
                return;

			if (!(_storeContext.CurrentStore.Id == 0 ||
				_settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(_storeContext.CurrentStore.Id, true)))
				return;

            var plugin = pluginDescriptor.Instance() as ClickatellSmsProvider;
            if (plugin == null)
                return;

            var order = eventMessage.Order;
            //send SMS
            if (plugin.SendSms(String.Format("New order '{0}' has been placed.", order.GetOrderNumber())))
            {
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = "\"Order placed\" SMS alert (to store owner) has been sent",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
        }
    }
}