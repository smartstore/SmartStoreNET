using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Payments
{
    /// <summary>
    /// Payment service
    /// </summary>
    public partial class PaymentService : IPaymentService
    {
		#region Constants
		
		private const string PAYMENTMETHOD_ALL_KEY = "SmartStore.paymentmethod.all";
		
		#endregion

        #region Fields

		private readonly IRepository<PaymentMethod> _paymentMethodRepository;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly IProviderManager _providerManager;
		private readonly ICurrencyService _currencyService;
		private readonly ICommonServices _services;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;

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
			IRepository<PaymentMethod> paymentMethodRepository,
			PaymentSettings paymentSettings, 
			IPluginFinder pluginFinder,
            ShoppingCartSettings shoppingCartSettings,
			IProviderManager providerManager,
			ICurrencyService currencyService,
			ICommonServices services,
			IOrderTotalCalculationService orderTotalCalculationService)
        {
			this._paymentMethodRepository = paymentMethodRepository;
            this._paymentSettings = paymentSettings;
            this._pluginFinder = pluginFinder;
            this._shoppingCartSettings = shoppingCartSettings;
			this._providerManager = providerManager;
			this._currencyService = currencyService;
			this._services = services;
			this._orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

		#region Methods

		/// <summary>
        /// Load active payment methods
        /// </summary>
		/// <param name="customer">Filter payment methods by customer and apply payment method restrictions; null to load all records</param>
		/// <param name="cart">Filter payment methods by cart amount; null to load all records</param>
		/// <param name="storeId">Filter payment methods by store identifier; pass 0 to load all records</param>
		/// <param name="types">Filter payment methods by payment method types</param>
		/// <param name="provideFallbackMethod">Provide a fallback payment method if none is active</param>
        /// <returns>Payment methods</returns>
		public virtual IEnumerable<Provider<IPaymentMethod>> LoadActivePaymentMethods(
			Customer customer = null,
			IList<OrganizedShoppingCartItem> cart = null,
			int storeId = 0,
			PaymentMethodType[] types = null,
			bool provideFallbackMethod = true)
        {
			List<int> customerRoleIds = null;
			int? selectedShippingMethodId = null;
			decimal? orderSubTotal = null;
			decimal? orderTotal = null;
			IList<PaymentMethod> allMethods = null;
			IEnumerable<Provider<IPaymentMethod>> allProviders = null;

			if (types != null && types.Any())
				allProviders = LoadAllPaymentMethods(storeId).Where(x => types.Contains(x.Value.PaymentMethodType));
			else
				allProviders = LoadAllPaymentMethods(storeId);

			var activeProviders = allProviders
				.Where(p =>
				{
					if (!p.Value.IsActive || !_paymentSettings.ActivePaymentMethodSystemNames.Contains(p.Metadata.SystemName, StringComparer.InvariantCultureIgnoreCase))
						return false;

					if (customer != null)
					{
						if (allMethods == null)
							allMethods = GetAllPaymentMethods();

						var method = allMethods.FirstOrDefault(x => x.PaymentMethodSystemName.IsCaseInsensitiveEqual(p.Metadata.SystemName));
						if (method != null)
						{
							// method restricted by customer role id?
							var excludedRoleIds = method.ExcludedCustomerRoleIds.ToIntArray();
							if (excludedRoleIds.Any())
							{
								if (customerRoleIds == null)
									customerRoleIds = customer.CustomerRoles.Where(r => r.Active).Select(r => r.Id).ToList();

								if (customerRoleIds != null && !customerRoleIds.Except(excludedRoleIds).Any())
									return false;
							}

							// method restricted by selected shipping method?
							var excludedShippingMethodIds = method.ExcludedShippingMethodIds.ToIntArray();
							if (excludedShippingMethodIds.Any())
							{
								if (!selectedShippingMethodId.HasValue)
								{
									var selectedShipping = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, storeId);
									selectedShippingMethodId = (selectedShipping == null ? 0 : selectedShipping.ShippingMethodId);
								}

								if ((selectedShippingMethodId ?? 0) != 0 && excludedShippingMethodIds.Contains(selectedShippingMethodId.Value))
									return false;
							}

							// method restricted by country of selected billing or shipping address?
							var excludedCountryIds = method.ExcludedCountryIds.ToIntArray();
							if (excludedCountryIds.Any())
							{
								int countryId = 0;
								if (method.CountryExclusionContext == CountryRestrictionContextType.ShippingAddress)
									countryId = (customer.ShippingAddress != null ? (customer.ShippingAddress.CountryId ?? 0) : 0);
								else
									countryId = (customer.BillingAddress != null ? (customer.BillingAddress.CountryId ?? 0) : 0);

								if (countryId != 0 && excludedCountryIds.Contains(countryId))
									return false;
							}

							// method restricted by min\max order amount?
							if ((method.MinimumOrderAmount.HasValue || method.MaximumOrderAmount.HasValue) && cart != null)
							{
								decimal compareAmount = decimal.Zero;

								if (method.AmountRestrictionContext == AmountRestrictionContextType.SubtotalAmount)
								{
									if (!orderSubTotal.HasValue)
									{
										decimal orderSubTotalDiscountAmountBase = decimal.Zero;
										Discount orderSubTotalAppliedDiscount = null;
										decimal subTotalWithoutDiscountBase = decimal.Zero;
										decimal subTotalWithDiscountBase = decimal.Zero;

										_orderTotalCalculationService.GetShoppingCartSubTotal(cart, out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
											out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

										orderSubTotal = _currencyService.ConvertFromPrimaryStoreCurrency(subTotalWithoutDiscountBase, _services.WorkContext.WorkingCurrency);
									}

									compareAmount = orderSubTotal.Value;
								}
								else if (method.AmountRestrictionContext == AmountRestrictionContextType.TotalAmount)
								{
									if (!orderTotal.HasValue)
									{
										orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart) ?? decimal.Zero;

										orderTotal = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotal.Value, _services.WorkContext.WorkingCurrency);
									}

									compareAmount = orderTotal.Value;
								}

								if (method.MinimumOrderAmount.HasValue && compareAmount < method.MinimumOrderAmount.Value)
									return false;

								if (method.MaximumOrderAmount.HasValue && compareAmount > method.MaximumOrderAmount.Value)
									return false;
							}
						}
					}
					return true;
				});

			if (!activeProviders.Any() && provideFallbackMethod)
			{
				var fallbackMethod = allProviders.FirstOrDefault(x => x.IsPaymentMethodActive(_paymentSettings));

				if (fallbackMethod == null)
					fallbackMethod = allProviders.FirstOrDefault();

				if (fallbackMethod != null)
				{
					return new Provider<IPaymentMethod>[] { fallbackMethod };
				}
				else
				{
					if (DataSettings.DatabaseIsInstalled())
						throw Error.Application("At least one payment method provider is required to be active.");
				}
			}

			return activeProviders;
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
		/// Gets all payment method extra data
		/// </summary>
		/// <returns>List of payment method objects</returns>
		public virtual IList<PaymentMethod> GetAllPaymentMethods()
		{
			var methods = _paymentMethodRepository.TableUntracked.ToList();
			return methods;
		}

		/// <summary>
		/// Gets payment method extra data by system name
		/// </summary>
		/// <param name="systemName">Provider system name</param>
		/// <returns>Payment method entity</returns>
		public virtual PaymentMethod GetPaymentMethodBySystemName(string systemName)
		{
			if (systemName.HasValue())
			{
				return _paymentMethodRepository.Table.FirstOrDefault(x => x.PaymentMethodSystemName == systemName);
			}
			return null;
		}

		/// <summary>
		/// Insert payment method extra data
		/// </summary>
		/// <param name="paymentMethod">Payment method</param>
		public virtual void InsertPaymentMethod(PaymentMethod paymentMethod)
		{
			if (paymentMethod == null)
				throw new ArgumentNullException("paymentMethod");

			_paymentMethodRepository.Insert(paymentMethod);

			_services.EventPublisher.EntityInserted(paymentMethod);
		}

		/// <summary>
		/// Updates payment method extra data
		/// </summary>
		/// <param name="paymentMethod">Payment method</param>
		public virtual void UpdatePaymentMethod(PaymentMethod paymentMethod)
		{
			if (paymentMethod == null)
				throw new ArgumentNullException("paymentMethod");

			_paymentMethodRepository.Update(paymentMethod);

			_services.EventPublisher.EntityUpdated(paymentMethod);
		}

		/// <summary>
		/// Delete payment method extra data
		/// </summary>
		/// <param name="paymentMethod">Payment method</param>
		public virtual void DeletePaymentMethod(PaymentMethod paymentMethod)
		{
			if (paymentMethod == null)
				throw new ArgumentNullException("paymentMethod");

			_paymentMethodRepository.Delete(paymentMethod);

			_services.EventPublisher.EntityDeleted(paymentMethod);
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

			if (paymentMethod.Value.PaymentMethodType != PaymentMethodType.Redirection && paymentMethod.Value.PaymentMethodType != PaymentMethodType.StandardAndRedirection)
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

			return paymentMethod.GetAdditionalHandlingFee(cart, _shoppingCartSettings.RoundPricesDuringCalculation);
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
				result.AddError(_services.Localization.GetResource("Common.Payment.NoCaptureSupport"));
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
				result.AddError(_services.Localization.GetResource("Common.Payment.NoRefundSupport"));
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
				result.AddError(_services.Localization.GetResource("Common.Payment.NoVoidSupport"));
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
					result.AddError(_services.Localization.GetResource("Common.Payment.NoRecurringPaymentSupport"));
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
				result.AddError(_services.Localization.GetResource("Common.Payment.NoRecurringPaymentSupport"));
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
