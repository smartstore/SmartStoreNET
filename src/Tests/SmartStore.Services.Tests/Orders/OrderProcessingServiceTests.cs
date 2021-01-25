using System;
using System.Collections.Generic;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Orders
{
    [TestFixture]
    public class OrderProcessingServiceTests : ServiceTest
    {
        IWorkContext _workContext;
        IStoreContext _storeContext;
        ITaxService _taxService;
        IShippingService _shippingService;
        IShipmentService _shipmentService;
        IPaymentService _paymentService;
        IProviderManager _providerManager;
        ICheckoutAttributeParser _checkoutAttributeParser;
        IDiscountService _discountService;
        IGiftCardService _giftCardService;
        IGenericAttributeService _genericAttributeService;
        TaxSettings _taxSettings;
        RewardPointsSettings _rewardPointsSettings;
        ICategoryService _categoryService;
        IManufacturerService _manufacturerService;
        IProductAttributeParser _productAttributeParser;
        IProductAttributeService _productAttributeService;
        IPriceCalculationService _priceCalcService;
        IOrderTotalCalculationService _orderTotalCalcService;
        IAddressService _addressService;
        ShippingSettings _shippingSettings;
        IRepository<ShippingMethod> _shippingMethodRepository;
        IRepository<StoreMapping> _storeMappingRepository;
        IOrderService _orderService;
        IWebHelper _webHelper;
        ILocalizationService _localizationService;
        ILanguageService _languageService;
        IProductService _productService;
        IPriceFormatter _priceFormatter;
        IProductAttributeFormatter _productAttributeFormatter;
        IShoppingCartService _shoppingCartService;
        ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        ICustomerService _customerService;
        IEncryptionService _encryptionService;
        IMessageFactory _messageFactory;
        ICustomerActivityService _customerActivityService;
        ICurrencyService _currencyService;
        OrderSettings _orderSettings;
        LocalizationSettings _localizationSettings;
        ShoppingCartSettings _shoppingCartSettings;
        CatalogSettings _catalogSettings;
        IOrderProcessingService _orderProcessingService;
        IEventPublisher _eventPublisher;
        IAffiliateService _affiliateService;
        ISettingService _settingService;
        IDownloadService _downloadService;
        INewsLetterSubscriptionService _newsLetterSubscriptionService;
        ICommonServices _services;
        HttpRequestBase _httpRequestBase;
        IGeoCountryLookup _geoCountryLookup;
        Store _store;
        ICartRuleProvider _cartRuleProvider;

        [SetUp]
        public new void SetUp()
        {
            _workContext = null;
            _services = MockRepository.GenerateMock<ICommonServices>();

            _store = new Store() { Id = 1 };
            _storeContext = MockRepository.GenerateMock<IStoreContext>();
            _storeContext.Expect(x => x.CurrentStore).Return(_store);

            var pluginFinder = PluginFinder.Current;

            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();

            //price calculation service
            _discountService = MockRepository.GenerateMock<IDiscountService>();
            _categoryService = MockRepository.GenerateMock<ICategoryService>();
            _manufacturerService = MockRepository.GenerateMock<IManufacturerService>();
            _productAttributeParser = MockRepository.GenerateMock<IProductAttributeParser>();
            _productAttributeService = MockRepository.GenerateMock<IProductAttributeService>();
            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _localizationService = MockRepository.GenerateMock<ILocalizationService>();
            _settingService = MockRepository.GenerateMock<ISettingService>();
            _cartRuleProvider = MockRepository.GenerateMock<ICartRuleProvider>();

            //shipping
            _shippingSettings = new ShippingSettings();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames = new List<string>();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add("FixedRateTestShippingRateComputationMethod");
            _shippingMethodRepository = MockRepository.GenerateMock<IRepository<ShippingMethod>>();
            _storeMappingRepository = MockRepository.GenerateMock<IRepository<StoreMapping>>();

            _shippingService = new ShippingService(
                _shippingMethodRepository,
                _storeMappingRepository,
                _productAttributeParser,
                _productService,
                _checkoutAttributeParser,
                _genericAttributeService,
                _shippingSettings,
                _settingService,
                this.ProviderManager,
                _services,
                _cartRuleProvider);

            _shipmentService = MockRepository.GenerateMock<IShipmentService>();

            _paymentService = MockRepository.GenerateMock<IPaymentService>();
            _providerManager = MockRepository.GenerateMock<IProviderManager>();
            _checkoutAttributeParser = MockRepository.GenerateMock<ICheckoutAttributeParser>();
            _giftCardService = MockRepository.GenerateMock<IGiftCardService>();

            //tax
            _taxSettings = new TaxSettings();
            _taxSettings.ShippingIsTaxable = true;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;
            _taxSettings.DefaultTaxAddressId = 10;

            _addressService = MockRepository.GenerateMock<IAddressService>();
            _addressService.Expect(x => x.GetAddressById(_taxSettings.DefaultTaxAddressId)).Return(new Address() { Id = _taxSettings.DefaultTaxAddressId });
            _downloadService = MockRepository.GenerateMock<IDownloadService>();
            _httpRequestBase = MockRepository.GenerateMock<HttpRequestBase>();
            _geoCountryLookup = MockRepository.GenerateMock<IGeoCountryLookup>();

            _taxService = new TaxService(_addressService, _workContext, _taxSettings, _shoppingCartSettings, pluginFinder, _geoCountryLookup, this.ProviderManager);

            _rewardPointsSettings = new RewardPointsSettings();

            _priceCalcService = new PriceCalculationService(_discountService, _categoryService, _manufacturerService, _productAttributeParser, _productService,
                _catalogSettings, _productAttributeService, _downloadService, _services, _httpRequestBase, _taxService, _taxSettings);

            _orderTotalCalcService = new OrderTotalCalculationService(_workContext, _storeContext,
                _priceCalcService, _taxService, _shippingService, _providerManager,
                _checkoutAttributeParser, _discountService, _giftCardService, _genericAttributeService, _paymentService, _currencyService, _productAttributeParser,
                _taxSettings, _rewardPointsSettings, _shippingSettings, _shoppingCartSettings, _catalogSettings);

            _orderService = MockRepository.GenerateMock<IOrderService>();
            _webHelper = MockRepository.GenerateMock<IWebHelper>();
            _languageService = MockRepository.GenerateMock<ILanguageService>();
            _productService = MockRepository.GenerateMock<IProductService>();
            _priceFormatter = MockRepository.GenerateMock<IPriceFormatter>();
            _productAttributeFormatter = MockRepository.GenerateMock<IProductAttributeFormatter>();
            _shoppingCartService = MockRepository.GenerateMock<IShoppingCartService>();
            _checkoutAttributeFormatter = MockRepository.GenerateMock<ICheckoutAttributeFormatter>();
            _customerService = MockRepository.GenerateMock<ICustomerService>();
            _encryptionService = MockRepository.GenerateMock<IEncryptionService>();
            _messageFactory = MockRepository.GenerateMock<IMessageFactory>();
            _customerActivityService = MockRepository.GenerateMock<ICustomerActivityService>();
            _currencyService = MockRepository.GenerateMock<ICurrencyService>();
            _affiliateService = MockRepository.GenerateMock<IAffiliateService>();
            _newsLetterSubscriptionService = MockRepository.GenerateMock<INewsLetterSubscriptionService>();
            _affiliateService = MockRepository.GenerateMock<IAffiliateService>();
            _checkoutAttributeParser = MockRepository.GenerateMock<ICheckoutAttributeParser>();
            _downloadService = MockRepository.GenerateMock<IDownloadService>();

            _orderSettings = new OrderSettings();
            _localizationSettings = new LocalizationSettings();

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _orderProcessingService = new OrderProcessingService(_orderService, _webHelper,
                _localizationService, _languageService,
                _productService, _paymentService,
                _orderTotalCalcService, _priceCalcService, _priceFormatter,
                _productAttributeParser, _productAttributeFormatter,
                _giftCardService, _shoppingCartService, _checkoutAttributeFormatter, _checkoutAttributeParser,
                _shippingService, _shipmentService, _taxService,
                _customerService, _discountService,
                _encryptionService, _workContext, _storeContext,
                _messageFactory, _customerActivityService, _currencyService, _affiliateService,
                _eventPublisher, _genericAttributeService,
                _newsLetterSubscriptionService, _downloadService,
                _rewardPointsSettings,
                _orderSettings, _taxSettings, _localizationSettings,
                _shoppingCartSettings,
                _catalogSettings);
        }

        [Test]
        public void Ensure_order_can_only_be_cancelled_when_orderStatus_is_not_cancelled_yet()
        {
            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os != OrderStatus.Cancelled)
                            _orderProcessingService.CanCancelOrder(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanCancelOrder(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_marked_as_authorized_when_orderStatus_is_not_cancelled_and_paymentStatus_is_pending()
        {
            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os != OrderStatus.Cancelled && ps == PaymentStatus.Pending)
                            _orderProcessingService.CanMarkOrderAsAuthorized(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanMarkOrderAsAuthorized(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_captured_when_orderStatus_is_not_cancelled_or_pending_and_paymentstatus_is_authorized_and_paymentModule_supports_capture()
        {
            _paymentService.Expect(ps => ps.SupportCapture("paymentMethodSystemName_that_supports_capture")).Return(true);
            _paymentService.Expect(ps => ps.SupportCapture("paymentMethodSystemName_that_doesn't_support_capture")).Return(false);
            var order = new Order();


            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_capture";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if ((os != OrderStatus.Cancelled && os != OrderStatus.Pending)
                            && (ps == PaymentStatus.Authorized))
                            _orderProcessingService.CanCapture(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanCapture(order).ShouldBeFalse();
                    }


            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_capture";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanCapture(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_marked_as_paid_when_orderStatus_is_cancelled_or_paymentStatus_is_paid_or_refunded_or_voided()
        {
            var order = new Order();
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;
                        if (os == OrderStatus.Cancelled
                            || (ps == PaymentStatus.Paid || ps == PaymentStatus.Refunded || ps == PaymentStatus.Voided))
                            _orderProcessingService.CanMarkOrderAsPaid(order).ShouldBeFalse();
                        else
                            _orderProcessingService.CanMarkOrderAsPaid(order).ShouldBeTrue();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_refunded_when_paymentstatus_is_paid_and_paymentModule_supports_refund()
        {
            _paymentService.Expect(ps => ps.SupportRefund("paymentMethodSystemName_that_supports_refund")).Return(true);
            _paymentService.Expect(ps => ps.SupportRefund("paymentMethodSystemName_that_doesn't_support_refund")).Return(false);
            var order = new Order();
            order.OrderTotal = 1;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_refund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Paid)
                            _orderProcessingService.CanRefund(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanRefund(order).ShouldBeFalse();
                    }



            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_refund";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanRefund(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_refunded_when_orderTotal_is_zero()
        {
            _paymentService.Expect(ps => ps.SupportRefund("paymentMethodSystemName_that_supports_refund")).Return(true);
            var order = new Order();
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_refund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanRefund(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_refunded_offline_when_paymentstatus_is_paid()
        {
            var order = new Order()
            {
                OrderTotal = 1,
            };
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Paid)
                            _orderProcessingService.CanRefundOffline(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanRefundOffline(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_refunded_offline_when_orderTotal_is_zero()
        {
            var order = new Order();

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanRefundOffline(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_voided_when_paymentstatus_is_authorized_and_paymentModule_supports_void()
        {
            _paymentService.Expect(ps => ps.SupportVoid("paymentMethodSystemName_that_supports_void")).Return(true);
            _paymentService.Expect(ps => ps.SupportVoid("paymentMethodSystemName_that_doesn't_support_void")).Return(false);
            var order = new Order();
            order.OrderTotal = 1;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_void";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Authorized)
                            _orderProcessingService.CanVoid(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanVoid(order).ShouldBeFalse();
                    }



            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_void";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanVoid(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_voided_when_orderTotal_is_zero()
        {
            _paymentService.Expect(ps => ps.SupportVoid("paymentMethodSystemName_that_supports_void")).Return(true);
            var order = new Order();
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_void";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanVoid(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_voided_offline_when_paymentstatus_is_authorized()
        {
            var order = new Order()
            {
                OrderTotal = 1,
            };
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Authorized)
                            _orderProcessingService.CanVoidOffline(order).ShouldBeTrue();
                        else
                            _orderProcessingService.CanVoidOffline(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_voided_offline_when_orderTotal_is_zero()
        {
            var order = new Order();

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanVoidOffline(order).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_partially_refunded_when_paymentstatus_is_paid_or_partiallyRefunded_and_paymentModule_supports_partialRefund()
        {
            _paymentService.Expect(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_supports_partialrefund")).Return(true);
            _paymentService.Expect(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_doesn't_support_partialrefund")).Return(false);
            var order = new Order();
            order.OrderTotal = 100;
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_partialrefund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        if (ps == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                            _orderProcessingService.CanPartiallyRefund(order, 10).ShouldBeTrue();
                        else
                            _orderProcessingService.CanPartiallyRefund(order, 10).ShouldBeFalse();
                    }



            order.PaymentMethodSystemName = "paymentMethodSystemName_that_doesn't_support_partialrefund";
            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanPartiallyRefund(order, 10).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_partially_refunded_when_amountToRefund_is_greater_than_amount_that_can_be_refunded()
        {
            _paymentService.Expect(ps => ps.SupportPartiallyRefund("paymentMethodSystemName_that_supports_partialrefund")).Return(true);
            var order = new Order()
            {
                OrderTotal = 100,
                RefundedAmount = 30, //100-30=70 can be refunded
            };
            order.PaymentMethodSystemName = "paymentMethodSystemName_that_supports_partialrefund";

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanPartiallyRefund(order, 80).ShouldBeFalse();
                    }
        }

        [Test]
        public void Ensure_order_can_only_be_partially_refunded_offline_when_paymentstatus_is_paid_or_partiallyRefunded()
        {
            var order = new Order();
            order.OrderTotal = 100;

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        {
                            order.OrderStatus = os;
                            order.PaymentStatus = ps;
                            order.ShippingStatus = ss;

                            if (ps == PaymentStatus.Paid || order.PaymentStatus == PaymentStatus.PartiallyRefunded)
                                _orderProcessingService.CanPartiallyRefundOffline(order, 10).ShouldBeTrue();
                            else
                                _orderProcessingService.CanPartiallyRefundOffline(order, 10).ShouldBeFalse();
                        }
                    }
        }

        [Test]
        public void Ensure_order_cannot_be_partially_refunded_offline_when_amountToRefund_is_greater_than_amount_that_can_be_refunded()
        {
            var order = new Order()
            {
                OrderTotal = 100,
                RefundedAmount = 30, //100-30=70 can be refunded
            };

            foreach (OrderStatus os in Enum.GetValues(typeof(OrderStatus)))
                foreach (PaymentStatus ps in Enum.GetValues(typeof(PaymentStatus)))
                    foreach (ShippingStatus ss in Enum.GetValues(typeof(ShippingStatus)))
                    {
                        order.OrderStatus = os;
                        order.PaymentStatus = ps;
                        order.ShippingStatus = ss;

                        _orderProcessingService.CanPartiallyRefundOffline(order, 80).ShouldBeFalse();
                    }
        }

        //TODO write unit tests for the following methods:
        //PlaceOrder
        //CanCancelRecurringPayment, ProcessNextRecurringPayment, CancelRecurringPayment
    }
}
