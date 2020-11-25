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

        public void HandleEvent(ValidatingCartEvent message)
        {
            // Default Order Totals restriction
            var roleIds = _workContext.OriginalCustomerIfImpersonated?.GetRoleIds() ?? message.Customer.GetRoleIds();

            // Minimum order totals validation
            var (isAboveMin, min) = _orderProcessingService.IsAboveOrderTotalMinimum(message.Cart, roleIds);
            if (!isAboveMin)
            {
                min = _currencyService.ConvertFromPrimaryStoreCurrency(min, _workContext.WorkingCurrency);
                message.Warnings.Add(string.Format(
                    _localizationService.GetResource("Checkout.MinOrderSubtotalAmount"),
                    _priceFormatter.FormatPrice(min, true, false))
                    );

                return;
            }

            // Maximum order totals validation
            var (isBelowMax, max) = _orderProcessingService.IsBelowOrderTotalMaximum(message.Cart, roleIds);
            if (!isBelowMax)
            {
                max = _currencyService.ConvertFromPrimaryStoreCurrency(max, _workContext.WorkingCurrency);
                message.Warnings.Add(string.Format(
                   _localizationService.GetResource("Checkout.MaxOrderSubtotalAmount"),
                   _priceFormatter.FormatPrice(max, true, false))
                    );
            }
        }
    }
}