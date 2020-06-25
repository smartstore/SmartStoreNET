using System;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalPaymentFilter : IPaymentMethodFilter
    {
        protected readonly ICommonServices _services;
        protected readonly Lazy<IOrderTotalCalculationService> _orderTotalCalculationService;

        public PayPalPaymentFilter(
            ICommonServices services,
            Lazy<IOrderTotalCalculationService> orderTotalCalculationService)
        {
            _services = services;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public bool IsExcluded(PaymentFilterRequest request)
        {
            if (request.Customer == null)
            {
                return false;
            }

            var method = request.PaymentMethod?.Metadata?.SystemName;

            if (method.IsCaseInsensitiveEqual(PayPalInstalmentsProvider.SystemName))
            {
                return IsPaymentExcluded(request);
            }

            return false;
        }

        protected bool CanPayOrderAmount(PayPalInstalmentsSettings settings, PaymentFilterRequest request)
        {
            var cartTotal = ((decimal?)_orderTotalCalculationService.Value.GetShoppingCartTotal(request.Cart)) ?? decimal.Zero;

            if (cartTotal == decimal.Zero)
            {
                return false;
            }

            return cartTotal >= settings.FinancingMin && cartTotal <= settings.FinancingMax;
        }

        protected bool IsPaymentExcluded(PaymentFilterRequest request)
        {
            var ba = request.Customer.BillingAddress;
            var sa = request.Customer.ShippingAddress;

            if (ba?.Country != null && !ba.Country.TwoLetterIsoCode.IsCaseInsensitiveEqual("DE"))
            {
                return true;
            }
            if (sa != null && sa.Country != null && !sa.Country.TwoLetterIsoCode.IsCaseInsensitiveEqual("DE"))
            {
                return true;
            }

            var settings = _services.Settings.LoadSetting<PayPalInstalmentsSettings>(request.StoreId);

            if (settings.ClientId.IsEmpty() || settings.Secret.IsEmpty())
            {
                return true;
            }

            if (!CanPayOrderAmount(settings, request))
            {
                return true;
            }

            return false;
        }
    }
}