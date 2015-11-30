using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order processing service
    /// </summary>
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields
        
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
		private readonly IAffiliateService _affiliateService;
        private readonly IEventPublisher _eventPublisher;
		private readonly IGenericAttributeService _genericAttributeService;

        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CurrencySettings _currencySettings;
		private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="productService">Product service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="orderTotalCalculationService">Order total calculationservice</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="productAttributeFormatter">Product attribute formatter</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="shoppingCartService">Shopping cart service</param>
        /// <param name="checkoutAttributeFormatter">Checkout attribute service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shipmentService">Shipment service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="customerActivityService">Customer activity service</param>
        /// <param name="currencyService">Currency service</param>
		/// <param name="affiliateService">Affiliate service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="orderSettings">Order settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="currencySettings">Currency settings</param>
        public OrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
			IStoreContext storeContext,
            IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
			IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
			IGenericAttributeService genericAttributeService,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings,
			ShoppingCartSettings shoppingCartSettings)
        {
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._productService = productService;
            this._paymentService = paymentService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._giftCardService = giftCardService;
            this._shoppingCartService = shoppingCartService;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._workflowMessageService = workflowMessageService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._taxService = taxService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._currencyService = currencyService;
			this._affiliateService = affiliateService;
            this._eventPublisher = eventPublisher;
			this._genericAttributeService = genericAttributeService;
            this._paymentSettings = paymentSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._localizationSettings = localizationSettings;
            this._currencySettings = currencySettings;
			this._shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Utilities

		private decimal Round(decimal value)
		{
			return (_shoppingCartSettings.RoundPricesDuringCalculation ? Math.Round(value, 2) : value);
		}

        private string TNote(string resKey)
        {
            return _localizationService.GetResource("Admin.OrderNotice." + resKey);
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

			// Truncate increases the risk of inaccuracy of rounding
            //int points = (int)Math.Truncate((amount ?? order.OrderTotal) / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);

			// why are points awarded for OrderTotal? wouldn't be OrderSubtotalInclTax better?

			int points = (int)Math.Round((amount ?? order.OrderTotal) / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);
            if (points == 0)
                return;

            //add reward points
            order.Customer.AddRewardPointsHistoryEntry(points, string.Format(_localizationService.GetResource("RewardPoints.Message.EarnedForOrder"), order.GetOrderNumber()));
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
            order.Customer.AddRewardPointsHistoryEntry(-points, string.Format(_localizationService.GetResource("RewardPoints.Message.ReducedForOrder"), order.GetOrderNumber()));

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
                    bool isRecipientNotified = gc.IsRecipientNotified;
                    if (gc.GiftCardType == GiftCardType.Virtual)
                    {
                        //send email for virtual gift card
                        if (!String.IsNullOrEmpty(gc.RecipientEmail) &&
                            !String.IsNullOrEmpty(gc.SenderEmail))
                        {
                            var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
                            if (customerLang == null)
                                customerLang = _languageService.GetAllLanguages().FirstOrDefault();
                            int queuedEmailId = _workflowMessageService.SendGiftCardNotification(gc, customerLang.Id);
                            if (queuedEmailId > 0)
                                isRecipientNotified = true;
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

            //order notes, notifications
            order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format(TNote("OrderStatusChanged"), os.GetLocalizedEnum(_localizationService)),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            _orderService.UpdateOrder(order);


            if (prevOrderStatus != OrderStatus.Complete && os == OrderStatus.Complete && notifyCustomer)
            {
                //notification
                int orderCompletedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId);
                if (orderCompletedCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format(TNote("CustomerCompletedEmailQueued"), orderCompletedCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }

            if (prevOrderStatus != OrderStatus.Cancelled && os == OrderStatus.Cancelled && notifyCustomer)
            {
                //notification
                int orderCancelledCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format(TNote("CustomerCancelledEmailQueued"), orderCancelledCustomerNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
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
            if (_orderSettings.GiftCards_Activated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Activated_OrderStatusId == (int)order.OrderStatus)
            {
                SetActivatedValueForPurchasedGiftCards(order, true);
            }

            //gift cards deactivation
            if (_orderSettings.GiftCards_Deactivated_OrderStatusId > 0 &&
               _orderSettings.GiftCards_Deactivated_OrderStatusId == (int)order.OrderStatus)
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

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public virtual PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest, Dictionary<string, string> extraData)
        {
            //think about moving functionality of processing recurring orders (after the initial order was placed) to ProcessNextRecurringPayment() method
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            if (processPaymentRequest.OrderGuid == Guid.Empty)
                processPaymentRequest.OrderGuid = Guid.NewGuid();

            var result = new PlaceOrderResult();
			var utcNow = DateTime.UtcNow;

            try
            {
                #region Order details (customer, totals)

                //Recurring orders. Load initial order
                Order initialOrder = _orderService.GetOrderById(processPaymentRequest.InitialOrderId);
                if (processPaymentRequest.IsRecurringPayment)
                {
                    if (initialOrder == null)
                        throw new ArgumentException("Initial order is not set for recurring payment");

                    processPaymentRequest.PaymentMethodSystemName = initialOrder.PaymentMethodSystemName;
                }

                //customer
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                if (customer == null)
                    throw new ArgumentException("Customer is not set");

				//affilites
				int affiliateId = 0;
				var affiliate = _affiliateService.GetAffiliateById(customer.AffiliateId);
				if (affiliate != null && affiliate.Active && !affiliate.Deleted)
					affiliateId = affiliate.Id;

                //customer currency
                string customerCurrencyCode = "";
                decimal customerCurrencyRate = decimal.Zero;
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

                //customer language
                Language customerLanguage = null;
                if (!processPaymentRequest.IsRecurringPayment)
				{
					customerLanguage = _languageService.GetLanguageById(customer.GetAttribute<int>(
						SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
				}
				else
				{
					customerLanguage = _languageService.GetLanguageById(initialOrder.CustomerLanguageId);
				}
                if (customerLanguage == null || !customerLanguage.Published)
                    customerLanguage = _workContext.WorkingLanguage;

                //check whether customer is guest
                if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new SmartException("Anonymous checkout is not allowed");

				var storeId = _storeContext.CurrentStore.Id;
                
                //load and validate customer shopping cart
                IList<OrganizedShoppingCartItem> cart = null;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    //load shopping cart
					cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);

                    if (cart.Count == 0)
                        throw new SmartException("Cart is empty");

                    //validate the entire shopping cart
					var warnings = _shoppingCartService.GetShoppingCartWarnings(cart,
						customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes), true);
                    if (warnings.Count > 0)
                    {
                        var warningsSb = new StringBuilder();
                        foreach (string warning in warnings)
                        {
                            warningsSb.Append(warning);
                            warningsSb.Append(";");
                        }
                        throw new SmartException(warningsSb.ToString());
                    }

                    //validate individual cart items
                    foreach (var sci in cart)
                    {
                        var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(customer, sci.Item.ShoppingCartType,
							sci.Item.Product, processPaymentRequest.StoreId, sci.Item.AttributesXml,
                            sci.Item.CustomerEnteredPrice, sci.Item.Quantity, false, childItems: sci.ChildItems);
                        if (sciWarnings.Count > 0)
                        {
                            var warningsSb = new StringBuilder();
                            foreach (string warning in sciWarnings)
                            {
                                warningsSb.Append(warning);
                                warningsSb.Append(";");
                            }
                            throw new SmartException(warningsSb.ToString());
                        }
                    }
                }

                //min totals validation
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    bool minOrderSubtotalAmountOk = ValidateMinOrderSubtotalAmount(cart);
                    if (!minOrderSubtotalAmountOk)
                    {
                        decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
                        throw new SmartException(string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false)));
                    }
                    bool minOrderTotalAmountOk = ValidateMinOrderTotalAmount(cart);
                    if (!minOrderTotalAmountOk)
                    {
                        decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                        throw new SmartException(string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false)));
                    }
                }
                
                //tax display type
                var customerTaxDisplayType = TaxDisplayType.IncludingTax;
                if (!processPaymentRequest.IsRecurringPayment)
                {
					customerTaxDisplayType = _workContext.GetTaxDisplayTypeFor(customer, processPaymentRequest.StoreId);
                }
                else
                {
                    customerTaxDisplayType = initialOrder.CustomerTaxDisplayType;
                }

                //checkout attributes
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

                //applied discount (used to store discount usage history)
                var appliedDiscounts = new List<Discount>();

                //sub total
                decimal orderSubTotalInclTax, orderSubTotalExclTax;
                decimal orderSubTotalDiscountInclTax = 0, orderSubTotalDiscountExclTax = 0;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    //sub total (incl tax)
                    decimal orderSubTotalDiscountAmount1 = decimal.Zero;
                    Discount orderSubTotalAppliedDiscount1 = null;
                    decimal subTotalWithoutDiscountBase1 = decimal.Zero;
                    decimal subTotalWithDiscountBase1 = decimal.Zero;

                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                        true, out orderSubTotalDiscountAmount1, out orderSubTotalAppliedDiscount1, out subTotalWithoutDiscountBase1, out subTotalWithDiscountBase1);

                    orderSubTotalInclTax = subTotalWithoutDiscountBase1;
                    orderSubTotalDiscountInclTax = orderSubTotalDiscountAmount1;

                    //discount history
                    if (orderSubTotalAppliedDiscount1 != null && !appliedDiscounts.Any(x => x.Id == orderSubTotalAppliedDiscount1.Id))
                        appliedDiscounts.Add(orderSubTotalAppliedDiscount1);

                    //sub total (excl tax)
                    decimal orderSubTotalDiscountAmount2 = decimal.Zero;
                    Discount orderSubTotalAppliedDiscount2 = null;
                    decimal subTotalWithoutDiscountBase2 = decimal.Zero;
                    decimal subTotalWithDiscountBase2 = decimal.Zero;

                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                        false, out orderSubTotalDiscountAmount2, out orderSubTotalAppliedDiscount2, out subTotalWithoutDiscountBase2, out subTotalWithDiscountBase2);

                    orderSubTotalExclTax = subTotalWithoutDiscountBase2;
                    orderSubTotalDiscountExclTax = orderSubTotalDiscountAmount2;
                }
                else
                {
                    orderSubTotalInclTax = initialOrder.OrderSubtotalInclTax;
                    orderSubTotalExclTax = initialOrder.OrderSubtotalExclTax;
                }


                //shipping info
                bool shoppingCartRequiresShipping = false;
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

                //shipping total
                decimal? orderShippingTotalInclTax, orderShippingTotalExclTax = null;
				decimal orderShippingTaxRate = decimal.Zero;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    Discount shippingTotalDiscount = null;
                    orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, true, out orderShippingTaxRate, out shippingTotalDiscount);
                    orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart, false);
                    if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                        throw new SmartException("Shipping total couldn't be calculated");

                    if (shippingTotalDiscount != null && !appliedDiscounts.Any(x => x.Id == shippingTotalDiscount.Id))
                        appliedDiscounts.Add(shippingTotalDiscount);
                }
                else
                {
                    orderShippingTotalInclTax = initialOrder.OrderShippingInclTax;
                    orderShippingTotalExclTax = initialOrder.OrderShippingExclTax;
					orderShippingTaxRate = initialOrder.OrderShippingTaxRate;
                }

                //payment total
				decimal paymentAdditionalFeeInclTax, paymentAdditionalFeeExclTax, paymentAdditionalFeeTaxRate;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    decimal paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, processPaymentRequest.PaymentMethodSystemName);
                    paymentAdditionalFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, true, customer, out paymentAdditionalFeeTaxRate);
                    paymentAdditionalFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentAdditionalFee, false, customer);
                }
                else
                {
                    paymentAdditionalFeeInclTax = initialOrder.PaymentMethodAdditionalFeeInclTax;
                    paymentAdditionalFeeExclTax = initialOrder.PaymentMethodAdditionalFeeExclTax;
					paymentAdditionalFeeTaxRate = initialOrder.PaymentMethodAdditionalFeeTaxRate;
                }

                //tax total
                decimal orderTaxTotal = decimal.Zero;
                string vatNumber = "", taxRates = "";
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    //tax amount
                    SortedDictionary<decimal, decimal> taxRatesDictionary = null;
                    orderTaxTotal = _orderTotalCalculationService.GetTaxTotal(cart, out taxRatesDictionary);

                    //VAT number
					var customerVatStatus = (VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
					if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
						vatNumber = customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);

                    //tax rates
                    foreach (var kvp in taxRatesDictionary)
                    {
                        var taxRate = kvp.Key;
                        var taxValue = kvp.Value;
                        taxRates += string.Format("{0}:{1};   ", taxRate.ToString(CultureInfo.InvariantCulture), taxValue.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    orderTaxTotal = initialOrder.OrderTax;
                    //VAT number
                    vatNumber = initialOrder.VatNumber;
                }
				processPaymentRequest.OrderTax = orderTaxTotal;

                //order total (and applied discounts, gift cards, reward points)
                decimal? orderTotal = null;
                decimal orderDiscountAmount = decimal.Zero;
                List<AppliedGiftCard> appliedGiftCards = null;
                int redeemedRewardPoints = 0;
                decimal redeemedRewardPointsAmount = decimal.Zero;
                if (!processPaymentRequest.IsRecurringPayment)
                {
                    Discount orderAppliedDiscount = null;
                    orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                        out orderDiscountAmount, out orderAppliedDiscount, out appliedGiftCards,
                        out redeemedRewardPoints, out redeemedRewardPointsAmount);
                    if (!orderTotal.HasValue)
                        throw new SmartException("Order total couldn't be calculated");

                    //discount history
                    if (orderAppliedDiscount != null && !appliedDiscounts.Any(x => x.Id == orderAppliedDiscount.Id))
                        appliedDiscounts.Add(orderAppliedDiscount);
                }
                else
                {
                    orderDiscountAmount = initialOrder.OrderDiscount;
                    orderTotal = initialOrder.OrderTotal;
                }
                processPaymentRequest.OrderTotal = orderTotal.Value;

                #endregion

				#region Addresses & pre payment workflow
				
				// give payment processor the opportunity to fullfill billing address
				var preProcessPaymentResult = _paymentService.PreProcessPayment(processPaymentRequest);

				if (!preProcessPaymentResult.Success)
				{
					result.Errors.AddRange(preProcessPaymentResult.Errors);
					result.Errors.Add(_localizationService.GetResource("Common.Error.PreProcessPayment"));
					return result;					
				}

				Address billingAddress = null;
				if (!processPaymentRequest.IsRecurringPayment)
				{
					if (customer.BillingAddress == null)
						throw new SmartException("Billing address is not provided");

					if (!customer.BillingAddress.Email.IsEmail())
						throw new SmartException("Email is not valid");

					billingAddress = (Address)customer.BillingAddress.Clone();
				}
				else
				{
					if (initialOrder.BillingAddress == null)
						throw new SmartException("Billing address is not available");

					billingAddress = (Address)initialOrder.BillingAddress.Clone();
				}

				if (billingAddress.Country != null && !billingAddress.Country.AllowsBilling)
					throw new SmartException(string.Format("Country '{0}' is not allowed for billing", billingAddress.Country.Name));

				Address shippingAddress = null;
				if (shoppingCartRequiresShipping)
				{
					if (!processPaymentRequest.IsRecurringPayment)
					{
						if (customer.ShippingAddress == null)
							throw new SmartException("Shipping address is not provided");

						if (!customer.ShippingAddress.Email.IsEmail())
							throw new SmartException("Email is not valid");

						shippingAddress = (Address)customer.ShippingAddress.Clone();
					}
					else
					{
						if (initialOrder.ShippingAddress == null)
							throw new SmartException("Shipping address is not available");

						shippingAddress = (Address)initialOrder.ShippingAddress.Clone();
					}

					if (shippingAddress.Country != null && !shippingAddress.Country.AllowsShipping)
						throw new SmartException(string.Format("Country '{0}' is not allowed for shipping", shippingAddress.Country.Name));
				}

				#endregion

				#region Payment workflow

				//skip payment workflow if order total equals zero
                bool skipPaymentWorkflow = false;
                if (orderTotal.Value == decimal.Zero)
                    skipPaymentWorkflow = true;

                //payment workflow
                Provider<IPaymentMethod> paymentMethod = null;
				if (!skipPaymentWorkflow)
				{
					paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
					if (paymentMethod == null)
						throw new SmartException("Payment method couldn't be loaded");

					//ensure that payment method is active
					if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
						throw new SmartException("Payment method is not active");
				}
				else
				{
					processPaymentRequest.PaymentMethodSystemName = "";
				}

                //recurring or standard shopping cart?
                bool isRecurringShoppingCart = false;
				if (!processPaymentRequest.IsRecurringPayment)
				{
					isRecurringShoppingCart = cart.IsRecurring();
					if (isRecurringShoppingCart)
					{
						int recurringCycleLength = 0;
						RecurringProductCyclePeriod recurringCyclePeriod;
						int recurringTotalCycles = 0;
						string recurringCyclesError = cart.GetRecurringCycleInfo(_localizationService, out recurringCycleLength, out recurringCyclePeriod, out recurringTotalCycles);
						if (!string.IsNullOrEmpty(recurringCyclesError))
							throw new SmartException(recurringCyclesError);
						processPaymentRequest.RecurringCycleLength = recurringCycleLength;
						processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
						processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;
					}
				}
				else
				{
					isRecurringShoppingCart = true;
				}

                //process payment
                ProcessPaymentResult processPaymentResult = null;
                if (!skipPaymentWorkflow)
                {
                    if (!processPaymentRequest.IsRecurringPayment)
                    {
                        if (isRecurringShoppingCart)
                        {
                            //recurring cart
                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new SmartException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                case RecurringPaymentType.Automatic:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                default:
                                    throw new SmartException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            //standard cart
                            processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                        }
                    }
                    else
                    {
                        if (isRecurringShoppingCart)
                        {
                            //Old credit card info
                            processPaymentRequest.CreditCardType = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardType) : "";
                            processPaymentRequest.CreditCardName = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardName) : "";
                            processPaymentRequest.CreditCardNumber = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardNumber) : "";
                            //MaskedCreditCardNumber 
                            processPaymentRequest.CreditCardCvv2 = initialOrder.AllowStoringCreditCardNumber ? _encryptionService.DecryptText(initialOrder.CardCvv2) : "";
                            try
                            {
                                processPaymentRequest.CreditCardExpireMonth = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationMonth)) : 0;
                                processPaymentRequest.CreditCardExpireYear = initialOrder.AllowStoringCreditCardNumber ? Convert.ToInt32(_encryptionService.DecryptText(initialOrder.CardExpirationYear)) : 0;
                            }
                            catch {}

                            var recurringPaymentType = _paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName);
                            switch (recurringPaymentType)
                            {
                                case RecurringPaymentType.NotSupported:
                                    throw new SmartException("Recurring payments are not supported by selected payment method");
                                case RecurringPaymentType.Manual:
                                    processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                    break;
                                case RecurringPaymentType.Automatic:
                                    //payment is processed on payment gateway site
                                    processPaymentResult = new ProcessPaymentResult();
                                    break;
                                default:
                                    throw new SmartException("Not supported recurring payment type");
                            }
                        }
                        else
                        {
                            throw new SmartException("No recurring products");
                        }
                    }
                }
                else
                {
                    //payment is not required
                    if (processPaymentResult == null)
                        processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }

                if (processPaymentResult == null)
                    throw new SmartException("processPaymentResult is not available");

                #endregion

                if (processPaymentResult.Success)
                {
                    //save order in data storage
                    //uncomment this line to support transactions
                    //using (var scope = new System.Transactions.TransactionScope())
                    {
                        #region Save order details

                        var shippingStatus = ShippingStatus.NotYetShipped;
                        if (!shoppingCartRequiresShipping)
                            shippingStatus = ShippingStatus.ShippingNotRequired;
                        
                        var order = new Order()
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
                            OrderTotal = orderTotal.Value,
                            RefundedAmount = decimal.Zero,
                            OrderDiscount = orderDiscountAmount,
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
							CreatedOnUtc = utcNow,
							UpdatedOnUtc = utcNow,
                            CustomerOrderComment = extraData.ContainsKey("CustomerComment") ? extraData["CustomerComment"] : ""
                        };
                        _orderService.InsertOrder(order);

                        result.PlacedOrder = order;

                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            //move shopping cart items to order products
                            foreach (var sc in cart)
                            {
								sc.Item.Product.MergeWithCombination(sc.Item.AttributesXml);

                                //prices
                                decimal taxRate = decimal.Zero;
								decimal unitPriceTaxRate = decimal.Zero;
                                decimal scUnitPrice = _priceCalculationService.GetUnitPrice(sc, true);
                                decimal scSubTotal = _priceCalculationService.GetSubTotal(sc, true);
								decimal scUnitPriceInclTax = _taxService.GetProductPrice(sc.Item.Product, scUnitPrice, true, customer, out unitPriceTaxRate);
                                decimal scUnitPriceExclTax = _taxService.GetProductPrice(sc.Item.Product, scUnitPrice, false, customer, out taxRate);
                                decimal scSubTotalInclTax = _taxService.GetProductPrice(sc.Item.Product, scSubTotal, true, customer, out taxRate);
                                decimal scSubTotalExclTax = _taxService.GetProductPrice(sc.Item.Product, scSubTotal, false, customer, out taxRate);

                                //discounts
                                Discount scDiscount = null;
                                decimal discountAmount = _priceCalculationService.GetDiscountAmount(sc, out scDiscount);
                                decimal discountAmountInclTax = _taxService.GetProductPrice(sc.Item.Product, discountAmount, true, customer, out taxRate);
                                decimal discountAmountExclTax = _taxService.GetProductPrice(sc.Item.Product, discountAmount, false, customer, out taxRate);
                                
								if (scDiscount != null && !appliedDiscounts.Any(x => x.Id == scDiscount.Id))
                                    appliedDiscounts.Add(scDiscount);

                                //attributes
                                string attributeDescription = _productAttributeFormatter.FormatAttributes(sc.Item.Product, sc.Item.AttributesXml, customer);

                                var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                                //save order item
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
									ProductCost = _priceCalculationService.GetProductCost(sc.Item.Product, sc.Item.AttributesXml)
                                };

								if (sc.Item.Product.ProductType == ProductType.BundledProduct && sc.ChildItems != null)
								{
									var listBundleData = new List<ProductBundleItemOrderData>();

									foreach (var childItem in sc.ChildItems)
									{
										decimal bundleItemSubTotal = _taxService.GetProductPrice(childItem.Item.Product, _priceCalculationService.GetSubTotal(childItem, true), out taxRate);

										string attributesInfo = _productAttributeFormatter.FormatAttributes(childItem.Item.Product, childItem.Item.AttributesXml, order.Customer,
											renderPrices: false, allowHyperlinks: false);

										childItem.BundleItemData.ToOrderData(listBundleData, bundleItemSubTotal, childItem.Item.AttributesXml, attributesInfo);
									}

									orderItem.SetBundleData(listBundleData);
								}

                                order.OrderItems.Add(orderItem);
                                _orderService.UpdateOrder(order);

                                //gift cards
                                if (sc.Item.Product.IsGiftCard)
                                {
                                    string giftCardRecipientName, giftCardRecipientEmail,
                                        giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                                    _productAttributeParser.GetGiftCardAttribute(sc.Item.AttributesXml,
                                        out giftCardRecipientName, out giftCardRecipientEmail,
                                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

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

                                //inventory
								_productService.AdjustInventory(sc, true);
                            }

                            //clear shopping cart
                            cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci.Item, false));
                        }
                        else
                        {
                            //recurring payment
                            var initialOrderItems = initialOrder.OrderItems;
                            foreach (var orderItem in initialOrderItems)
                            {
                                //save item
                                var newOrderItem = new OrderItem()
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
									ProductCost = orderItem.ProductCost
                                };
                                order.OrderItems.Add(newOrderItem);
                                _orderService.UpdateOrder(order);

                                //gift cards
                                if (orderItem.Product.IsGiftCard)
                                {
                                    string giftCardRecipientName, giftCardRecipientEmail, giftCardSenderName, giftCardSenderEmail, giftCardMessage;

                                    _productAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml,
                                        out giftCardRecipientName, out giftCardRecipientEmail,
                                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                    for (int i = 0; i < orderItem.Quantity; i++)
                                    {
                                        var gc = new GiftCard()
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

                                //inventory
								_productService.AdjustInventory(orderItem, true, orderItem.Quantity);
                            }
                        }

                        //discount usage history
						if (!processPaymentRequest.IsRecurringPayment)
						{
							foreach (var discount in appliedDiscounts)
							{
								var duh = new DiscountUsageHistory()
								{
									Discount = discount,
									Order = order,
									CreatedOnUtc = utcNow
								};
								_discountService.InsertDiscountUsageHistory(duh);
							}
						}

                        //gift card usage history
						if (!processPaymentRequest.IsRecurringPayment && appliedGiftCards != null)
						{
							foreach (var agc in appliedGiftCards)
							{
								decimal amountUsed = agc.AmountCanBeUsed;
								var gcuh = new GiftCardUsageHistory()
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

                        //reward points history
                        if (redeemedRewardPointsAmount > decimal.Zero)
                        {
                            customer.AddRewardPointsHistoryEntry(-redeemedRewardPoints,
                                string.Format(_localizationService.GetResource("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.GetOrderNumber()),
                                order,
                                redeemedRewardPointsAmount);
                            _customerService.UpdateCustomer(customer);
                        }

                        //recurring orders
                        if (!processPaymentRequest.IsRecurringPayment && isRecurringShoppingCart)
                        {
                            //create recurring payment (the first payment)
                            var rp = new RecurringPayment()
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
                                    {
                                        //not supported
                                    }
                                    break;
                                case RecurringPaymentType.Manual:
                                    {
                                        //first payment
                                        var rph = new RecurringPaymentHistory()
                                        {
                                            RecurringPayment = rp,
											CreatedOnUtc = utcNow,
                                            OrderId = order.Id,
                                        };
                                        rp.RecurringPaymentHistory.Add(rph);
                                        _orderService.UpdateRecurringPayment(rp);
                                    }
                                    break;
                                case RecurringPaymentType.Automatic:
                                    {
                                        //will be created later (process is automated)
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        #endregion

                        #region Notifications, notes and attributes
                        
                        //notes, messages
                        order.OrderNotes.Add(new OrderNote
                            {
                                Note = TNote("OrderPlaced"),
                                DisplayToCustomer = false,
								CreatedOnUtc = utcNow
                            });
                        _orderService.UpdateOrder(order);

                        //send email notifications
                        int orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
                        if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
                        {
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = string.Format(TNote("MerchantEmailQueued"), orderPlacedStoreOwnerNotificationQueuedEmailId),
                                DisplayToCustomer = false,
								CreatedOnUtc = utcNow
                            });
                            _orderService.UpdateOrder(order);
                        }

                        int orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId);
                        if (orderPlacedCustomerNotificationQueuedEmailId > 0)
                        {
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = string.Format(TNote("CustomerEmailQueued"), orderPlacedCustomerNotificationQueuedEmailId),
                                DisplayToCustomer = false,
								CreatedOnUtc = utcNow
                            });
                            _orderService.UpdateOrder(order);
                        }

                        //check order status
                        CheckOrderStatus(order);

                        //reset checkout data
                        if (!processPaymentRequest.IsRecurringPayment)
							_customerService.ResetCheckoutData(customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);

						// check for generic attributes to be inserted automatically
						foreach (var customProperty in processPaymentRequest.CustomProperties.Where(x => x.Key.HasValue() && x.Value.AutoCreateGenericAttribute))
						{
							_genericAttributeService.SaveAttribute<object>(order, customProperty.Key, customProperty.Value.Value, order.StoreId);
						}

                        //uncomment this line to support transactions
                        //scope.Complete();

                        //raise event       
                        _eventPublisher.PublishOrderPlaced(order);

                        if (!processPaymentRequest.IsRecurringPayment)
                        {
                            _customerActivityService.InsertActivity(
                                "PublicStore.PlaceOrder",
                                _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"),
                                order.GetOrderNumber());
                        }
						
                        //raise event         
                        if (order.PaymentStatus == PaymentStatus.Paid)
                        {
                            _eventPublisher.PublishOrderPaid(order);
                        }
                        #endregion
                    }
                }
                else
                {
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(paymentError);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

			if (result.Errors.Count > 0)
			{
				_logger.Error(string.Join(" ", result.Errors));
			}

            return result;
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //reward points
            ReduceRewardPoints(order);

            //cancel recurring payments
            var recurringPayments = _orderService.SearchRecurringPayments(0, 0, order.Id, null);
            foreach (var rp in recurringPayments)
            {
                //use errors?
                var errors = CancelRecurringPayment(rp);
            }

            //Adjust inventory
			foreach (var orderItem in order.OrderItems)
			{
				_productService.AdjustInventory(orderItem, false, orderItem.Quantity);
			}

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = TNote("OrderDeleted"),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            
            //now delete an order
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
                    throw new SmartException("Recurring payment is not active");

                var initialOrder = recurringPayment.InitialOrder;
                if (initialOrder == null)
                    throw new SmartException("Initial order could not be loaded");

                var customer = initialOrder.Customer;
                if (customer == null)
                    throw new SmartException("Customer could not be loaded");

                var nextPaymentDate = recurringPayment.NextPaymentDate;
                if (!nextPaymentDate.HasValue)
                    throw new SmartException("Next payment date could not be calculated");

                //payment info
                var paymentInfo = new ProcessPaymentRequest()
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
                        throw new SmartException("Placed order could not be loaded");

                    var rph = new RecurringPaymentHistory()
                    {
                        RecurringPayment = recurringPayment,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = result.PlacedOrder.Id,
                    };
                    recurringPayment.RecurringPaymentHistory.Add(rph);
                    _orderService.UpdateRecurringPayment(recurringPayment);
                }
                else
                {
                    string error = "";
                    for (int i = 0; i < result.Errors.Count; i++)
                    {
                        error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                        if (i != result.Errors.Count - 1)
                            error += ". ";
                    }
                    throw new SmartException(error);
                }
            }
            catch (Exception exc)
            {
                _logger.Error(string.Format("Error while processing recurring order. {0}", exc.Message), exc);
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
                return new List<string>() { "Initial order could not be loaded" };


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


                    //add a note
                    initialOrder.OrderNotes.Add(new OrderNote()
                    {
                        Note = TNote("RecurringPaymentCancelled"),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(initialOrder);

                    //notify a store owner
                    _workflowMessageService
                        .SendRecurringPaymentCancelledStoreOwnerNotification(recurringPayment, 
                        _localizationSettings.DefaultAdminLanguageId);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CancelRecurringPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                initialOrder.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format(TNote("RecurringPaymentCancellationError"), error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(initialOrder);

                //log it
                string logError = string.Format("Error cancelling recurring payment. Order #{0}. Error: {1}", initialOrder.Id, error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
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
                throw new Exception("Order cannot be loaded");

            if (shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is already shipped");

            shipment.ShippedDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            //check whether we have more items to ship
            if (order.HasItemsToAddToShipment() || order.HasItemsToShip())
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            else
                order.ShippingStatusId = (int)ShippingStatus.Shipped;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format(TNote("ShipmentSent"), shipment.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            _orderService.UpdateOrder(order);

            if (notifyCustomer)
            {
                //notify customer
                int queuedEmailId = _workflowMessageService.SendShipmentSentCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = string.Format(TNote("CustomerShippedEmailQueued"), queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
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
                throw new Exception("Order cannot be loaded");

            if (shipment.DeliveryDateUtc.HasValue)
                throw new Exception("This shipment is already delivered");

            shipment.DeliveryDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            if (!order.HasItemsToAddToShipment() && !order.HasItemsToShip() && !order.HasItemsToDeliver())
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = string.Format(TNote("ShipmentDelivered"), shipment.Id),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            if (notifyCustomer)
            {
                //send email notification
                int queuedEmailId = _workflowMessageService.SendShipmentDeliveredCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = string.Format(TNote("CustomerDeliveredEmailQueued"), queuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
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
            if (order == null)
                throw new ArgumentNullException("order");

            if (!CanCancelOrder(order))
                throw new SmartException("Cannot do cancel for order.");

            //Cancel order
            SetOrderStatus(order, OrderStatus.Cancelled, notifyCustomer);

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = TNote("OrderCancelled"),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //cancel recurring payments
            var recurringPayments = _orderService.SearchRecurringPayments(0, 0, order.Id, null);
            foreach (var rp in recurringPayments)
            {
                //use errors?
                var errors = CancelRecurringPayment(rp);
            }

            //Adjust inventory
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
				decimal priceInclTax = Round(context.QuantityNew * oi.UnitPriceInclTax);
				decimal priceExclTax = Round(context.QuantityNew * oi.UnitPriceExclTax);

				decimal deltaPriceInclTax = priceInclTax - (context.IsNewOrderItem ? decimal.Zero : oi.PriceInclTax);
				decimal deltaPriceExclTax = priceExclTax - (context.IsNewOrderItem ? decimal.Zero : oi.PriceExclTax);

				oi.Quantity = context.QuantityNew;
				oi.PriceInclTax = Round(priceInclTax);
				oi.PriceExclTax = Round(priceExclTax);

				decimal subtotalInclTax = oi.Order.OrderSubtotalInclTax + deltaPriceInclTax;
				decimal subtotalExclTax = oi.Order.OrderSubtotalExclTax + deltaPriceExclTax;

				oi.Order.OrderSubtotalInclTax = Round(subtotalInclTax);
				oi.Order.OrderSubtotalExclTax = Round(subtotalExclTax);

				decimal discountInclTax = oi.DiscountAmountInclTax * context.QuantityChangeFactor;
				decimal discountExclTax = oi.DiscountAmountExclTax * context.QuantityChangeFactor;

				decimal deltaDiscountInclTax = discountInclTax - oi.DiscountAmountInclTax;
				decimal deltaDiscountExclTax = discountExclTax - oi.DiscountAmountExclTax;

				oi.DiscountAmountInclTax = Round(discountInclTax);
				oi.DiscountAmountExclTax = Round(discountExclTax);

				decimal total = Math.Max(oi.Order.OrderTotal + deltaPriceInclTax, 0);
				decimal tax = Math.Max(oi.Order.OrderTax + (deltaPriceInclTax - deltaPriceExclTax), 0);

				oi.Order.OrderTotal = Round(total);
				oi.Order.OrderTax = Round(tax);

				_orderService.UpdateOrder(oi.Order);
			}

			if (context.AdjustInventory && context.QuantityDelta != 0)
			{
				context.Inventory = _productService.AdjustInventory(oi, context.QuantityDelta > 0, Math.Abs(context.QuantityDelta));
			}

			if (context.UpdateRewardPoints && context.QuantityDelta < 0)
			{
				// we reduce but we do not award points subsequently. they can be awarded once per order anyway (see Order.RewardPointsWereAdded).
				// UpdateRewardPoints only visible for unpending orders (see RewardPointsSettingsValidator).
				// note: reducing can of cource only work if oi.UnitPriceExclTax has not been changed!
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

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = TNote("OrderMarkedAsAuthorized"),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

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
                throw new SmartException("You can't mark this order as completed");

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

            if (order.OrderStatus == OrderStatus.Cancelled ||
                order.OrderStatus == OrderStatus.Pending)
                return false;

            if (order.PaymentStatus == PaymentStatus.Authorized &&
                _paymentService.SupportCapture(order.PaymentMethodSystemName))
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
                throw new SmartException("Cannot do capture for order.");

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
                        paidDate = DateTime.UtcNow;

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatus = result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;
                    _orderService.UpdateOrder(order);

                    //add a note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = TNote("OrderCaptured"),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    CheckOrderStatus(order);

                    //raise event         
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        _eventPublisher.PublishOrderPaid(order);
                    }
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CapturePaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }


            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format("Unable to capture order. {0}", error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format(TNote("OrderCaptureError"), order.GetOrderNumber(), error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
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
                throw new SmartException("You can't mark this order as paid");

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = TNote("OrderMarkedAsPaid"),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

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

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatus == PaymentStatus.Paid &&
                _paymentService.SupportRefund(order.PaymentMethodSystemName))
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
                throw new SmartException("Cannot do refund for order.");

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
                    //total amount refunded
                    decimal totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                    //update order info
                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatus = result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    //add a note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = string.Format(TNote("OrderRefunded"), _priceFormatter.FormatPrice(request.AmountToRefund, true, false)),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //check order status
                    CheckOrderStatus(order);
                }

            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format(TNote("OrderRefundError"), error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error refunding order '{0}'. Error: {1}", order.GetOrderNumber(), error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
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

            //uncomment the lines below in order to allow this operation for cancelled orders
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
                throw new SmartException("You can't refund this order");

            //amout to refund
            decimal amountToRefund = order.OrderTotal;

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatus = PaymentStatus.Refunded;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = string.Format(TNote("OrderMarkedAsRefunded"), _priceFormatter.FormatPrice(amountToRefund, true, false)),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

            //check order status
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
                throw new SmartException("Cannot do partial refund for order.");

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


                    //add a note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = string.Format(TNote("OrderPartiallyRefunded"), _priceFormatter.FormatPrice(amountToRefund, true, false)),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format(TNote("OrderPartiallyRefundError"), error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error refunding order '{0}'. Error: {1}", order.GetOrderNumber(), error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
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
                throw new SmartException("You can't partially refund (offline) this order");

            //total amount refunded
            decimal totalAmountRefunded = order.RefundedAmount + amountToRefund;

            //update order info
            order.RefundedAmount = totalAmountRefunded;
            //if (order.OrderTotal == totalAmountRefunded), then set order.PaymentStatus = PaymentStatus.Refunded;
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = string.Format(TNote("OrderMarkedAsPartiallyRefunded"), _priceFormatter.FormatPrice(amountToRefund, true, false)),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

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
                throw new SmartException("Cannot do void for order.");

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

                    //add a note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = TNote("OrderVoided"),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //check order status
                    CheckOrderStatus(order);
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new VoidPaymentResult();
                result.AddError(string.Format("Error: {0}. Full exception: {1}", exc.Message, exc.ToString()));
            }

            //process errors
            string error = "";
            for (int i = 0; i < result.Errors.Count; i++)
            {
                error += string.Format("Error {0}: {1}", i, result.Errors[i]);
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }
            if (!String.IsNullOrEmpty(error))
            {
                //add a note
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Format(TNote("OrderVoidError"), error),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //log it
                string logError = string.Format("Error voiding order '{0}'. Error: {1}", order.GetOrderNumber(), error);
                _logger.InsertLog(LogLevel.Error, logError, logError);
            }
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
                throw new SmartException("You can't void this order");

            order.PaymentStatusId = (int)PaymentStatus.Voided;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote()
            {
                Note = TNote("OrderMarkedAsVoided"),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);

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
				bool isBundle = (orderItem.Product.ProductType == ProductType.BundledProduct);

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
        
        /// <summary>
        /// Check whether return request is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public virtual bool IsReturnRequestAllowed(Order order)
        {
            if (!_orderSettings.ReturnRequestsEnabled)
                return false;

            if (order == null || order.Deleted)
                return false;

            if (order.OrderStatus != OrderStatus.Complete)
                return false;

            bool numberOfDaysReturnRequestAvailableValid = false;
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


        /// <summary>
        /// Valdiate minimum order sub-total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order sub-total amount is not reached</returns>
        public virtual bool ValidateMinOrderSubtotalAmount(IList<OrganizedShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            //min order amount sub-total validation
            if (cart.Count > 0 && _orderSettings.MinOrderSubtotalAmount > decimal.Zero)
            {
                //subtotal
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                if (subTotalWithoutDiscountBase < _orderSettings.MinOrderSubtotalAmount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valdiate minimum order total amount
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order total amount is not reached</returns>
		public virtual bool ValidateMinOrderTotalAmount(IList<OrganizedShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (cart.Count > 0 && _orderSettings.MinOrderTotalAmount > decimal.Zero)
            {
                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value < _orderSettings.MinOrderTotalAmount)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
