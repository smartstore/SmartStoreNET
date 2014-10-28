using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Payments
{
    /// <summary>
    /// Payment service
    /// </summary>
    public partial class PaymentService : IPaymentService
    {
        #region Fields

        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly IProviderManager _providerManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
		/// <param name="pluginService">Plugin service</param>
        public PaymentService(
			PaymentSettings paymentSettings, 
			IPluginFinder pluginFinder,
            ShoppingCartSettings shoppingCartSettings,
			ISettingService settingService,
			ILocalizationService localizationService,
			IProviderManager providerManager)
        {
            this._paymentSettings = paymentSettings;
            this._pluginFinder = pluginFinder;
            this._shoppingCartSettings = shoppingCartSettings;
			this._settingService = settingService;
			this._localizationService = localizationService;
			this._providerManager = providerManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active payment methods
        /// </summary>
        /// <param name="filterByCustomerId">Filter payment methods by customer; null to load all records</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Payment methods</returns>
		public virtual IEnumerable<Provider<IPaymentMethod>> LoadActivePaymentMethods(int? filterByCustomerId = null, int storeId = 0)
        {
			var allMethods = LoadAllPaymentMethods(storeId);
			var activeMethods = allMethods
				   .Where(p => _paymentSettings.ActivePaymentMethodSystemNames.Contains(p.Metadata.SystemName, StringComparer.InvariantCultureIgnoreCase));

			if (!activeMethods.Any())
			{
				var fallbackMethod = allMethods.FirstOrDefault();
				if (fallbackMethod != null)
				{
					_paymentSettings.ActivePaymentMethodSystemNames.Clear();
					_paymentSettings.ActivePaymentMethodSystemNames.Add(fallbackMethod.Metadata.SystemName);
					_settingService.SaveSetting(_paymentSettings);
					return new Provider<IPaymentMethod>[] { fallbackMethod };
				}
				else
				{
					if (DataSettings.DatabaseIsInstalled())
						throw Error.Application("At least one payment method provider is required to be active.");
				}
			}

			return activeMethods;
        }

		/// <summary>
		/// Determines whether a payment method is active\enabled for a shop
		/// </summary>
		public virtual bool IsPaymentMethodActive(string systemName, int storeId = 0)
		{
			var method = LoadPaymentMethodBySystemName(systemName, true, storeId);
			return method != null;
		}

        /// <summary>
        /// Load payment provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found payment provider</returns>
		public virtual Provider<IPaymentMethod> LoadPaymentMethodBySystemName(string systemName, bool onlyWhenActive = false, int storeId = 0)
        {
			var provider = _providerManager.GetProvider<IPaymentMethod>(systemName, storeId);
			if (provider != null && onlyWhenActive && !provider.IsPaymentMethodActive(_paymentSettings))
			{
				return null;
			}
			return provider;
        }

        /// <summary>
        /// Load all payment providers
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Payment providers</returns>
		public virtual IEnumerable<Provider<IPaymentMethod>> LoadAllPaymentMethods(int storeId = 0)
        {
			return _providerManager.GetAllProviders<IPaymentMethod>(storeId);
        }


		/// <summary>
		/// Pre process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Pre process payment result</returns>
		public virtual PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			if (processPaymentRequest.OrderTotal == decimal.Zero)
			{
				var result = new PreProcessPaymentResult();
				return result;
			}
			else
			{
				var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
				if (paymentMethod == null)
					throw new SmartException("Payment method couldn't be loaded");

				return paymentMethod.Value.PreProcessPayment(processPaymentRequest);
			}
		}

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public virtual ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult()
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }
            else
            {
                //We should strip out any white space or dash in the CC number entered.
                if (!String.IsNullOrWhiteSpace(processPaymentRequest.CreditCardNumber))
                {
                    processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace(" ", "");
                    processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace("-", "");
                }
                var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new SmartException("Payment method couldn't be loaded");
                return paymentMethod.Value.ProcessPayment(processPaymentRequest);
            }
        }

        /// <summary>
        /// Post process payment (e.g. used by payment gateways to redirect to a third-party URL).
		/// Called after an order has been placed or when customer re-post the payment.
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public virtual void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(postProcessPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new SmartException("Payment method couldn't be loaded");
            paymentMethod.Value.PostProcessPayment(postProcessPaymentRequest);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public virtual bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!_paymentSettings.AllowRePostingPayments)
                return false;

            var paymentMethod = LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            if (paymentMethod == null)
                return false; //Payment method couldn't be loaded (for example, was uninstalled)

			if (paymentMethod.Value.PaymentMethodType != PaymentMethodType.Redirection)
                return false;   //this option is available only for redirection payment methods

            if (order.Deleted)
                return false;  //do not allow for deleted orders

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;  //do not allow for cancelled orders

            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;  //payment status should be Pending

			return paymentMethod.Value.CanRePostProcessPayment(order);
        }



        /// <summary>
        /// Gets an additional handling fee of a payment method
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Additional handling fee</returns>
		public virtual decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart, string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return decimal.Zero;

			decimal result = paymentMethod.Value.GetAdditionalHandlingFee(cart);
            if (result < decimal.Zero)
                result = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                result = Math.Round(result, 2);
            return result;
        }



        /// <summary>
        /// Gets a value indicating whether capture is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether capture is supported</returns>
        public virtual bool SupportCapture(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
			return paymentMethod.Value.SupportCapture;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public virtual CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(capturePaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new SmartException("Payment method couldn't be loaded");

			try
			{
				return paymentMethod.Value.Capture(capturePaymentRequest);
			}
			catch (NotSupportedException)
			{
				var result = new CapturePaymentResult();
				result.AddError(_localizationService.GetResource("Common.Payment.NoCaptureSupport"));
				return result;
			}
			catch
			{
				throw;
			}
        }



        /// <summary>
        /// Gets a value indicating whether partial refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether partial refund is supported</returns>
        public virtual bool SupportPartiallyRefund(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
			return paymentMethod.Value.SupportPartiallyRefund;
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether refund is supported</returns>
        public virtual bool SupportRefund(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
			return paymentMethod.Value.SupportRefund;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(refundPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new SmartException("Payment method couldn't be loaded");

			try
			{
				return paymentMethod.Value.Refund(refundPaymentRequest);
			}
			catch (NotSupportedException)
			{
				var result = new RefundPaymentResult();
				result.AddError(_localizationService.GetResource("Common.Payment.NoRefundSupport"));
				return result;
			}
			catch
			{
				throw;
			}
        }



        /// <summary>
        /// Gets a value indicating whether void is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether void is supported</returns>
        public virtual bool SupportVoid(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
			return paymentMethod.Value.SupportVoid;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(voidPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new SmartException("Payment method couldn't be loaded");

			try
			{
				return paymentMethod.Value.Void(voidPaymentRequest);
			}
			catch (NotSupportedException)
			{
				var result = new VoidPaymentResult();
				result.AddError(_localizationService.GetResource("Common.Payment.NoVoidSupport"));
				return result;
			}
			catch
			{
				throw;
			}
        }



        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A recurring payment type of payment method</returns>
        public virtual RecurringPaymentType GetRecurringPaymentType(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RecurringPaymentType.NotSupported;
			return paymentMethod.Value.RecurringPaymentType;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public virtual ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult()
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }
            else
            {
                var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new SmartException("Payment method couldn't be loaded");

				try
				{
					return paymentMethod.Value.ProcessRecurringPayment(processPaymentRequest);
				}
				catch (NotSupportedException)
				{
					var result = new ProcessPaymentResult();
					result.AddError(_localizationService.GetResource("Common.Payment.NoRecurringPaymentSupport"));
					return result;
				}
				catch
				{
					throw;
				}
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest.Order.OrderTotal == decimal.Zero)
                return new CancelRecurringPaymentResult();

            var paymentMethod = LoadPaymentMethodBySystemName(cancelPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new SmartException("Payment method couldn't be loaded");

			try
			{
				return paymentMethod.Value.CancelRecurringPayment(cancelPaymentRequest);
			}
			catch (NotSupportedException)
			{
				var result = new CancelRecurringPaymentResult();
				result.AddError(_localizationService.GetResource("Common.Payment.NoRecurringPaymentSupport"));
				return result;
			}
			catch
			{
				throw;
			}
        }



        /// <summary>
        /// Gets a payment method type
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A payment method type</returns>
        public virtual PaymentMethodType GetPaymentMethodType(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return PaymentMethodType.Unknown;
			return paymentMethod.Value.PaymentMethodType;
        }

        /// <summary>
        /// Gets masked credit card number
        /// </summary>
        /// <param name="creditCardNumber">Credit card number</param>
        /// <returns>Masked credit card number</returns>
        public virtual string GetMaskedCreditCardNumber(string creditCardNumber)
        {
            if (String.IsNullOrEmpty(creditCardNumber))
                return string.Empty;

            if (creditCardNumber.Length <= 4)
                return creditCardNumber;

            string last4 = creditCardNumber.Substring(creditCardNumber.Length - 4, 4);
            string maskedChars = string.Empty;
            for (int i = 0; i < creditCardNumber.Length - 4; i++)
            {
                maskedChars += "*";
            }
            return maskedChars + last4;
        }

        #endregion
    }
}
