using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.OfflinePayment.Controllers;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
    public abstract class OfflinePaymentProviderBase<TSetting> : PaymentMethodBase
        where TSetting : PaymentSettingsBase, ISettings, new()
    {
        public ICommonServices CommonServices { get; set; }
        public IOrderTotalCalculationService OrderTotalCalculationService { get; set; }

        public override Type GetControllerType()
        {
            return typeof(OfflinePaymentController);
        }

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        protected abstract string GetActionPrefix();

        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var result = decimal.Zero;
            try
            {
                var settings = CommonServices.Settings.LoadSetting<TSetting>(CommonServices.StoreContext.CurrentStore.Id);

                result = this.CalculateAdditionalFee(OrderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "{0}Configure".FormatInvariant(GetActionPrefix());
            controllerName = "OfflinePayment";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.OfflinePayment" } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "{0}PaymentInfo".FormatInvariant(GetActionPrefix());
            controllerName = "OfflinePayment";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.OfflinePayment" } };
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }
    }
}