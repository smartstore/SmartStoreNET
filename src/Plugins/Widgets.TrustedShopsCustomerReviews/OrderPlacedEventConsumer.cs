using System;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Orders;
using SmartStore.Services.Stores;
using SmartStore.Core;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews
{
    public class OrderPlacedEventConsumer : IConsumer<MessageTokensAddedEvent<Token>>
    {
        private readonly TrustedShopsCustomerReviewsSettings _trustedShopsCustomerReviewsSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderService _orderService;
		private readonly IStoreContext _storeContext;
		private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;

        public OrderPlacedEventConsumer(TrustedShopsCustomerReviewsSettings trustedShopsCustomerReviewsSettings,
            IPluginFinder pluginFinder,
			IOrderService orderService,
			IStoreContext storeContext,
			ISettingService settingService,
            IWorkContext workContext,
            IStoreService storeService,
            IEventPublisher eventPublisher)
        {
            _trustedShopsCustomerReviewsSettings = trustedShopsCustomerReviewsSettings;
            _pluginFinder = pluginFinder;
            _orderService = orderService;
			_storeContext = storeContext;
			_settingService = settingService;
            _workContext = workContext;
            _storeService = storeService;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(MessageTokensAddedEvent<Token> messageTokenEvent)
        {
            //is enabled?
            if (!_trustedShopsCustomerReviewsSettings.DisplayReviewLinkInEmail)
                return;

            //is email for customer?
            if(!(messageTokenEvent.Message.Name == "OrderPlaced.CustomerNotification"))
                return;

            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Widgets.TrustedShopsCustomerReviews");
            if (pluginDescriptor == null)
                return;

			if (!(_storeContext.CurrentStore.Id == 0 ||
				_settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(_storeContext.CurrentStore.Id, true)))
				return;

            var plugin = pluginDescriptor.Instance() as TrustedShopsCustomerReviewsPlugin;
            if (plugin == null)
                return;
            
            //respect language of current customer
            var imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_de.gif";
            var language = _workContext.WorkingLanguage.LanguageCulture.Split(new Char[] { '-' })[0];

            switch (language)
            {
                case "en":
                    imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_en.gif";
                    break;
                case "es":
                    imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_es.gif";
                    break;
                case "fr":
                    imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_fr.gif";
                    break;
                case "pl":
                    imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_pl.gif";
                    break;
                default:
                    imageLink = "https://www.trustedshops.com/bewertung/widget/img/bewerten_de.gif";
                    break;
            }

            var link = "<a href=\"https://www.trustedshops.com/buyerrating/rate_{0}.html&buyerEmail={1}&shopOrderID={2}\" target=\"_blank\" title=\"{3}\" ><img alt=\"{4}\" src=\"{5}\"></a>";

            var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
                null, null, null, null, null, null, null, 0, 1).FirstOrDefault();

            var buyerMail = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_workContext.CurrentCustomer.BillingAddress.Email));
            var orderId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(order.Id.ToString()));

            link = link.FormatWith(_trustedShopsCustomerReviewsSettings.TrustedShopsId, buyerMail, orderId, _trustedShopsCustomerReviewsSettings.ShopName, _trustedShopsCustomerReviewsSettings.ShopName, imageLink);

            //scrape message for TrustedShops.CustomerReviews and add it if it doesn't exist
            var containsTrustedShopsToken = messageTokenEvent.Message.Body.IndexOf("%TrustedShops.CustomerReviews%");

            //if token doesn't exist add it to message template
            if (containsTrustedShopsToken == -1 && messageTokenEvent.Message.Name == "OrderPlaced.CustomerNotification") 
            {
                //place token at end of body
                messageTokenEvent.Message.Body = messageTokenEvent.Message.Body.Replace("</td>", "<p>%TrustedShops.CustomerReviews%</p>\n</td>");
            }

            // add MessageToken 
            messageTokenEvent.Tokens.Add(new Token("TrustedShops.CustomerReviews", link, true));
        }
    }
}