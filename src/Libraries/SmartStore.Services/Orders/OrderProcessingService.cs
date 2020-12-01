using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Orders
{
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IMessageFactory _messageFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IDownloadService _downloadService;

        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public OrderProcessingService(
            IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IMessageFactory messageFactory,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IDownloadService downloadService,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            _orderService = orderService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _languageService = languageService;
            _productService = productService;
            _paymentService = paymentService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _giftCardService = giftCardService;
            _shoppingCartService = shoppingCartService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _checkoutAttributeParser = checkoutAttributeParser;
            _workContext = workContext;
            _storeContext = storeContext;
            _messageFactory = messageFactory;
            _shippingService = shippingService;
            _shipmentService = shipmentService;
            _taxService = taxService;
            _customerService = customerService;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _customerActivityService = customerActivityService;
            _currencyService = currencyService;
            _affiliateService = affiliateService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _downloadService = downloadService;
            _rewardPointsSettings = rewardPointsSettings;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        #endregion

        #region Utilities

        protected string FormatTaxRates(SortedDictionary<decimal, decimal> taxRates)
        {
            var result = string.Empty;

            foreach (var rate in taxRates)
            {
                result += "{0}:{1};   ".FormatInvariant(
                    rate.Key.ToString(CultureInfo.InvariantCulture),
                    rate.Value.ToString(CultureInfo.InvariantCulture));
            }

            return result;
        }

        private void ProcessErrors(Order order, IList<string> errors, string messageKey)
        {
            if (errors.Any())
            {
                var msg = string.Concat(T(messageKey, order.GetOrderNumber()), " ", string.Join(" ", errors));

                _orderService.AddOrderNote(order, msg);
                Logger.Error(msg);
            }
        }

        /// <summary>
        /// Award reward points
        /// </summary>
        /// <param name="order">Order</param>
		/// <param name="amount">The amount. OrderTotal is used if null.</param>
		protected void AwardRewardPoints(Order order, decimal? amount = null)
        {
            if (!_rewardPointsSettings.Enabled)
                return;

            if (_rewardPointsSettings.PointsForPurchases_Amount <= decimal.Zero)
                return;

            //Ensure that reward points are applied only to registered users
            if (order.Customer == null || order.Customer.IsGuest())
                return;

            //Ensure that reward points were not added before. We should not add reward points if they were already earned for this order
            if (order.RewardPointsWereAdded)
                return;

            // Truncate same as Floor for positive amounts
            var points = (int)Math.Truncate((amount ?? order.OrderTotal) / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);
            if (points == 0)
                return;

            //add reward points
            order.Customer.AddRewardPointsHistoryEntry(points, T("RewardPoints.Message.EarnedForOrder", order.GetOrderNumber()));
            order.RewardPointsWereAdded = true;

            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Reduce reward points
        /// </summary>
        /// <param name="order">Order</param>
		/// <param name="amount">The amount. OrderTotal is used if null.</param>
        protected void ReduceRewardPoints(Order order, decimal? amount = null)
        {
            if (!_rewardPointsSettings.Enabled)
                return;

            if (_rewardPointsSettings.PointsForPurchases_Amount <= decimal.Zero)
                return;

            //ensure that reward points were already earned for this order before
            if (!order.RewardPointsWereAdded)
                return;

            //Ensure that reward points are applied only to registered users
            if (order.Customer == null || order.Customer.IsGuest())
                return;

            // Truncate increases the risk of inaccuracy of rounding
            //int points = (int)Math.Truncate((amount ?? order.OrderTotal) / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);

            int points = (int)Math.Round((amount ?? order.OrderTotal) / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);

            if (order.RewardPointsRemaining.HasValue && order.RewardPointsRemaining.Value < points)
                points = order.RewardPointsRemaining.Value;

            if (points == 0)
                return;

            //reduce reward points
            order.Customer.AddRewardPointsHistoryEntry(-points, T("RewardPoints.Message.ReducedForOrder", order.GetOrderNumber()));

            if (!order.RewardPointsRemaining.HasValue)
                order.RewardPointsRemaining = (int)Math.Round(order.OrderTotal / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);

            order.RewardPointsRemaining = Math.Max(order.RewardPointsRemaining.Value - points, 0);

            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Set IsActivated value for purchase gift cards for particular order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="activate">A value indicating whether to activate gift cards; true - actuvate, false - deactivate</param>
        protected void SetActivatedValueForPurchasedGiftCards(Order order, bool activate)
        {
            var giftCards = _giftCardService.GetAllGiftCards(order.Id, null, null, !activate);

            foreach (var gc in giftCards)
            {
                if (activate)
                {
                    //activate
                    var isRecipientNotified = gc.IsRecipientNotified;

                    if (gc.GiftCardType == GiftCardType.Virtual)
                    {
                        // Send email for virtual gift card
                        if (!String.IsNullOrEmpty(gc.RecipientEmail) && !String.IsNullOrEmpty(gc.SenderEmail))
                        {
                            var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
                            if (customerLang == null)
                                customerLang = _languageService.GetAllLanguages().FirstOrDefault();

                            var qe = _messageFactory.SendGiftCardNotification(gc, customerLang.Id);
                            isRecipientNotified = qe?.Email.Id != null;
                        }
                    }

                    gc.IsGiftCardActivated = true;
                    gc.IsRecipientNotified = isRecipientNotified;

                    _giftCardService.UpdateGiftCard(gc);
                }
                else
                {
                    //deactivate
                    gc.IsGiftCardActivated = false;

                    _giftCardService.UpdateGiftCard(gc);
                }
            }
        }

        /// <summary>
        /// Sets an order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="os">New order status</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        protected void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            OrderStatus prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
                return;

            //set and save new order status
            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderStatusChanged", os.GetLocalizedEnum(_localizationService)));

            if (prevOrderStatus != OrderStatus.Complete && os == OrderStatus.Complete && notifyCustomer)
            {
                //notification
                var msg = _messageFactory.SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId);
                if (msg?.Email?.Id != null)
                {
                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerCompletedEmailQueued", msg.Email.Id));
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled && os == OrderStatus.Cancelled && notifyCustomer)
            {
                //notification
                var msg = _messageFactory.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (msg?.Email?.Id != null)
                {
                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerCancelledEmailQueued", msg.Email.Id));
                }
            }

            //reward points
            if (_rewardPointsSettings.PointsForPurchases_Awarded == order.OrderStatus)
            {
                AwardRewardPoints(order);
            }
            if (_rewardPointsSettings.PointsForPurchases_Canceled == order.OrderStatus)
            {
                ReduceRewardPoints(order);
            }

            //gift cards activation
            if (_orderSettings.GiftCards_Activated_OrderStatusId > 0 && _orderSettings.GiftCards_Activated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, true);
            }

            //gift cards deactivation
            if (_orderSettings.GiftCards_Deactivated_OrderStatusId > 0 && _orderSettings.GiftCards_Deactivated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, false);
            }
        }

        /// <summary>
        /// Checks order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Validated order</returns>
        public void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.PaymentStatus == PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                //ensure that paid date is set
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.PaymentStatus == PaymentStatus.Authorized || order.PaymentStatus == PaymentStatus.Paid)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus == OrderStatus.Pending)
            {
                if (order.ShippingStatus == ShippingStatus.PartiallyShipped || order.ShippingStatus == ShippingStatus.Shipped || order.ShippingStatus == ShippingStatus.Delivered)
                {
                    SetOrderStatus(order, OrderStatus.Processing, false);
                }
            }

            if (order.OrderStatus != OrderStatus.Cancelled && order.OrderStatus != OrderStatus.Complete)
            {
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    if (order.ShippingStatus == ShippingStatus.ShippingNotRequired || order.ShippingStatus == ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Complete, true);
                    }
                }
            }
        }

        #endregion

        #region Methods

        public virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer, Store store)
        {
            Guard.NotNull(customer, nameof(customer));
            Guard.NotNull(store, nameof(store));

            // Prevent 2 orders being placed within an X seconds time frame.
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
            {
                return true;
            }

            var lastOrder = _orderService.SearchOrders(store.Id, customer.Id, null, null, null, null, null, null, null, null, 0, 1).FirstOrDefault();
            if (lastOrder == null)
            {
                return true;
            }

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        public virtual IList<string> GetOrderPlacementWarnings(ProcessPaymentRequest processPaymentRequest)
        {
            Guard.NotNull(processPaymentRequest, nameof(processPaymentRequest));

            var initialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            return GetOrderPlacementWarnings(processPaymentRequest, initialOrder, customer, out var cart);
        }

        public virtual IList<string> GetOrderPlacementWarnings(
            ProcessPaymentRequest processPaymentRequest,
            Order initialOrder,
            Customer customer,
            out IList<OrganizedShoppingCartItem> cart)
        {
            Guard.NotNull(processPaymentRequest, nameof(processPaymentRequest));

            cart = null;

            var warnings = new List<string>();
            var skipPaymentWorkflow = false;
            var isRecurringShoppingCart = false;
            var paymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName;

            if (customer == null)
            {
                warnings.Add(T("Customer.DoesNotExist"));
                return warnings;
            }

            // Check whether guest checkout is allowed.
            if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
            {
                warnings.Add(T("Checkout.AnonymousNotAllowed"));
                return warnings;
            }

            if (!processPaymentRequest.IsRecurringPayment)
            {
                if (processPaymentRequest.ShoppingCartItemIds.Any())
                {
                    cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId)
                        .Where(x => processPaymentRequest.ShoppingCartItemIds.Contains(x.Item.Id))
                        .ToList();
                }
                else
                {
                    cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);
                }

                if (!cart.Any())
                {
                    warnings.Add(T("ShoppingCart.CartIsEmpty"));
                    return warnings;
                }

                // Validate the entire shopping cart.
                var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes), true);
                if (cartWarnings.Any())
                {
                    warnings.AddRange(cartWarnings);
                    return warnings;
                }

                // Validate individual cart items.
                foreach (var sci in cart)
                {
                    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(customer, sci.Item.ShoppingCartType,
                        sci.Item.Product, processPaymentRequest.StoreId, sci.Item.AttributesXml,
                        sci.Item.CustomerEnteredPrice, sci.Item.Quantity, false, childItems: sci.ChildItems);

                    if (sciWarnings.Any())
                    {
                        warnings.AddRange(sciWarnings);
                        return warnings;
                    }
                }

                // Order total minimum validation
                var customerRoleIds = customer.GetRoleIds();
                var (isAboveOrderTotalMinimum, orderTotalMinimum) = IsAboveOrderTotalMinimum(cart, customerRoleIds);
                if (!isAboveOrderTotalMinimum)
                {
                    orderTotalMinimum = _currencyService.ConvertFromPrimaryStoreCurrency(
                        orderTotalMinimum,
                        _workContext.WorkingCurrency);

                    var msg = T("Checkout.MinOrderSubtotalAmount", _priceFormatter.FormatPrice(orderTotalMinimum, true, false));
                    warnings.Add(msg);
                    return warnings;
                }

                // Order total maximum validation
                var (isBelowOrderTotalMaximum, orderTotalMaximum) = IsBelowOrderTotalMaximum(cart, customerRoleIds);
                if (isAboveOrderTotalMinimum && !isBelowOrderTotalMaximum)
                {
                    orderTotalMaximum = _currencyService.ConvertFromPrimaryStoreCurrency(
                        orderTotalMaximum,
                        _workContext.WorkingCurrency);

                    var msg = T("Checkout.MaxOrderSubtotalAmount", _priceFormatter.FormatPrice(orderTotalMaximum, true, false));
                    warnings.Add(msg);
                    return warnings;
                }

                // Total validations
                var orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, true, out var orderShippingTaxRate, out var shippingTotalDiscount);
                var orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, false);

                if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                {
                    warnings.Add(T("Order.CannotCalculateShippingTotal"));
                    return warnings;
                }

                var cartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                if (!cartTotal.TotalAmount.HasValue)
                {
                    warnings.Add(T("Order.CannotCalculateOrderTotal"));
                    return warnings;
                }

                skipPaymentWorkflow = cartTotal.TotalAmount.Value == decimal.Zero;

                // Address validations.
                if (customer.BillingAddress == null)
                {
                    warnings.Add(T("Order.BillingAddressMissing"));
                }
                else if (!customer.BillingAddress.Email.IsEmail())
                {
                    warnings.Add(T("Common.Error.InvalidEmail"));
                }
                else if (customer.BillingAddress.Country != null && !customer.BillingAddress.Country.AllowsBilling)
                {
                    warnings.Add(T("Order.CountryNotAllowedForBilling", customer.BillingAddress.Country.Name));
                }

                if (cart.RequiresShipping())
                {
                    if (customer.ShippingAddress == null)
                    {
                        warnings.Add(T("Order.ShippingAddressMissing"));
                        throw new SmartException();
                    }
                    else if (!customer.ShippingAddress.Email.IsEmail())
                    {
                        warnings.Add(T("Common.Error.InvalidEmail"));
                    }
                    else if (customer.ShippingAddress.Country != null && !customer.ShippingAddress.Country.AllowsShipping)
                    {
                        warnings.Add(T("Order.CountryNotAllowedForShipping", customer.ShippingAddress.Country.Name));
                    }
                }
            }
            else
            {
                // Recurring order.
                if (initialOrder == null)
                {
                    warnings.Add(T("Order.InitialOrderDoesNotExistForRecurringPayment"));
                    return warnings;
                }

                var cartTotal = new ShoppingCartTotal(initialOrder.OrderTotal);
                skipPaymentWorkflow = cartTotal.TotalAmount.Value == decimal.Zero;
                paymentMethodSystemName = initialOrder.PaymentMethodSystemName;

                // Address validations.
                if (initialOrder.BillingAddress == null)
                {
                    warnings.Add(T("Order.BillingAddressMissing"));
                }
                else if (initialOrder.BillingAddress.Country != null && !initialOrder.BillingAddress.Country.AllowsBilling)
                {
                    warnings.Add(T("Order.CountryNotAllowedForBilling", initialOrder.BillingAddress.Country.Name));
                }

                if (initialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    if (initialOrder.ShippingAddress == null)
                    {
                        warnings.Add(T("Order.ShippingAddressMissing"));
                    }
                    else if (initialOrder.ShippingAddress.Country != null && !initialOrder.ShippingAddress.Country.AllowsShipping)
                    {
                        warnings.Add(T("Order.CountryNotAllowedForShipping", initialOrder.ShippingAddress.Country.Name));
                    }
                }
            }

            // Payment.
            if (!warnings.Any() && !skipPaymentWorkflow)
            {
                var isPaymentMethodActive = _paymentService.IsPaymentMethodActive(paymentMethodSystemName, customer, cart, processPaymentRequest.StoreId);
                if (!isPaymentMethodActive)
                {
                    warnings.Add(T("Payment.MethodNotAvailable"));
                }
            }

            // Recurring or standard shopping cart?
            if (!warnings.Any() && !processPaymentRequest.IsRecurringPayment)
            {
                isRecurringShoppingCart = cart.IsRecurring();
                if (isRecurringShoppingCart)
                {
                    var recurringCyclesError = cart.GetRecurringCycleInfo(_localizationService, out var recurringCycleLength, out var recurringCyclePeriod, out var recurringTotalCycles);
                    if (recurringCyclesError.HasValue())
                    {
                        warnings.Add(recurringCyclesError);
                    }
                }
            }
            else
            {
                isRecurringShoppingCart = true;
            }

            // Validate recurring payment type.
            if (!warnings.Any() && !skipPaymentWorkflow && !processPaymentRequest.IsMultiOrder)
            {
                RecurringPaymentType? recurringPaymentType = null;

                if (!processPaymentRequest.IsRecurringPayment)
                {
                    if (isRecurringShoppingCart)
                    {
                        recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                    }
                }
                else
                {
                    if (isRecurringShoppingCart)
                    {
                        recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                    }
                    else
                    {
                        warnings.Add(T("Order.NoRecurringProducts"));
                    }
                }

                if (recurringPaymentType.HasValue)
                {
                    switch (recurringPaymentType.Value)
                    {
                        case RecurringPaymentType.NotSupported:
                            warnings.Add(T("Payment.RecurringPaymentNotSupported"));
                            break;
                        case RecurringPaymentType.Manual:
                        case RecurringPaymentType.Automatic:
                            break;
                        default:
                            warnings.Add(T("Payment.RecurringPaymentTypeUnknown"));
                            break;
                    }
                }
            }

            return warnings;
        }

        public virtual PlaceOrderResult PlaceOrder(
            ProcessPaymentRequest processPaymentRequest,
            Dictionary<string, string> extraData)
        {
            // Think about moving functionality of processing recurring orders (after the initial order was placed) to ProcessNextRecurringPayment() method.
            Guard.NotNull(processPaymentRequest, nameof(processPaymentRequest));

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
            }

            var result = new PlaceOrderResult();
            var utcNow = DateTime.UtcNow;

            try
            {
                var initialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

                var warnings = GetOrderPlacementWarnings(processPaymentRequest, initialOrder, customer, out var cart);
                if (warnings.Any())
                {
                    result.Errors.AddRange(warnings);
                    Logger.Warn(string.Join(" ", result.Errors));
                    return result;
                }

                #region Order details

                if (processPaymentRequest.IsRecurringPayment)
                {
                    processPaymentRequest.PaymentMethodSystemName = initialOrder.PaymentMethodSystemName;
                }

                // Affilites.
                var affiliateId = 0;
                var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
                if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                {
                    affiliateId = affiliate.Id;
                }

                // Customer currency.
                var customerCurrencyCode = string.Empty;
                var customerCurrencyRate = decimal.Zero;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    var currencyTmp = _currencyService.GetCurrencyById(customer.GetAttribute<int>(SystemCustomerAttributeNames.CurrencyId, processPaymentRequest.StoreId));
                    var customerCurrency = (currencyTmp != null && currencyTmp.Published) ? currencyTmp : _workContext.WorkingCurrency;
                    customerCurrencyCode = customerCurrency.CurrencyCode;

                    var primaryStoreCurrency = _storeContext.CurrentStore.PrimaryStoreCurrency;
                    customerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;
                }
                else
                {
                    customerCurrencyCode = initialOrder.CustomerCurrencyCode;
                    customerCurrencyRate = initialOrder.CurrencyRate;
                }

                // Customer language.
                Language customerLanguage = null;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    customerLanguage = _languageService.GetLanguageById(customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
                }
                else
                {
                    customerLanguage = _languageService.GetLanguageById(initialOrder.CustomerLanguageId);
                }

                if (customerLanguage == null || !customerLanguage.Published)
                {
                    customerLanguage = _workContext.WorkingLanguage;
                }

                // Tax display type.
                var customerTaxDisplayType = TaxDisplayType.IncludingTax;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    customerTaxDisplayType = _workContext.GetTaxDisplayTypeFor(customer, processPaymentRequest.StoreId);
                }
                else
                {
                    customerTaxDisplayType = initialOrder.CustomerTaxDisplayType;
                }

                // Checkout attributes.
                string checkoutAttributeDescription, checkoutAttributesXml;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes);
                    checkoutAttributeDescription = _checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, customer);
                }
                else
                {
                    checkoutAttributesXml = initialOrder.CheckoutAttributesXml;
                    checkoutAttributeDescription = initialOrder.CheckoutAttributeDescription;
                }

                // Applied discount (used to store discount usage history).
                var appliedDiscounts = new List<Discount>();
                decimal orderSubTotalInclTax, orderSubTotalExclTax;
                decimal orderSubTotalDiscountInclTax = 0, orderSubTotalDiscountExclTax = 0;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    // Sub total (incl tax).
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, true,
                        out var orderSubTotalDiscountAmount1, out var orderSubTotalAppliedDiscount1, out var subTotalWithoutDiscountBase1, out var subTotalWithDiscountBase1);

                    orderSubTotalInclTax = subTotalWithoutDiscountBase1;
                    orderSubTotalDiscountInclTax = orderSubTotalDiscountAmount1;

                    // Discount history.
                    if (orderSubTotalAppliedDiscount1 != null && !appliedDiscounts.Any(x => x.Id == orderSubTotalAppliedDiscount1.Id))
                    {
                        appliedDiscounts.Add(orderSubTotalAppliedDiscount1);
                    }

                    // Sub total (excl tax).
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, false,
                        out var orderSubTotalDiscountAmount2, out var orderSubTotalAppliedDiscount2, out var subTotalWithoutDiscountBase2, out var subTotalWithDiscountBase2);

                    orderSubTotalExclTax = subTotalWithoutDiscountBase2;
                    orderSubTotalDiscountExclTax = orderSubTotalDiscountAmount2;
                }
                else
                {
                    orderSubTotalInclTax = initialOrder.OrderSubtotalInclTax;
                    orderSubTotalExclTax = initialOrder.OrderSubtotalExclTax;
                }


                // Shipping info.
                var shoppingCartRequiresShipping = false;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    shoppingCartRequiresShipping = cart.RequiresShipping();
                }
                else
                {
                    shoppingCartRequiresShipping = initialOrder.ShippingStatus != ShippingStatus.ShippingNotRequired;
                }

                string shippingMethodName = "", shippingRateComputationMethodSystemName = "";
                if (shoppingCartRequiresShipping)
                {
                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, processPaymentRequest.StoreId);
                        if (shippingOption != null)
                        {
                            shippingMethodName = shippingOption.Name;
                            shippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                        }
                    }
                    else
                    {
                        shippingMethodName = initialOrder.ShippingMethod;
                        shippingRateComputationMethodSystemName = initialOrder.ShippingRateComputationMethodSystemName;
                    }
                }

                // Shipping total.
                decimal? orderShippingTotalInclTax, orderShippingTotalExclTax = null;
                decimal orderShippingTaxRate = decimal.Zero;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, true, out orderShippingTaxRate, out var shippingTotalDiscount);
                    orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, false);

                    if (shippingTotalDiscount != null && !appliedDiscounts.Any(x => x.Id == shippingTotalDiscount.Id))
                    {
                        appliedDiscounts.Add(shippingTotalDiscount);
                    }
                }
                else
                {
                    orderShippingTotalInclTax = initialOrder.OrderShippingInclTax;
                    orderShippingTotalExclTax = initialOrder.OrderShippingExclTax;
                    orderShippingTaxRate = initialOrder.OrderShippingTaxRate;
                }

                // Payment total.
                decimal paymentAdditionalFeeInclTax, paymentAdditionalFeeExclTax, paymentAdditionalFeeTaxRate;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, processPaymentRequest.PaymentMethodSystemName);
                    paymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, customer, out paymentAdditionalFeeTaxRate);
                    paymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, customer);
                }
                else
                {
                    paymentAdditionalFeeInclTax = initialOrder.PaymentMethodAdditionalFeeInclTax;
                    paymentAdditionalFeeExclTax = initialOrder.PaymentMethodAdditionalFeeExclTax;
                    paymentAdditionalFeeTaxRate = initialOrder.PaymentMethodAdditionalFeeTaxRate;
                }

                // Tax total.
                var orderTaxTotal = decimal.Zero;
                string vatNumber = "", taxRates = "";
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    // Tax amount.
                    orderTaxTotal = _orderTotalCalculationService.GetTaxTotal(cart, out var taxRatesDictionary);

                    // VAT number.
                    var customerVatStatus = (VatNumberStatus)customer.VatNumberStatusId;
                    if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                    {
                        vatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
                    }

                    taxRates = FormatTaxRates(taxRatesDictionary);
                }
                else
                {
                    orderTaxTotal = initialOrder.OrderTax;
                    vatNumber = initialOrder.VatNumber;
                }

                processPaymentRequest.OrderTax = orderTaxTotal;

                // Order total (and applied discounts, gift cards, reward points).
                ShoppingCartTotal cartTotal = null;

                if (!processPaymentRequest.IsRecurringPayment)
                {
                    cartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);

                    // Discount history.
                    if (cartTotal.AppliedDiscount != null && !appliedDiscounts.Any(x => x.Id == cartTotal.AppliedDiscount.Id))
                    {
                        appliedDiscounts.Add(cartTotal.AppliedDiscount);
                    }
                }
                else
                {
                    cartTotal = new ShoppingCartTotal(initialOrder.OrderTotal);
                    cartTotal.DiscountAmount = initialOrder.OrderDiscount;
                }

                processPaymentRequest.OrderTotal = cartTotal.TotalAmount.Value;

                #endregion

                #region Addresses & pre-payment workflow

                // Give payment processor the opportunity to fullfill billing address.
                var preProcessPaymentResult = _paymentService.PreProcessPayment(processPaymentRequest);

                if (!preProcessPaymentResult.Success)
                {
                    result.Errors.AddRange(preProcessPaymentResult.Errors);
                    result.Errors.Add(T("Common.Error.PreProcessPayment"));
                    return result;
                }

                var billingAddress = !processPaymentRequest.IsRecurringPayment
                    ? (Address)customer.BillingAddress.Clone()
                    : (Address)initialOrder.BillingAddress.Clone();

                Address shippingAddress = null;
                if (shoppingCartRequiresShipping)
                {
                    shippingAddress = !processPaymentRequest.IsRecurringPayment
                        ? (Address)customer.ShippingAddress.Clone()
                        : (Address)initialOrder.ShippingAddress.Clone();
                }

                #endregion

                #region Payment workflow

                // Skip payment workflow if order total equals zero.
                var skipPaymentWorkflow = cartTotal.TotalAmount.Value == decimal.Zero;

                // Payment workflow.
                Provider<IPaymentMethod> paymentMethod = null;
                if (!skipPaymentWorkflow)
                {
                    paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                }
                else
                {
                    processPaymentRequest.PaymentMethodSystemName = "";
                }

                // Recurring or standard shopping cart?
                var isRecurringShoppingCart = false;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    isRecurringShoppingCart = cart.IsRecurring();
                    if (isRecurringShoppingCart)
                    {
                        var unused = cart.GetRecurringCycleInfo(_localizationService, out var recurringCycleLength, out var recurringCyclePeriod, out var recurringTotalCycles);

                        processPaymentRequest.RecurringCycleLength = recurringCycleLength;
                        processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
                        processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
                    }
                }
                else
                {
                    isRecurringShoppingCart = true;
                }

                // Process payment.
                ProcessPaymentResult processPaymentResult = null;
                if (!skipPaymentWorkflow && !processPaymentRequest.IsMultiOrder)
                {
                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        if (isRecurringShoppingCart)
                        {
                            // Recurring cart.
                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new SmartException(T("Payment.RecurringPaymentNotSupported"));
                                case RecurringPaymentType.Manual:
                                case RecurringPaymentType.Automatic:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                default:
                                    throw new SmartException(T("Payment.RecurringPaymentTypeUnknown"));
                            }
                        }
                        else
                        {
                            // Standard cart.
                            processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                        }
                    }
                    else
                    {
                        if (isRecurringShoppingCart)
                        {
                            // Old credit card info.
                            processPaymentRequest.CreditCardType = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardType) : "";
                            processPaymentRequest.CreditCardName = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardName) : "";
                            processPaymentRequest.CreditCardNumber = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardNumber) : "";
                            // MaskedCreditCardNumber.
                            processPaymentRequest.CreditCardCvv2 = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardCvv2) : "";

                            try
                            {
                                processPaymentRequest.CreditCardExpireMonth = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationMonth)) : 0;
                                processPaymentRequest.CreditCardExpireYear = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationYear)) : 0;
                            }
                            catch { }

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new SmartException(T("Payment.RecurringPaymentNotSupported"));
                                case RecurringPaymentType.Manual:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                case RecurringPaymentType.Automatic:
                                    // Payment is processed on payment gateway site.
                                    processPaymentResult = new ProcessPaymentResult();
                                    break;
                                default:
                                    throw new SmartException(T("Payment.RecurringPaymentTypeUnknown"));
                            }
                        }
                        else
                        {
                            throw new SmartException(T("Order.NoRecurringProducts"));
                        }
                    }
                }
                else
                {
                    // Payment is not required.
                    processPaymentResult = new ProcessPaymentResult
                    {
                        NewPaymentStatus = PaymentStatus.Paid
                    };
                }

                #endregion

                if (processPaymentResult.Success)
                {
                    // Save order.
                    // Uncomment this line to support transactions.
                    //using (var scope = new System.Transactions.TransactionScope())
                    {
                        #region Save order details

                        var shippingStatus = ShippingStatus.NotYetShipped;
                        if (!shoppingCartRequiresShipping)
                        {
                            shippingStatus = ShippingStatus.ShippingNotRequired;
                        }

                        var order = new Order
                        {
                            StoreId = processPaymentRequest.StoreId,
                            OrderGuid = processPaymentRequest.OrderGuid,
                            CustomerId = customer.Id,
                            CustomerLanguageId = customerLanguage.Id,
                            CustomerTaxDisplayType = customerTaxDisplayType,
                            CustomerIp = _webHelper.GetCurrentIpAddress(),
                            OrderSubtotalInclTax = orderSubTotalInclTax,
                            OrderSubtotalExclTax = orderSubTotalExclTax,
                            OrderSubTotalDiscountInclTax = orderSubTotalDiscountInclTax,
                            OrderSubTotalDiscountExclTax = orderSubTotalDiscountExclTax,
                            OrderShippingInclTax = orderShippingTotalInclTax.Value,
                            OrderShippingExclTax = orderShippingTotalExclTax.Value,
                            OrderShippingTaxRate = orderShippingTaxRate,
                            PaymentMethodAdditionalFeeInclTax = paymentAdditionalFeeInclTax,
                            PaymentMethodAdditionalFeeExclTax = paymentAdditionalFeeExclTax,
                            PaymentMethodAdditionalFeeTaxRate = paymentAdditionalFeeTaxRate,
                            TaxRates = taxRates,
                            OrderTax = orderTaxTotal,
                            OrderTotalRounding = cartTotal.RoundingAmount,
                            OrderTotal = cartTotal.TotalAmount.Value,
                            RefundedAmount = decimal.Zero,
                            OrderDiscount = cartTotal.DiscountAmount,
                            CreditBalance = cartTotal.CreditBalance,
                            CheckoutAttributeDescription = checkoutAttributeDescription,
                            CheckoutAttributesXml = checkoutAttributesXml,
                            CustomerCurrencyCode = customerCurrencyCode,
                            CurrencyRate = customerCurrencyRate,
                            AffiliateId = affiliateId,
                            OrderStatus = OrderStatus.Pending,
                            AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                            CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                            CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                            CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                            MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                            CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                            CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                            CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                            AllowStoringDirectDebit = processPaymentResult.AllowStoringDirectDebit,
                            DirectDebitAccountHolder = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitAccountHolder) : string.Empty,
                            DirectDebitAccountNumber = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitAccountNumber) : string.Empty,
                            DirectDebitBankCode = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitBankCode) : string.Empty,
                            DirectDebitBankName = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitBankName) : string.Empty,
                            DirectDebitBIC = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitBic) : string.Empty,
                            DirectDebitCountry = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitCountry) : string.Empty,
                            DirectDebitIban = processPaymentResult.AllowStoringDirectDebit ? _encryptionService.EncryptText(processPaymentRequest.DirectDebitIban) : string.Empty,
                            PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                            AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                            AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                            AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                            CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                            CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                            SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                            PurchaseOrderNumber = processPaymentRequest.PurchaseOrderNumber,
                            PaymentStatus = processPaymentResult.NewPaymentStatus,
                            PaidDateUtc = null,
                            BillingAddress = billingAddress,
                            ShippingAddress = shippingAddress,
                            ShippingStatus = shippingStatus,
                            ShippingMethod = shippingMethodName,
                            ShippingRateComputationMethodSystemName = shippingRateComputationMethodSystemName,
                            VatNumber = vatNumber,
                            CustomerOrderComment = extraData.ContainsKey("CustomerComment") ? extraData["CustomerComment"] : ""
                        };

                        if (extraData.ContainsKey("AcceptThirdPartyEmailHandOver") && _shoppingCartSettings.ThirdPartyEmailHandOver != CheckoutThirdPartyEmailHandOver.None)
                        {
                            order.AcceptThirdPartyEmailHandOver = extraData["AcceptThirdPartyEmailHandOver"].ToBool();
                        }

                        _orderService.InsertOrder(order);

                        result.PlacedOrder = order;

                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            // Move shopping cart items to order products.
                            foreach (var sc in cart)
                            {
                                sc.Item.Product.MergeWithCombination(sc.Item.AttributesXml);

                                // Prices.
                                decimal taxRate = decimal.Zero;
                                decimal unitPriceTaxRate = decimal.Zero;
                                decimal scUnitPrice = _priceCalculationService.GetUnitPrice(sc, true);
                                decimal scSubTotal = _priceCalculationService.GetSubTotal(sc, true);
                                decimal scUnitPriceInclTax = _taxService.GetProductPrice(sc.Item.Product, scUnitPrice, true, customer, out unitPriceTaxRate);
                                decimal scUnitPriceExclTax = _taxService.GetProductPrice(sc.Item.Product, scUnitPrice, false, customer, out taxRate);
                                decimal scSubTotalInclTax = _taxService.GetProductPrice(sc.Item.Product, scSubTotal, true, customer, out taxRate);
                                decimal scSubTotalExclTax = _taxService.GetProductPrice(sc.Item.Product, scSubTotal, false, customer, out taxRate);

                                // Discounts.
                                Discount scDiscount = null;
                                decimal discountAmount = _priceCalculationService.GetDiscountAmount(sc, out scDiscount);
                                decimal discountAmountInclTax = _taxService.GetProductPrice(sc.Item.Product, discountAmount, true, customer, out taxRate);
                                decimal discountAmountExclTax = _taxService.GetProductPrice(sc.Item.Product, discountAmount, false, customer, out taxRate);

                                if (scDiscount != null && !appliedDiscounts.Any(x => x.Id == scDiscount.Id))
                                {
                                    appliedDiscounts.Add(scDiscount);
                                }

                                var attributeDescription = _productAttributeFormatter.FormatAttributes(sc.Item.Product, sc.Item.AttributesXml, customer);
                                var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);
                                var displayDeliveryTime =
                                    _shoppingCartSettings.DeliveryTimesInShoppingCart != DeliveryTimesPresentation.None &&
                                    sc.Item.Product.DeliveryTimeId.HasValue &&
                                    sc.Item.Product.IsShipEnabled &&
                                    sc.Item.Product.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

                                // Save order item.
                                var orderItem = new OrderItem
                                {
                                    OrderItemGuid = Guid.NewGuid(),
                                    Order = order,
                                    ProductId = sc.Item.ProductId,
                                    UnitPriceInclTax = scUnitPriceInclTax,
                                    UnitPriceExclTax = scUnitPriceExclTax,
                                    PriceInclTax = scSubTotalInclTax,
                                    PriceExclTax = scSubTotalExclTax,
                                    TaxRate = unitPriceTaxRate,
                                    AttributeDescription = attributeDescription,
                                    AttributesXml = sc.Item.AttributesXml,
                                    Quantity = sc.Item.Quantity,
                                    DiscountAmountInclTax = discountAmountInclTax,
                                    DiscountAmountExclTax = discountAmountExclTax,
                                    DownloadCount = 0,
                                    IsDownloadActivated = false,
                                    LicenseDownloadId = 0,
                                    ItemWeight = itemWeight,
                                    ProductCost = _priceCalculationService.GetProductCost(sc.Item.Product, sc.Item.AttributesXml),
                                    DeliveryTimeId = sc.Item.Product.GetDeliveryTimeIdAccordingToStock(_catalogSettings),
                                    DisplayDeliveryTime = displayDeliveryTime
                                };

                                if (sc.Item.Product.ProductType == ProductType.BundledProduct && sc.ChildItems != null)
                                {
                                    var listBundleData = new List<ProductBundleItemOrderData>();

                                    foreach (var childItem in sc.ChildItems)
                                    {
                                        var bundleItemSubTotal = _taxService.GetProductPrice(childItem.Item.Product, _priceCalculationService.GetSubTotal(childItem, true), out taxRate);

                                        var attributesInfo = _productAttributeFormatter.FormatAttributes(childItem.Item.Product, childItem.Item.AttributesXml, order.Customer,
                                            renderPrices: false, allowHyperlinks: true);

                                        childItem.BundleItemData.ToOrderData(listBundleData, bundleItemSubTotal, childItem.Item.AttributesXml, attributesInfo);
                                    }

                                    orderItem.SetBundleData(listBundleData);
                                }

                                order.OrderItems.Add(orderItem);
                                _orderService.UpdateOrder(order);

                                // Gift cards.
                                if (sc.Item.Product.IsGiftCard)
                                {
                                    _productAttributeParser.GetGiftCardAttribute(
                                        sc.Item.AttributesXml,
                                        out var giftCardRecipientName,
                                        out var giftCardRecipientEmail,
                                        out var giftCardSenderName,
                                        out var giftCardSenderEmail,
                                        out var giftCardMessage);

                                    for (int i = 0; i < sc.Item.Quantity; i++)
                                    {
                                        var gc = new GiftCard
                                        {
                                            GiftCardType = sc.Item.Product.GiftCardType,
                                            PurchasedWithOrderItem = orderItem,
                                            Amount = scUnitPriceExclTax,
                                            IsGiftCardActivated = false,
                                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                            RecipientName = giftCardRecipientName,
                                            RecipientEmail = giftCardRecipientEmail,
                                            SenderName = giftCardSenderName,
                                            SenderEmail = giftCardSenderEmail,
                                            Message = giftCardMessage,
                                            IsRecipientNotified = false,
                                            CreatedOnUtc = utcNow
                                        };
                                        _giftCardService.InsertGiftCard(gc);
                                    }
                                }

                                _productService.AdjustInventory(sc, true);
                            }

                            // Clear shopping cart.
                            if (!processPaymentRequest.IsMultiOrder)
                            {
                                cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci.Item, false));
                            }
                        }
                        else
                        {
                            // Recurring payment.
                            var initialOrderItems = initialOrder.OrderItems;
                            foreach (var orderItem in initialOrderItems)
                            {
                                // Save item.
                                var newOrderItem = new OrderItem
                                {
                                    OrderItemGuid = Guid.NewGuid(),
                                    Order = order,
                                    ProductId = orderItem.ProductId,
                                    UnitPriceInclTax = orderItem.UnitPriceInclTax,
                                    UnitPriceExclTax = orderItem.UnitPriceExclTax,
                                    PriceInclTax = orderItem.PriceInclTax,
                                    PriceExclTax = orderItem.PriceExclTax,
                                    TaxRate = orderItem.TaxRate,
                                    AttributeDescription = orderItem.AttributeDescription,
                                    AttributesXml = orderItem.AttributesXml,
                                    Quantity = orderItem.Quantity,
                                    DiscountAmountInclTax = orderItem.DiscountAmountInclTax,
                                    DiscountAmountExclTax = orderItem.DiscountAmountExclTax,
                                    DownloadCount = 0,
                                    IsDownloadActivated = false,
                                    LicenseDownloadId = 0,
                                    ItemWeight = orderItem.ItemWeight,
                                    BundleData = orderItem.BundleData,
                                    ProductCost = orderItem.ProductCost,
                                    DeliveryTimeId = orderItem.DeliveryTimeId,
                                    DisplayDeliveryTime = orderItem.DisplayDeliveryTime
                                };
                                order.OrderItems.Add(newOrderItem);
                                _orderService.UpdateOrder(order);

                                // Gift cards.
                                if (orderItem.Product.IsGiftCard)
                                {
                                    _productAttributeParser.GetGiftCardAttribute(
                                        orderItem.AttributesXml,
                                        out var giftCardRecipientName,
                                        out var giftCardRecipientEmail,
                                        out var giftCardSenderName,
                                        out var giftCardSenderEmail,
                                        out var giftCardMessage);

                                    for (int i = 0; i < orderItem.Quantity; i++)
                                    {
                                        var gc = new GiftCard
                                        {
                                            GiftCardType = orderItem.Product.GiftCardType,
                                            PurchasedWithOrderItem = newOrderItem,
                                            Amount = orderItem.UnitPriceExclTax,
                                            IsGiftCardActivated = false,
                                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                            RecipientName = giftCardRecipientName,
                                            RecipientEmail = giftCardRecipientEmail,
                                            SenderName = giftCardSenderName,
                                            SenderEmail = giftCardSenderEmail,
                                            Message = giftCardMessage,
                                            IsRecipientNotified = false,
                                            CreatedOnUtc = utcNow
                                        };
                                        _giftCardService.InsertGiftCard(gc);
                                    }
                                }

                                _productService.AdjustInventory(orderItem, true, orderItem.Quantity);
                            }
                        }

                        // Discount usage history.
                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            foreach (var discount in appliedDiscounts)
                            {
                                var duh = new DiscountUsageHistory
                                {
                                    Discount = discount,
                                    Order = order,
                                    CreatedOnUtc = utcNow
                                };
                                _discountService.InsertDiscountUsageHistory(duh);
                            }
                        }

                        // Gift card usage history.
                        if (!processPaymentRequest.IsRecurringPayment && cartTotal.AppliedGiftCards != null)
                        {
                            foreach (var agc in cartTotal.AppliedGiftCards)
                            {
                                var amountUsed = agc.AmountCanBeUsed;
                                var gcuh = new GiftCardUsageHistory
                                {
                                    GiftCard = agc.GiftCard,
                                    UsedWithOrder = order,
                                    UsedValue = amountUsed,
                                    CreatedOnUtc = utcNow
                                };
                                agc.GiftCard.GiftCardUsageHistory.Add(gcuh);
                                _giftCardService.UpdateGiftCard(agc.GiftCard);
                            }
                        }

                        // Reward points history.
                        if (cartTotal.RedeemedRewardPointsAmount > decimal.Zero)
                        {
                            customer.AddRewardPointsHistoryEntry(-cartTotal.RedeemedRewardPoints,
                                _localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId).FormatInvariant(order.GetOrderNumber()),
                                order,
                                cartTotal.RedeemedRewardPointsAmount);

                            _customerService.UpdateCustomer(customer);
                        }

                        // Recurring orders.
                        if (!processPaymentRequest.IsRecurringPayment && isRecurringShoppingCart)
                        {
                            // Create recurring payment (the first payment).
                            var rp = new RecurringPayment
                            {
                                CycleLength = processPaymentRequest.RecurringCycleLength,
                                CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                                TotalCycles = processPaymentRequest.RecurringTotalCycles,
                                StartDateUtc = utcNow,
                                IsActive = true,
                                CreatedOnUtc = utcNow,
                                InitialOrder = order,
                            };
                            _orderService.InsertRecurringPayment(rp);

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    break;
                                case RecurringPaymentType.Manual:
                                    {
                                        // First payment.
                                        rp.RecurringPaymentHistory.Add(new RecurringPaymentHistory
                                        {
                                            RecurringPayment = rp,
                                            CreatedOnUtc = utcNow,
                                            OrderId = order.Id
                                        });
                                        _orderService.UpdateRecurringPayment(rp);
                                    }
                                    break;
                                case RecurringPaymentType.Automatic:
                                    {
                                        // Will be created later (process is automated).
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        #endregion

                        #region Notifications, notes and attributes

                        // Notes, messages.
                        _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderPlaced"));

                        // Send email notifications.
                        var msg = _messageFactory.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                        if (msg?.Email?.Id != null)
                        {
                            _orderService.AddOrderNote(order, T("Admin.OrderNotice.MerchantEmailQueued", msg.Email.Id));
                        }

                        msg = _messageFactory.SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId);
                        if (msg?.Email?.Id != null)
                        {
                            _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerEmailQueued", msg.Email.Id));
                        }

                        // Check order status.
                        CheckOrderStatus(order);

                        // Reset checkout data.
                        if (!processPaymentRequest.IsRecurringPayment && !processPaymentRequest.IsMultiOrder)
                        {
                            _customerService.ResetCheckoutData(customer, processPaymentRequest.StoreId, true, true, true, clearCreditBalance: true);
                        }

                        // Check for generic attributes to be inserted automatically.
                        foreach (var customProperty in processPaymentRequest.CustomProperties.Where(x => x.Key.HasValue() && x.Value.AutoCreateGenericAttribute))
                        {
                            _genericAttributeService.SaveAttribute<object>(order, customProperty.Key, customProperty.Value.Value, order.StoreId);
                        }

                        // Handle transiancy of uploaded files for checkout attributes
                        if (order.CheckoutAttributesXml.HasValue())
                        {
                            var checkoutAttrs = _checkoutAttributeParser.ParseCheckoutAttributes(order.CheckoutAttributesXml);

                            foreach (var attr in checkoutAttrs)
                            {
                                if (attr.AttributeControlType == AttributeControlType.FileUpload)
                                {
                                    var value = _checkoutAttributeParser.ParseValues(order.CheckoutAttributesXml, attr.Id).FirstOrDefault();
                                    Guid.TryParse(value, out var downloadGuid);
                                    if (downloadGuid != null)
                                    {
                                        var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                        if (download != null)
                                        {
                                            download.IsTransient = false;
                                            _downloadService.UpdateDownload(download);
                                        }
                                    }
                                }
                            }
                        }

                        // Uncomment this line to support transactions.
                        //scope.Complete();

                        // Publish events.
                        _eventPublisher.PublishOrderPlaced(order);

                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            _customerActivityService.InsertActivity("PublicStore.PlaceOrder", T("ActivityLog.PublicStore.PlaceOrder", order.GetOrderNumber()));
                        }

                        if (order.PaymentStatus == PaymentStatus.Paid)
                        {
                            _eventPublisher.PublishOrderPaid(order);
                        }

                        #endregion

                        #region Newsletter subscription

                        if (extraData.ContainsKey("SubscribeToNewsLetter") && _shoppingCartSettings.NewsLetterSubscription != CheckoutNewsLetterSubscription.None)
                        {
                            var addSubscription = extraData["SubscribeToNewsLetter"].ToBool();

                            bool? nsResult = _newsLetterSubscriptionService.AddNewsLetterSubscriptionFor(addSubscription, customer.Email, order.StoreId);

                            if (nsResult.HasValue)
                            {
                                _orderService.AddOrderNote(order, T(nsResult.Value ? "Admin.OrderNotice.NewsLetterSubscriptionAdded" : "Admin.OrderNotice.NewsLetterSubscriptionRemoved"));
                            }
                        }

                        #endregion
                    }
                }
                else
                {
                    result.AddError(T("Payment.PayingFailed"));

                    foreach (var paymentError in processPaymentResult.Errors)
                    {
                        result.AddError(paymentError);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.AddError(ex.Message);
            }

            if (result.Errors.Any())
            {
                Logger.Error(string.Join(" ", result.Errors));
            }

            return result;
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            Guard.NotNull(order, nameof(order));

            if (order.OrderStatus != OrderStatus.Cancelled)
            {
                ReduceRewardPoints(order);

                // Cancel recurring payments.
                var recurringPayments = _orderService.SearchRecurringPayments(0, 0, order.Id, null);
                foreach (var rp in recurringPayments)
                {
                    CancelRecurringPayment(rp);
                }

                // Adjust inventory.
                foreach (var orderItem in order.OrderItems)
                {
                    _productService.AdjustInventory(orderItem, false, orderItem.Quantity);
                }
            }

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderDeleted"));

            _orderService.DeleteOrder(order);
        }


        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void ProcessNextRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            try
            {
                if (!recurringPayment.IsActive)
                    throw new SmartException(T("Payment.RecurringPaymentNotActive"));

                var initialOrder = recurringPayment.InitialOrder;
                if (initialOrder == null)
                    throw new SmartException(T("Order.InitialOrderDoesNotExistForRecurringPayment"));

                var customer = initialOrder.Customer;
                if (customer == null)
                    throw new SmartException(T("Customer.DoesNotExist"));

                var nextPaymentDate = recurringPayment.NextPaymentDate;
                if (!nextPaymentDate.HasValue)
                    throw new SmartException(T("Payment.CannotCalculateNextPaymentDate"));

                var paymentInfo = new ProcessPaymentRequest
                {
                    StoreId = initialOrder.StoreId,
                    CustomerId = customer.Id,
                    OrderGuid = Guid.NewGuid(),
                    IsRecurringPayment = true,
                    InitialOrderId = initialOrder.Id,
                    RecurringCycleLength = recurringPayment.CycleLength,
                    RecurringCyclePeriod = recurringPayment.CyclePeriod,
                    RecurringTotalCycles = recurringPayment.TotalCycles,
                };

                //place a new order
                var result = this.PlaceOrder(paymentInfo, new Dictionary<string, string>());

                if (result.Success)
                {
                    if (result.PlacedOrder == null)
                        throw new SmartException(T("Order.NotFound", "".NaIfEmpty()));

                    var rph = new RecurringPaymentHistory
                    {
                        RecurringPayment = recurringPayment,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = result.PlacedOrder.Id
                    };

                    recurringPayment.RecurringPaymentHistory.Add(rph);
                    _orderService.UpdateRecurringPayment(recurringPayment);
                }
                else if (result.Errors.Count > 0)
                {
                    throw new SmartException(string.Join(" ", result.Errors));
                }
            }
            catch (Exception exception)
            {
                Logger.ErrorsAll(exception);
                throw;
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual IList<string> CancelRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
                return new List<string> { T("Order.InitialOrderDoesNotExistForRecurringPayment") };

            var request = new CancelRecurringPaymentRequest();
            CancelRecurringPaymentResult result = null;

            try
            {
                request.Order = initialOrder;

                result = _paymentService.CancelRecurringPayment(request);

                if (result.Success)
                {
                    //update recurring payment
                    recurringPayment.IsActive = false;
                    _orderService.UpdateRecurringPayment(recurringPayment);

                    _orderService.AddOrderNote(initialOrder, T("Admin.OrderNotice.RecurringPaymentCancelled"));

                    //notify a store owner
                    _messageFactory.SendRecurringPaymentCancelledStoreOwnerNotification(recurringPayment, _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exception)
            {
                if (result == null)
                {
                    result = new CancelRecurringPaymentResult();
                }

                result.AddError(exception.ToAllMessages());
            }

            ProcessErrors(initialOrder, result.Errors, "Admin.OrderNotice.RecurringPaymentCancellationError");

            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        public virtual bool CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                return false;

            if (customerToValidate == null)
                return false;

            var initialOrder = recurringPayment.InitialOrder;
            if (initialOrder == null)
                return false;

            var customer = recurringPayment.InitialOrder.Customer;
            if (customer == null)
                return false;

            if (initialOrder.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (!customerToValidate.IsAdmin())
            {
                if (customer.Id != customerToValidate.Id)
                    return false;
            }

            if (!recurringPayment.NextPaymentDate.HasValue)
                return false;

            return true;
        }



        /// <summary>
        /// Send a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Ship(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new SmartException(T("Order.NotFound", shipment.OrderId));

            if (shipment.ShippedDateUtc.HasValue)
                throw new SmartException(T("Shipment.AlreadyShipped"));

            shipment.ShippedDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            //check whether we have more items to ship
            if (order.CanAddItemsToShipment() || order.HasItemsToDispatch())
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            else
                order.ShippingStatusId = (int)ShippingStatus.Shipped;

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.ShipmentSent", shipment.Id));

            if (notifyCustomer)
            {
                //notify customer
                var msg = _messageFactory.SendShipmentSentCustomerNotification(shipment, order.CustomerLanguageId);
                if (msg?.Email?.Id != null)
                {
                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerShippedEmailQueued", msg.Email.Id));
                }
            }

            //check order status
            CheckOrderStatus(order);
        }

        /// <summary>
        /// Marks a shipment as delivered
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void Deliver(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new SmartException(T("Order.NotFound", shipment.OrderId));

            if (shipment.DeliveryDateUtc.HasValue)
                throw new SmartException(T("Shipment.AlreadyDelivered"));

            shipment.DeliveryDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            if (!order.CanAddItemsToShipment() && !order.HasItemsToDispatch() && !order.HasItemsToDeliver())
            {
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            }

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.ShipmentDelivered", shipment.Id));

            if (notifyCustomer)
            {
                //send email notification
                var msg = _messageFactory.SendShipmentDeliveredCustomerNotification(shipment, order.CustomerLanguageId);
                if (msg?.Email?.Id != null)
                {
                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.CustomerDeliveredEmailQueued", msg.Email.Id));
                }
            }

            //check order status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        public virtual bool CanCancelOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            return true;
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        public virtual void CancelOrder(Order order, bool notifyCustomer)
        {
            Guard.NotNull(order, nameof(order));

            if (!CanCancelOrder(order))
            {
                throw new SmartException(T("Order.CannotCancel"));
            }

            // Cancel order.
            SetOrderStatus(order, OrderStatus.Cancelled, notifyCustomer);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderCancelled"));

            // Cancel recurring payments.
            var recurringPayments = _orderService.SearchRecurringPayments(0, 0, order.Id, null);
            foreach (var rp in recurringPayments)
            {
                CancelRecurringPayment(rp);
            }

            // Adjust inventory.
            foreach (var orderItem in order.OrderItems)
            {
                _productService.AdjustInventory(orderItem, false, orderItem.Quantity);
            }
        }

        /// <summary>
        /// Auto update order details
        /// </summary>
        /// <param name="context">Context parameters</param>
        public virtual void AutoUpdateOrderDetails(AutoUpdateOrderItemContext context)
        {
            var oi = context.OrderItem;

            context.RewardPointsOld = context.RewardPointsNew = oi.Order.Customer.GetRewardPointsBalance();

            if (context.UpdateTotals && oi.Order.OrderStatusId <= (int)OrderStatus.Pending)
            {
                var currency = _currencyService.GetCurrencyByCode(oi.Order.CustomerCurrencyCode);

                decimal priceInclTax = (context.QuantityNew * oi.UnitPriceInclTax).RoundIfEnabledFor(currency);
                decimal priceExclTax = (context.QuantityNew * oi.UnitPriceExclTax).RoundIfEnabledFor(currency);

                decimal deltaPriceInclTax = context.IsNewOrderItem
                    ? priceInclTax
                    : priceInclTax - (context.PriceInclTaxOld ?? oi.PriceInclTax);

                decimal deltaPriceExclTax = context.IsNewOrderItem
                    ? priceExclTax
                    : priceExclTax - (context.PriceExclTaxOld ?? oi.PriceExclTax);

                oi.Quantity = context.QuantityNew;
                oi.PriceInclTax = priceInclTax.RoundIfEnabledFor(currency);
                oi.PriceExclTax = priceExclTax.RoundIfEnabledFor(currency);

                decimal subtotalInclTax = oi.Order.OrderSubtotalInclTax + deltaPriceInclTax;
                decimal subtotalExclTax = oi.Order.OrderSubtotalExclTax + deltaPriceExclTax;

                oi.Order.OrderSubtotalInclTax = subtotalInclTax.RoundIfEnabledFor(currency);
                oi.Order.OrderSubtotalExclTax = subtotalExclTax.RoundIfEnabledFor(currency);

                decimal discountInclTax = oi.DiscountAmountInclTax * context.QuantityChangeFactor;
                decimal discountExclTax = oi.DiscountAmountExclTax * context.QuantityChangeFactor;

                //decimal deltaDiscountInclTax = discountInclTax - oi.DiscountAmountInclTax;
                //decimal deltaDiscountExclTax = discountExclTax - oi.DiscountAmountExclTax;

                oi.DiscountAmountInclTax = discountInclTax.RoundIfEnabledFor(currency);
                oi.DiscountAmountExclTax = discountExclTax.RoundIfEnabledFor(currency);

                decimal total = Math.Max(oi.Order.OrderTotal + deltaPriceInclTax, 0);
                decimal tax = Math.Max(oi.Order.OrderTax + (deltaPriceInclTax - deltaPriceExclTax), 0);

                oi.Order.OrderTotal = total.RoundIfEnabledFor(currency);
                oi.Order.OrderTax = tax.RoundIfEnabledFor(currency);

                // Update tax rate value.
                var deltaTax = deltaPriceInclTax - deltaPriceExclTax;
                if (deltaTax != decimal.Zero)
                {
                    var taxRates = oi.Order.TaxRatesDictionary;

                    taxRates[oi.TaxRate] = taxRates.ContainsKey(oi.TaxRate)
                        ? Math.Max(taxRates[oi.TaxRate] + deltaTax, 0)
                        : Math.Max(deltaTax, 0);

                    oi.Order.TaxRates = FormatTaxRates(taxRates);
                }

                _orderService.UpdateOrder(oi.Order);
            }

            if (context.AdjustInventory && context.QuantityDelta != 0)
            {
                context.Inventory = _productService.AdjustInventory(oi, context.QuantityDelta > 0, Math.Abs(context.QuantityDelta));
            }

            if (context.UpdateRewardPoints && context.QuantityDelta < 0)
            {
                // We reduce but we do not award points subsequently. They can be awarded once per order anyway (see Order.RewardPointsWereAdded).
                // UpdateRewardPoints only visible for unpending orders (see RewardPointsSettingsValidator).
                // Note: reducing can of cource only work if oi.UnitPriceExclTax has not been changed!
                decimal reduceAmount = Math.Abs(context.QuantityDelta) * oi.UnitPriceInclTax;
                ReduceRewardPoints(oi.Order, reduceAmount);
                context.RewardPointsNew = oi.Order.Customer.GetRewardPointsBalance();
            }
        }


        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as authorized</returns>
        public virtual bool CanMarkOrderAsAuthorized(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Pending)
                return true;

            return false;
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkAsAuthorized(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.PaymentStatusId = (int)PaymentStatus.Authorized;
            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderMarkedAsAuthorized"));

            //check order status
            CheckOrderStatus(order);
        }


        /// <summary>
        /// Gets a value indicating whether the order can be marked as completed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether the order can be marked as completed</returns>
        public virtual bool CanCompleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            return (order.OrderStatus != OrderStatus.Complete && order.OrderStatus != OrderStatus.Cancelled);
        }

        /// <summary>
        /// Marks the order as completed
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void CompleteOrder(Order order)
        {
            if (!CanCompleteOrder(order))
                throw new SmartException(T("Order.CannotMarkCompleted"));

            if (CanMarkOrderAsPaid(order))
            {
                MarkOrderAsPaid(order);
            }

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            }

            _orderService.UpdateOrder(order);

            CheckOrderStatus(order);
        }


        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        public virtual bool CanCapture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled || order.OrderStatus == OrderStatus.Pending)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized && _paymentService.SupportCapture(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Capture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanCapture(order))
                throw new SmartException(T("Order.CannotCapture"));

            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            try
            {
                //old info from placing order
                request.Order = order;
                result = _paymentService.Capture(request);

                if (result.Success)
                {
                    var paidDate = order.PaidDateUtc;
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                    {
                        paidDate = DateTime.UtcNow;
                    }

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatus = result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;

                    _orderService.UpdateOrder(order);

                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderCaptured"));

                    CheckOrderStatus(order);

                    //raise event         
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        _eventPublisher.PublishOrderPaid(order);
                    }
                }
            }
            catch (Exception exception)
            {
                if (result == null)
                {
                    result = new CapturePaymentResult();
                }

                result.AddError(exception.ToAllMessages());
            }

            ProcessErrors(order, result.Errors, "Admin.OrderNotice.OrderCaptureError");

            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as paid</returns>
        public virtual bool CanMarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.Refunded ||
                order.PaymentStatus == PaymentStatus.Voided)
                return false;

            return true;
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanMarkOrderAsPaid(order))
                throw new SmartException(T("Order.CannotMarkPaid"));

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderMarkedAsPaid"));

            CheckOrderStatus(order);

            // raise event         
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                _eventPublisher.PublishOrderPaid(order);
            }
        }



        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanRefund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            // Only partial refunds allowed if already refunded.
            if (order.RefundedAmount > decimal.Zero)
                return false;

            // Uncomment the lines below in order to allow this operation for cancelled orders.
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Paid && _paymentService.SupportRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> Refund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanRefund(order))
                throw new SmartException(T("Order.CannotRefund"));

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Order = order;
                request.AmountToRefund = order.OrderTotal;
                request.IsPartialRefund = false;

                result = _paymentService.Refund(request);

                if (result.Success)
                {
                    // Total amount refunded.
                    decimal totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                    // Update order info.
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;

                    _orderService.UpdateOrder(order);

                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderRefunded", _priceFormatter.FormatPrice(request.AmountToRefund, true, false)));

                    CheckOrderStatus(order);
                }

            }
            catch (Exception exception)
            {
                if (result == null)
                {
                    result = new RefundPaymentResult();
                }

                result.AddError(exception.ToAllMessages());
            }

            ProcessErrors(order, result.Errors, "Admin.OrderNotice.OrderRefundError");

            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as refunded</returns>
        public virtual bool CanRefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            // Only partial refunds allowed if already refunded.
            if (order.RefundedAmount > decimal.Zero)
                return false;

            // Uncomment the lines below in order to allow this operation for cancelled orders.
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //     return false;

            if (order.PaymentStatus == PaymentStatus.Paid)
                return true;

            return false;
        }

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void RefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanRefundOffline(order))
                throw new SmartException(T("Order.CannotRefund"));

            // Amout to refund.
            decimal amountToRefund = order.OrderTotal;

            // Total amount refunded.
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            // Update order info.
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = PaymentStatus.Refunded;

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderMarkedAsRefunded", _priceFormatter.FormatPrice(amountToRefund, true, false)));

            CheckOrderStatus(order);
        }

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        public virtual bool CanPartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if ((order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded) &&
                _paymentService.SupportPartiallyRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        public virtual IList<string> PartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanPartiallyRefund(order, amountToRefund))
                throw new SmartException(T("Order.CannotPartialRefund"));

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;

            try
            {
                request.Order = order;
                request.AmountToRefund = amountToRefund;
                request.IsPartialRefund = true;

                result = _paymentService.Refund(request);

                if (result.Success)
                {
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;

                    _orderService.UpdateOrder(order);

                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderPartiallyRefunded", _priceFormatter.FormatPrice(amountToRefund, true, false)));

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exception)
            {
                if (result == null)
                {
                    result = new RefundPaymentResult();
                }

                result.AddError(exception.ToAllMessages());
            }

            ProcessErrors(order, result.Errors, "Admin.OrderNotice.OrderPartiallyRefundError");

            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether order can be marked as partially refunded</returns>
        public virtual bool CanPartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            decimal canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if (order.PaymentStatus == PaymentStatus.Paid ||
                order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                return true;

            return false;
        }

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        public virtual void PartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanPartiallyRefundOffline(order, amountToRefund))
                throw new SmartException(T("Order.CannotPartialRefund"));

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            //if (order.OrderTotal == totalAmountRefunded), then set order.PaymentStatus = PaymentStatus.Refunded;
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderMarkedAsPartiallyRefunded", _priceFormatter.FormatPrice(amountToRefund, true, false)));

            //check order status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        public virtual bool CanVoid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportVoid(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Voided order</returns>
        public virtual IList<string> Void(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanVoid(order))
                throw new SmartException(T("Order.CannotVoid"));

            var request = new VoidPaymentRequest();
            VoidPaymentResult result = null;

            try
            {
                request.Order = order;
                result = _paymentService.Void(request);

                if (result.Success)
                {
                    //update order info
                    order.PaymentStatus = result.NewPaymentStatus;

                    _orderService.UpdateOrder(order);

                    _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderVoided"));

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exception)
            {
                if (result == null)
                {
                    result = new VoidPaymentResult();
                }

                result.AddError(exception.ToAllMessages());
            }

            ProcessErrors(order, result.Errors, "Admin.OrderNotice.OrderVoidError");

            return result.Errors;
        }

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as voided</returns>
        public virtual bool CanVoidOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Authorized)
                return true;

            return false;
        }

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void VoidOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanVoidOffline(order))
                throw new SmartException(T("Order.CannotVoid"));

            order.PaymentStatusId = (int)PaymentStatus.Voided;

            _orderService.UpdateOrder(order);

            _orderService.AddOrderNote(order, T("Admin.OrderNotice.OrderMarkedAsVoided"));

            //check orer status
            CheckOrderStatus(order);
        }



        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void ReOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            foreach (var orderItem in order.OrderItems)
            {
                var isBundle = (orderItem.Product.ProductType == ProductType.BundledProduct);

                var addToCartContext = new AddToCartContext();

                addToCartContext.Warnings = _shoppingCartService.AddToCart(order.Customer, orderItem.Product, ShoppingCartType.ShoppingCart, order.StoreId,
                    orderItem.AttributesXml, isBundle ? decimal.Zero : orderItem.UnitPriceExclTax, orderItem.Quantity, false, addToCartContext);

                if (isBundle && orderItem.BundleData.HasValue() && addToCartContext.Warnings.Count == 0)
                {
                    foreach (var bundleData in orderItem.GetBundleData())
                    {
                        var bundleItem = _productService.GetBundleItemById(bundleData.BundleItemId);
                        addToCartContext.BundleItem = bundleItem;

                        addToCartContext.Warnings = _shoppingCartService.AddToCart(order.Customer, bundleItem.Product, ShoppingCartType.ShoppingCart, order.StoreId,
                            bundleData.AttributesXml, decimal.Zero, bundleData.Quantity, false, addToCartContext);
                    }
                }

                _shoppingCartService.AddToCartStoring(addToCartContext);
            }
        }

        public virtual bool IsReturnRequestAllowed(Order order)
        {
            if (!_orderSettings.ReturnRequestsEnabled)
                return false;

            if (order == null || order.Deleted)
                return false;

            if (order.OrderStatus != OrderStatus.Complete)
                return false;

            var numberOfDaysReturnRequestAvailableValid = false;

            if (_orderSettings.NumberOfDaysReturnRequestAvailable == 0)
            {
                numberOfDaysReturnRequestAvailableValid = true;
            }
            else
            {
                var daysPassed = (DateTime.UtcNow - order.CreatedOnUtc).TotalDays;
                numberOfDaysReturnRequestAvailableValid = (daysPassed - _orderSettings.NumberOfDaysReturnRequestAvailable) < 0;
            }

            return numberOfDaysReturnRequestAvailableValid;
        }

        public virtual (bool valid, decimal orderTotalMinimum) IsAboveOrderTotalMinimum(IList<OrganizedShoppingCartItem> cart, int[] customerRoleIds)
        {
            var query = _customerService.GetAllCustomerRoles().SourceQuery
                .Where(x => x.OrderTotalMinimum > decimal.Zero && customerRoleIds.Contains(x.Id));

            var customerRole = _orderSettings.MultipleOrderTotalRestrictionsExpandRange
                ? query.OrderBy(x => x.OrderTotalMinimum).FirstOrDefault()
                : query.OrderByDescending(x => x.OrderTotalMinimum).FirstOrDefault();

            var minimumOrderTotal = customerRole == null
                ? _orderSettings.OrderTotalMinimum : customerRole.OrderTotalMinimum;

            return IsAboveOrderTotalMinimum(cart, minimumOrderTotal ?? decimal.Zero);
        }

        /// <summary>
        /// Validate order total minimum.
        /// </summary>
        /// <param name="cart">Shopping cart, minimum order total</param>
        /// <returns>true - OK; false - minimum order total is not reached</returns>
        private (bool valid, decimal orderTotalMinimum) IsAboveOrderTotalMinimum(IList<OrganizedShoppingCartItem> cart, decimal minimumOrderTotal)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (cart.Count > 0 && minimumOrderTotal > decimal.Zero)
            {
                _orderTotalCalculationService.GetShoppingCartSubTotal(
                    cart, out _, out _, out var cartSubtotal, out _);

                if (cartSubtotal < minimumOrderTotal)
                    return (false, minimumOrderTotal);
            }

            return (true, minimumOrderTotal);
        }

        public virtual (bool valid, decimal orderTotalMaximum) IsBelowOrderTotalMaximum(IList<OrganizedShoppingCartItem> cart, int[] customerRoleIds)
        {
            var query = _customerService.GetAllCustomerRoles().SourceQuery
                .Where(x => x.OrderTotalMaximum > decimal.Zero && customerRoleIds.Contains(x.Id));

            var customerRole = _orderSettings.MultipleOrderTotalRestrictionsExpandRange
                ? query.OrderByDescending(x => x.OrderTotalMaximum).FirstOrDefault()
                : query.OrderBy(x => x.OrderTotalMaximum).FirstOrDefault();

            var maximumOrderTotal = customerRole == null
                ? _orderSettings.OrderTotalMaximum : customerRole.OrderTotalMaximum;

            return IsBelowOrderTotalMaximum(cart, maximumOrderTotal ?? decimal.Zero);
        }

        /// <summary>
        /// Validate order total maximum.
        /// </summary>
        /// <param name="cart">Shopping cart, maximum order total</param>
        /// <returns>true - OK; false - maximum order total is exceeded</returns>
        private (bool valid, decimal orderTotalMaximum) IsBelowOrderTotalMaximum(IList<OrganizedShoppingCartItem> cart, decimal maximumOrderTotal)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (cart.Count > 0 && maximumOrderTotal > decimal.Zero)
            {
                _orderTotalCalculationService.GetShoppingCartSubTotal(
                    cart, out _, out _, out var cartSubtotal, out _);

                if (cartSubtotal > maximumOrderTotal)
                    return (false, maximumOrderTotal);
            }

            return (true, maximumOrderTotal);
        }

        public virtual Shipment AddShipment(
            Order order,
            string trackingNumber,
            string trackingUrl,
            Dictionary<int, int> quantities)
        {
            Guard.NotNull(order, nameof(order));

            Shipment shipment = null;
            decimal? totalWeight = null;

            foreach (var orderItem in order.OrderItems)
            {
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                // Ensure that this product can be shipped (have at least one item to ship).
                var maxQtyToAdd = orderItem.GetItemsCanBeAddedToShipmentCount();
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0;

                if (quantities != null && quantities.ContainsKey(orderItem.Id))
                    qtyToAdd = quantities[orderItem.Id];
                else if (quantities == null)
                    qtyToAdd = maxQtyToAdd;

                if (qtyToAdd <= 0)
                    continue;

                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;

                    totalWeight += orderItemTotalWeight.Value;
                }

                if (shipment == null)
                {
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        Order = order,      // Otherwise order updated event would not be fired during InsertShipment.
                        TrackingNumber = trackingNumber,
                        TrackingUrl = trackingUrl,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }

                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd
                };

                shipment.ShipmentItems.Add(shipmentItem);
            }

            if (shipment != null && shipment.ShipmentItems.Count > 0)
            {
                shipment.TotalWeight = totalWeight;

                _shipmentService.InsertShipment(shipment);

                return shipment;
            }

            return null;
        }

        #endregion
    }
}
