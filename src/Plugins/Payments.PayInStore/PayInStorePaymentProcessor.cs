using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Payments.PayInStore.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.Plugin.Payments.PayInStore
{
    /// <summary>
    /// PayInStore payment processor
    /// </summary>
	public class PayInStorePaymentProcessor : PaymentPluginBase, IConfigurable
    {
        #region Fields
        private readonly PayInStorePaymentSettings _payInStorePaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        #endregion

        #region Ctor

        public PayInStorePaymentProcessor(PayInStorePaymentSettings payInStorePaymentSettings,
            ISettingService settingService,
            ILocalizationService localizationService,
			IOrderTotalCalculationService orderTotalCalculationService)
        {
            this._payInStorePaymentSettings = payInStorePaymentSettings;
            this._settingService = settingService;
            this._localizationService = localizationService;
			this._orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, _payInStorePaymentSettings.AdditionalFee, _payInStorePaymentSettings.AdditionalFeePercentage);
			return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPayInStore";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.PayInStore.Controllers" }, { "area", "Payments.PayInStore" } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayInStore";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.PayInStore.Controllers" }, { "area", "Payments.PayInStore" } };
        }

        public override Type GetControllerType()
        {
            return typeof(PaymentPayInStoreController);
        }

        public override void Install()
        {
            var settings = new PayInStorePaymentSettings()
            {
                DescriptionText = "@Plugins.Payment.PayInStore.PaymentInfoDescription"
            };
            _settingService.SaveSetting(settings);

            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);
            
            base.Install();
        }
        
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PayInStorePaymentSettings>();

            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Payments.PayInStore", false);
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public override PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        #endregion
        
    }
}
