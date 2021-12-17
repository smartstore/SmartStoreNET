using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Payments
{
    public partial class PaymentService : IPaymentService
    {
        private const string PAYMENT_METHODS_ALL_KEY = "SmartStore.paymentmethod.all-{0}-";
        private const string PAYMENT_METHODS_PATTERN_KEY = "SmartStore.paymentmethod.*";

        private readonly static object _lock = new object();
        private static IList<Type> _paymentMethodFilterTypes = null;

        private readonly IRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ICartRuleProvider _cartRuleProvider;
        private readonly IProviderManager _providerManager;
        private readonly ICommonServices _services;
        private readonly ITypeFinder _typeFinder;

        public PaymentService(
            IRepository<PaymentMethod> paymentMethodRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService,
            PaymentSettings paymentSettings,
            ICartRuleProvider cartRuleProvider,
            IProviderManager providerManager,
            ICommonServices services,
            ITypeFinder typeFinder)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _storeMappingRepository = storeMappingRepository;
            _storeMappingService = storeMappingService;
            _paymentSettings = paymentSettings;
            _cartRuleProvider = cartRuleProvider;
            _providerManager = providerManager;
            _services = services;
            _typeFinder = typeFinder;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public DbQuerySettings QuerySettings { get; set; } = DbQuerySettings.Default;

        #region Methods

        public virtual bool IsPaymentMethodActive(string systemName, int storeId = 0)
        {
            var method = LoadPaymentMethodBySystemName(systemName, true, storeId);
            return method != null;
        }

        public virtual bool IsPaymentMethodActive(
            string systemName,
            Customer customer = null,
            IList<OrganizedShoppingCartItem> cart = null,
            int storeId = 0)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            var activePaymentMethods = LoadActivePaymentMethods(customer, cart, storeId, null, false).ToList();
            var method = activePaymentMethods.FirstOrDefault(x => x.Metadata.SystemName.IsCaseInsensitiveEqual(systemName));

            return method != null;
        }

        public virtual IEnumerable<Provider<IPaymentMethod>> LoadActivePaymentMethods(
            Customer customer = null,
            IList<OrganizedShoppingCartItem> cart = null,
            int storeId = 0,
            PaymentMethodType[] types = null,
            bool provideFallbackMethod = true)
        {
            var filterRequest = new PaymentFilterRequest
            {
                Cart = cart,
                StoreId = storeId,
                Customer = customer
            };

            var allFilters = GetAllPaymentMethodFilters();
            var allProviders = types != null && types.Any()
                ? LoadAllPaymentMethods(storeId).Where(x => types.Contains(x.Value.PaymentMethodType))
                : LoadAllPaymentMethods(storeId);

            var paymentMethods = GetAllPaymentMethods(storeId);

            var activeProviders = allProviders
                .Where(p =>
                {
                    try
                    {
                        // Only active payment methods.
                        if (!p.Value.IsActive || !_paymentSettings.ActivePaymentMethodSystemNames.Contains(p.Metadata.SystemName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            return false;
                        }

                        // Rule sets.
                        if (paymentMethods.TryGetValue(p.Metadata.SystemName, out var pm))
                        {
                            if (!_cartRuleProvider.RuleMatches(pm))
                            {
                                return false;
                            }
                        }

                        filterRequest.PaymentMethod = p;

                        // Only payment methods that have not been filtered out.
                        if (allFilters.Any(x => x.IsExcluded(filterRequest)))
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    return true;
                });

            if (!activeProviders.Any() && provideFallbackMethod)
            {
                var fallbackMethod = allProviders.FirstOrDefault(x => x.IsPaymentMethodActive(_paymentSettings));
                if (fallbackMethod == null)
                {
                    fallbackMethod = allProviders.FirstOrDefault(x => x.Metadata?.PluginDescriptor?.SystemName?.IsCaseInsensitiveEqual("SmartStore.OfflinePayment") ?? false)
                        ?? allProviders.FirstOrDefault();
                }

                if (fallbackMethod != null)
                {
                    return new Provider<IPaymentMethod>[] { fallbackMethod };
                }

                if (DataSettings.DatabaseIsInstalled())
                {
                    throw new SmartException(T("Payment.OneActiveMethodProviderRequired"));
                }
            }

            return activeProviders;
        }

        public virtual Provider<IPaymentMethod> LoadPaymentMethodBySystemName(string systemName, bool onlyWhenActive = false, int storeId = 0)
        {
            var provider = _providerManager.GetProvider<IPaymentMethod>(systemName, storeId);
            if (provider != null)
            {
                if (onlyWhenActive && !provider.IsPaymentMethodActive(_paymentSettings))
                {
                    return null;
                }

                if (!QuerySettings.IgnoreMultiStore && storeId > 0)
                {
                    // Return provider if paymentMethod is null!
                    var paymentMethod = _paymentMethodRepository.TableUntracked.FirstOrDefault(x => x.PaymentMethodSystemName == systemName);
                    if (paymentMethod != null && !_storeMappingService.Authorize(paymentMethod, storeId))
                    {
                        return null;
                    }
                }
            }

            return provider;
        }

        public virtual IEnumerable<Provider<IPaymentMethod>> LoadAllPaymentMethods(int storeId = 0)
        {
            var providers = _providerManager.GetAllProviders<IPaymentMethod>(storeId);

            if (providers.Any() && !QuerySettings.IgnoreMultiStore && storeId > 0)
            {
                var unauthorizedMethods = _paymentMethodRepository.TableUntracked
                    .Where(x => x.LimitedToStores)
                    .ToList();

                var unauthorizedMethodNames = unauthorizedMethods
                    .Where(x => !_storeMappingService.Authorize(x, storeId))
                    .Select(x => x.PaymentMethodSystemName)
                    .ToList();

                return providers.Where(x => !unauthorizedMethodNames.Contains(x.Metadata.SystemName));
            }

            return providers;
        }

        public virtual IDictionary<string, PaymentMethod> GetAllPaymentMethods(int storeId = 0)
        {
            var result = _services.RequestCache.Get(PAYMENT_METHODS_ALL_KEY.FormatInvariant(storeId), () =>
            {
                var query = _paymentMethodRepository.TableUntracked;

                if (!QuerySettings.IgnoreMultiStore && storeId > 0)
                {
                    query =
                        from x in query
                        join sm in _storeMappingRepository.TableUntracked
                        on new { c1 = x.Id, c2 = "PaymentMethod" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
                        from sm in m_sm.DefaultIfEmpty()
                        where !x.LimitedToStores || storeId == sm.StoreId
                        select x;

                    query =
                        from x in query
                        group x by x.Id into mGroup
                        orderby mGroup.Key
                        select mGroup.FirstOrDefault();
                }

                var methods = query
                    .ToList()
                    .ToDictionarySafe(x => x.PaymentMethodSystemName.EmptyNull(), x => x, StringComparer.OrdinalIgnoreCase);

                return methods;
            });

            return result;
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
            Guard.NotNull(paymentMethod, nameof(paymentMethod));

            _paymentMethodRepository.Insert(paymentMethod);

            _services.RequestCache.RemoveByPattern(PAYMENT_METHODS_PATTERN_KEY);
        }

        /// <summary>
        /// Updates payment method extra data
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        public virtual void UpdatePaymentMethod(PaymentMethod paymentMethod)
        {
            Guard.NotNull(paymentMethod, nameof(paymentMethod));

            _paymentMethodRepository.Update(paymentMethod);

            _services.RequestCache.RemoveByPattern(PAYMENT_METHODS_PATTERN_KEY);
        }

        /// <summary>
        /// Delete payment method extra data
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        public virtual void DeletePaymentMethod(PaymentMethod paymentMethod)
        {
            if (paymentMethod != null)
            {
                _paymentMethodRepository.Delete(paymentMethod);

                _services.RequestCache.RemoveByPattern(PAYMENT_METHODS_PATTERN_KEY);
            }
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
                    throw new SmartException(T("Payment.CouldNotLoadMethod"));

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
                    throw new SmartException(T("Payment.CouldNotLoadMethod"));

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
            if (postProcessPaymentRequest.Order.PaymentMethodSystemName.HasValue())
            {
                var paymentMethod = LoadPaymentMethodBySystemName(postProcessPaymentRequest.Order.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new SmartException(T("Payment.CouldNotLoadMethod"));

                paymentMethod.Value.PostProcessPayment(postProcessPaymentRequest);
            }
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
            var paymentMethodAdditionalFee = (paymentMethod != null ? paymentMethod.Value.GetAdditionalHandlingFee(cart) : decimal.Zero);

            paymentMethodAdditionalFee = paymentMethodAdditionalFee.RoundIfEnabledFor(_services.WorkContext.WorkingCurrency);

            return paymentMethodAdditionalFee;
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
                throw new SmartException(T("Payment.CouldNotLoadMethod"));

            try
            {
                return paymentMethod.Value.Capture(capturePaymentRequest);
            }
            catch (NotSupportedException)
            {
                var result = new CapturePaymentResult();
                result.AddError(T("Common.Payment.NoCaptureSupport"));
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
                throw new SmartException(T("Payment.CouldNotLoadMethod"));

            try
            {
                return paymentMethod.Value.Refund(refundPaymentRequest);
            }
            catch (NotSupportedException)
            {
                var result = new RefundPaymentResult();
                result.AddError(T("Common.Payment.NoRefundSupport"));
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
                throw new SmartException(T("Payment.CouldNotLoadMethod"));

            try
            {
                return paymentMethod.Value.Void(voidPaymentRequest);
            }
            catch (NotSupportedException)
            {
                var result = new VoidPaymentResult();
                result.AddError(T("Common.Payment.NoVoidSupport"));
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
                    throw new SmartException(T("Payment.CouldNotLoadMethod"));

                try
                {
                    return paymentMethod.Value.ProcessRecurringPayment(processPaymentRequest);
                }
                catch (NotSupportedException)
                {
                    var result = new ProcessPaymentResult();
                    result.AddError(T("Common.Payment.NoRecurringPaymentSupport"));
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
                throw new SmartException(T("Payment.CouldNotLoadMethod"));

            try
            {
                return paymentMethod.Value.CancelRecurringPayment(cancelPaymentRequest);
            }
            catch (NotSupportedException)
            {
                var result = new CancelRecurringPaymentResult();
                result.AddError(T("Common.Payment.NoRecurringPaymentSupport"));
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

        public virtual IList<IPaymentMethodFilter> GetAllPaymentMethodFilters()
        {
            if (_paymentMethodFilterTypes == null)
            {
                lock (_lock)
                {
                    if (_paymentMethodFilterTypes == null)
                    {
                        _paymentMethodFilterTypes = _typeFinder.FindClassesOfType<IPaymentMethodFilter>(ignoreInactivePlugins: true).ToList();
                    }
                }
            }

            var paymentMethodFilters = _paymentMethodFilterTypes
                .Select(x => EngineContext.Current.ContainerManager.ResolveUnregistered(x) as IPaymentMethodFilter)
                .ToList();

            return paymentMethodFilters;
        }

        #endregion
    }
}
