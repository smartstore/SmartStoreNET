using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Payments.Invoice.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.Plugin.Payments.Invoice
{
    /// <summary>
    /// Invoice payment processor
    /// </summary>
	public class InvoicePaymentProcessor : PaymentPluginBase, IConfigurable
    {
        #region Fields

        private readonly InvoicePaymentSettings _invoicePaymentSettings;
        private readonly ISettingService _settingService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public InvoicePaymentProcessor(InvoicePaymentSettings invoicePaymentSettings,
            ISettingService settingService,
			IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService)
        {
            this._invoicePaymentSettings = invoicePaymentSettings;
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
				_invoicePaymentSettings.AdditionalFee, _invoicePaymentSettings.AdditionalFeePercentage);
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
            controllerName = "PaymentInvoice";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.Invoice.Controllers" }, { "area", "Payments.Invoice" } };
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
            controllerName = "PaymentInvoice";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.Invoice.Controllers" }, { "area", "Payments.Invoice" } };
        }

        public override Type GetControllerType()
        {
            return typeof(PaymentInvoiceController);
        }

        public override void Install()
        {
            var settings = new InvoicePaymentSettings()
            {
                DescriptionText = "@Plugins.Payment.Invoice.PaymentInfoDescription"
            };
            _settingService.SaveSetting(settings);

            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }
        
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<InvoicePaymentSettings>();

            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Payments.Invoice", false);

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
