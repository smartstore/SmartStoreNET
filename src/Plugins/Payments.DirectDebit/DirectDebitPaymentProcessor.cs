using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Payments.DirectDebit.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.Plugin.Payments.DirectDebit
{
    /// <summary>
    /// DirectDebit payment processor
    /// </summary>
	public class DirectDebitPaymentProcessor : PaymentPluginBase, IConfigurable
    {
        #region Fields

        private readonly DirectDebitPaymentSettings _directDebitPaymentSettings;
        private readonly ISettingService _settingService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public DirectDebitPaymentProcessor(DirectDebitPaymentSettings directDebitPaymentSettings,
            ISettingService settingService,
			IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService)
        {
            this._directDebitPaymentSettings = directDebitPaymentSettings;
            this._settingService = settingService;
			this._orderTotalCalculationService = orderTotalCalculationService;
            this._localizationService = localizationService;
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
            result.AllowStoringDirectDebit = true;
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
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
				_directDebitPaymentSettings.AdditionalFee, _directDebitPaymentSettings.AdditionalFeePercentage);
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
            controllerName = "PaymentDirectDebit";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.DirectDebit.Controllers" }, { "area", "Payments.DirectDebit" } };
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
            controllerName = "PaymentDirectDebit";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.DirectDebit.Controllers" }, { "area", "Payments.DirectDebit" } };
        }

        public override Type GetControllerType()
        {
            return typeof(PaymentDirectDebitController);
        }

        public override void Install()
        {
            var settings = new DirectDebitPaymentSettings()
            {
                DescriptionText = "@Plugins.Payments.DirectDebit.PaymentInfoDescription"
            };
            _settingService.SaveSetting(settings);

            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }
        
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<DirectDebitPaymentSettings>();

            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Payments.DirectDebit", false);

            base.Uninstall();
        }

        #endregion

        #region Properties

		public override bool RequiresInteraction
		{
			get
			{
				return true;
			}
		}

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
