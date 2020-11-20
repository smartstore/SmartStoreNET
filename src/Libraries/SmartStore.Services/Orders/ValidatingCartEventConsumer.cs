using System;
using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Services.Cart;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Orders
{
    public class ValidatingCartEventConsumer : IConsumer
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;

        public ValidatingCartEventConsumer(
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext)
        {
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
        }

        public void HandleEvent(ValidatingCartEvent message) // services // customer groups
        {
            // Default Order Totals restriction
            if (message.Customer == null || message.Warnings == null || message.Cart == null)
                throw new NullReferenceException("message of " + typeof(ValidatingCartEvent).ToString());

            var customerRoleIds = message.Customer.GetRoleIds();

            // Minimum order totals validation
            var (isAboveMinimumOrderTotal, orderTotalMinimum) = _orderProcessingService.IsAboveOrderTotalMinimum(message.Cart, customerRoleIds);
            if (!isAboveMinimumOrderTotal)
            {
                orderTotalMinimum = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalMinimum, _workContext.WorkingCurrency);

                message.Warnings.Add(string.Format(
                    _localizationService.GetResource("Checkout.MinOrderSubtotalAmount"),
                    _priceFormatter.FormatPrice(orderTotalMinimum, true, false))
                    );

                return;
            }

            // Maximum order totals validation
            var (isBelowOrderTotalMaximum, orderTotalMaximum) = _orderProcessingService.IsBelowOrderTotalMaximum(message.Cart, customerRoleIds);
            if (!isBelowOrderTotalMaximum)
            {
                orderTotalMaximum = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalMaximum, _workContext.WorkingCurrency);

                message.Warnings.Add(string.Format(
                   _localizationService.GetResource("Checkout.MaxOrderSubtotalAmount"),
                   _priceFormatter.FormatPrice(orderTotalMaximum, true, false))
                    );
            }
        }
    }
}