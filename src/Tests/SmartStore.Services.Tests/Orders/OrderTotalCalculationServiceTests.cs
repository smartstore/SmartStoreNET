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
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Orders
{
    [TestFixture]
    public class OrderTotalCalculationServiceTests : ServiceTest
    {
        IWorkContext _workContext;
        IStoreContext _storeContext;
        ITaxService _taxService;
        IShippingService _shippingService;
        IProviderManager _providerManager;
        ICheckoutAttributeParser _checkoutAttributeParser;
        IDiscountService _discountService;
        IGiftCardService _giftCardService;
        IGenericAttributeService _genericAttributeService;
        IPaymentService _paymentService;
        ICurrencyService _currencyService;
        TaxSettings _taxSettings;
        RewardPointsSettings _rewardPointsSettings;
        ICategoryService _categoryService;
        IManufacturerService _manufacturerService;
        IProductAttributeParser _productAttributeParser;
        IProductService _productService;
        IProductAttributeService _productAttributeService;
        IPriceCalculationService _priceCalcService;
        IOrderTotalCalculationService _orderTotalCalcService;
        IAddressService _addressService;
        ShippingSettings _shippingSettings;
        IRepository<ShippingMethod> _shippingMethodRepository;
        IRepository<StoreMapping> _storeMappingRepository;
        ShoppingCartSettings _shoppingCartSettings;
        CatalogSettings _catalogSettings;
        IEventPublisher _eventPublisher;
        ISettingService _settingService;
        IDownloadService _downloadService;
        ICommonServices _services;
        HttpRequestBase _httpRequestBase;
        IGeoCountryLookup _geoCountryLookup;
        Store _store;
        Currency _currency;
        ICartRuleProvider _cartRuleProvider;

        [SetUp]
        public new void SetUp()
        {
            _store = new Store { Id = 1 };
            _storeContext = MockRepository.GenerateMock<IStoreContext>();
            _storeContext.Expect(x => x.CurrentStore).Return(_store);

            _currency = new Currency { Id = 1 };
            _workContext = MockRepository.GenerateMock<IWorkContext>();
            _workContext.Expect(x => x.WorkingCurrency).Return(_currency);

            _services = MockRepository.GenerateMock<ICommonServices>();
            _services.Expect(x => x.StoreContext).Return(_storeContext);
            _services.Expect(x => x.WorkContext).Return(_workContext);

            var pluginFinder = PluginFinder.Current;

            _shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();

            //price calculation service
            _discountService = MockRepository.GenerateMock<IDiscountService>();
            _categoryService = MockRepository.GenerateMock<ICategoryService>();
            _manufacturerService = MockRepository.GenerateMock<IManufacturerService>();
            _productAttributeParser = MockRepository.GenerateMock<IProductAttributeParser>();
            _productService = MockRepository.GenerateMock<IProductService>();
            _productAttributeService = MockRepository.GenerateMock<IProductAttributeService>();
            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _paymentService = MockRepository.GenerateMock<IPaymentService>();
            _currencyService = MockRepository.GenerateMock<ICurrencyService>();
            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

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

            _providerManager = MockRepository.GenerateMock<IProviderManager>();
            _checkoutAttributeParser = MockRepository.GenerateMock<ICheckoutAttributeParser>();
            _giftCardService = MockRepository.GenerateMock<IGiftCardService>();

            //tax
            _taxSettings = new TaxSettings
            {
                ShippingPriceIncludesTax = false,
                ShippingIsTaxable = true,
                PaymentMethodAdditionalFeeIsTaxable = true,
                PricesIncludeTax = false,
                TaxDisplayType = TaxDisplayType.IncludingTax,
                DefaultTaxAddressId = 10
            };

            _addressService = MockRepository.GenerateMock<IAddressService>();
            _addressService.Expect(x => x.GetAddressById(_taxSettings.DefaultTaxAddressId)).Return(new Address { Id = _taxSettings.DefaultTaxAddressId });
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
        }

        [Test]
        public void Can_get_shopping_cart_subTotal_excluding_tax()
        {
            //customer
            Customer customer = new Customer();

            //shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 12.34M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci1 = new ShoppingCartItem()
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 21.57M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci2 = new ShoppingCartItem()
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

            decimal discountAmount;
            Discount appliedDiscount;
            decimal subTotalWithoutDiscount;
            decimal subTotalWithDiscount;
            SortedDictionary<decimal, decimal> taxRates;
            //10% - default tax rate
            _orderTotalCalcService.GetShoppingCartSubTotal(cart, false,
                out discountAmount, out appliedDiscount,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
            discountAmount.ShouldEqual(0);
            appliedDiscount.ShouldBeNull();
            subTotalWithoutDiscount.ShouldEqual(89.39);
            subTotalWithDiscount.ShouldEqual(89.39);
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(8.939);
        }

        [Test]
        public void Can_get_shopping_cart_subTotal_including_tax()
        {
            //customer
            Customer customer = new Customer();

            //shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 12.34M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci1 = new ShoppingCartItem()
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 21.57M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci2 = new ShoppingCartItem()
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

            decimal discountAmount;
            Discount appliedDiscount;
            decimal subTotalWithoutDiscount;
            decimal subTotalWithDiscount;
            SortedDictionary<decimal, decimal> taxRates;

            _orderTotalCalcService.GetShoppingCartSubTotal(cart, true,
                out discountAmount, out appliedDiscount,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
            discountAmount.ShouldEqual(0);
            appliedDiscount.ShouldBeNull();
            subTotalWithoutDiscount.ShouldEqual(98.329);
            subTotalWithDiscount.ShouldEqual(98.329);
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(8.939);
        }

        [Test]
        public void Can_get_shopping_cart_subTotal_discount_excluding_tax()
        {
            var customer = new Customer();

            //shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 12.34M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci1 = new ShoppingCartItem
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 21.57M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci2 = new ShoppingCartItem
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            //discounts
            var discount1 = new Discount
            {
                Id = 1,
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToOrderSubTotal,
                DiscountAmount = 3,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            _discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToOrderSubTotal)).Return(new List<Discount> { discount1 });
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

            decimal discountAmount;
            Discount appliedDiscount;
            decimal subTotalWithoutDiscount;
            decimal subTotalWithDiscount;
            SortedDictionary<decimal, decimal> taxRates;
            //10% - default tax rate

            _orderTotalCalcService.GetShoppingCartSubTotal(cart, false,
                out discountAmount, out appliedDiscount,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);

            discountAmount.ShouldEqual(3);
            appliedDiscount.ShouldNotBeNull();
            appliedDiscount.Name.ShouldEqual("Discount 1");
            subTotalWithoutDiscount.ShouldEqual(89.39);
            subTotalWithDiscount.ShouldEqual(86.39);
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(8.639);
        }

        [Test]
        public void Can_get_shopping_cart_subTotal_discount_including_tax()
        {
            var customer = new Customer();

            //shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 12.34M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci1 = new ShoppingCartItem
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 21.57M,
                CustomerEntersPrice = false,
                Published = true,
            };
            var sci2 = new ShoppingCartItem
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            //discounts
            var discount1 = new Discount
            {
                Id = 1,
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToOrderSubTotal,
                DiscountAmount = 3,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            _discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToOrderSubTotal)).Return(new List<Discount> { discount1 });
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

            decimal discountAmount;
            Discount appliedDiscount;
            decimal subTotalWithoutDiscount;
            decimal subTotalWithDiscount;
            SortedDictionary<decimal, decimal> taxRates;

            _orderTotalCalcService.GetShoppingCartSubTotal(cart, true,
                out discountAmount, out appliedDiscount,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);

            (3.3M == Math.Round(discountAmount, 8)).ShouldBeTrue();
            appliedDiscount.ShouldNotBeNull();
            appliedDiscount.Name.ShouldEqual("Discount 1");
            subTotalWithoutDiscount.ShouldEqual(98.329);
            subTotalWithDiscount.ShouldEqual(95.029);
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(8.639);
        }



        [Test]
        public void Can_get_shoppingCartItem_additional_shippingCharge()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    AdditionalShippingCharge = 5.5M,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 6.5M,
                    IsShipEnabled = true,
                }
            };

            //sci3 is not shippable
            var sci3 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 5,
                Product = new Product()
                {
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 7.5M,
                    IsShipEnabled = false,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));
            cart.Add(new OrganizedShoppingCartItem(sci3));

            _orderTotalCalcService.GetShoppingCartAdditionalShippingCharge(cart).ShouldEqual(42.5M);
        }

        [Test]
        public void Shipping_should_be_free_when_all_shoppingCartItems_are_marked_as_freeShipping()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    IsFreeShipping = true,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    IsFreeShipping = true,
                    IsShipEnabled = true,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            _orderTotalCalcService.IsFreeShipping(cart).ShouldEqual(true);
        }

        [Test]
        public void Shipping_should_not_be_free_when_some_of_shoppingCartItems_are_not_marked_as_freeShipping()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    IsFreeShipping = true,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    IsFreeShipping = false,
                    IsShipEnabled = true,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            _orderTotalCalcService.IsFreeShipping(cart).ShouldEqual(false);
        }

        [Test]
        public void Shipping_should_be_free_when_customer_is_in_role_with_free_shipping()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    IsFreeShipping = false,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    IsFreeShipping = false,
                    IsShipEnabled = true,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            var customer = new Customer();
            var customerRole1 = new CustomerRole
            {
                Active = true,
                FreeShipping = true,
            };
            var customerRole2 = new CustomerRole
            {
                Active = true,
                FreeShipping = false,
            };

            customer.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRole = customerRole1
            });
            customer.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRole = customerRole2
            });

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            _orderTotalCalcService.IsFreeShipping(cart).ShouldEqual(true);
        }

        [Test]
        public void Can_get_shipping_total_with_fixed_shipping_rate_excluding_tax()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Id = 1,
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    AdditionalShippingCharge = 5.5M,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Id = 2,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 6.5M,
                    IsShipEnabled = true,
                }
            };

            //sci3 is not shippable
            var sci3 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 5,
                Product = new Product()
                {
                    Id = 3,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 7.5M,
                    IsShipEnabled = false,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));
            cart.Add(new OrganizedShoppingCartItem(sci3));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            decimal taxRate = decimal.Zero;
            Discount appliedDiscount = null;
            decimal? shipping = null;


            shipping = _orderTotalCalcService.GetShoppingCartShippingTotal(cart, false, out taxRate, out appliedDiscount);
            shipping.ShouldNotBeNull();
            //10 - default fixed shipping rate, 42.5 - additional shipping change
            shipping.ShouldEqual(52.5);
            appliedDiscount.ShouldBeNull();
            //10 - default fixed tax rate
            taxRate.ShouldEqual(10);
        }

        [Test]
        public void Can_get_shipping_total_with_fixed_shipping_rate_including_tax()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Id = 1,
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    AdditionalShippingCharge = 5.5M,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Id = 2,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 6.5M,
                    IsShipEnabled = true,
                }
            };

            //sci3 is not shippable
            var sci3 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 5,
                Product = new Product()
                {
                    Id = 3,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 7.5M,
                    IsShipEnabled = false,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));
            cart.Add(new OrganizedShoppingCartItem(sci3));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            decimal taxRate = decimal.Zero;
            Discount appliedDiscount = null;
            decimal? shipping = null;

            shipping = _orderTotalCalcService.GetShoppingCartShippingTotal(cart, true, out taxRate, out appliedDiscount);
            shipping.ShouldNotBeNull();
            //10 - default fixed shipping rate, 42.5 - additional shipping change
            shipping.ShouldEqual(57.75);
            appliedDiscount.ShouldBeNull();
            //10 - default fixed tax rate
            taxRate.ShouldEqual(10);
        }

        [Test]
        public void Can_get_shipping_total_discount_excluding_tax()
        {
            var sci1 = new ShoppingCartItem
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product
                {
                    Id = 1,
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    AdditionalShippingCharge = 5.5M,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product
                {
                    Id = 2,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 6.5M,
                    IsShipEnabled = true,
                }
            };

            //sci3 is not shippable
            var sci3 = new ShoppingCartItem
            {
                AttributesXml = "",
                Quantity = 5,
                Product = new Product
                {
                    Id = 3,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 7.5M,
                    IsShipEnabled = false,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));
            cart.Add(new OrganizedShoppingCartItem(sci3));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            //discounts
            var discount1 = new Discount
            {
                Id = 1,
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToShipping,
                DiscountAmount = 3,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            _discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToShipping)).Return(new List<Discount>() { discount1 });

            var shipping = _orderTotalCalcService.GetShoppingCartShippingTotal(cart, false, out var taxRate, out var appliedDiscount);
            appliedDiscount.ShouldNotBeNull();
            appliedDiscount.Name.ShouldEqual("Discount 1");
            shipping.ShouldNotBeNull();
            //10 - default fixed shipping rate, 42.5 - additional shipping change, -3 - discount
            shipping.ShouldEqual(49.5);
            //10 - default fixed tax rate
            taxRate.ShouldEqual(10);
        }

        [Test]
        public void Can_get_shipping_total_discount_including_tax()
        {
            var sci1 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
                Product = new Product()
                {
                    Id = 1,
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M,
                    AdditionalShippingCharge = 5.5M,
                    IsShipEnabled = true,
                }
            };
            var sci2 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 4,
                Product = new Product()
                {
                    Id = 2,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 6.5M,
                    IsShipEnabled = true,
                }
            };

            //sci3 is not shippable
            var sci3 = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 5,
                Product = new Product()
                {
                    Id = 3,
                    Weight = 11.5M,
                    Height = 12.5M,
                    Length = 13.5M,
                    Width = 14.5M,
                    AdditionalShippingCharge = 7.5M,
                    IsShipEnabled = false,
                }
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));
            cart.Add(new OrganizedShoppingCartItem(sci3));

            var customer = new Customer();
            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            //discounts
            var discount1 = new Discount()
            {
                Id = 1,
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToShipping,
                DiscountAmount = 3,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            _discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToShipping)).Return(new List<Discount>() { discount1 });


            decimal taxRate = decimal.Zero;
            Discount appliedDiscount = null;
            decimal? shipping = null;

            shipping = _orderTotalCalcService.GetShoppingCartShippingTotal(cart, true, out taxRate, out appliedDiscount);
            appliedDiscount.ShouldNotBeNull();
            appliedDiscount.Name.ShouldEqual("Discount 1");
            shipping.ShouldNotBeNull();
            //10 - default fixed shipping rate, 42.5 - additional shipping change, -3 - discount
            shipping.ShouldEqual(54.45);
            //10 - default fixed tax rate
            taxRate.ShouldEqual(10);
        }

        [Test]
        public void Can_get_tax_total()
        {
            //customer
            var customer = new Customer
            {
                Id = 10,
            };

            //shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 10M,
                Published = true,
                IsShipEnabled = true,
            };
            var sci1 = new ShoppingCartItem()
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 12M,
                Published = true,
                IsShipEnabled = true,
            };
            var sci2 = new ShoppingCartItem()
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);


            //_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
            //	.Return(new List<GenericAttribute>()
            //				{
            //					new GenericAttribute()
            //						{
            //							StoreId = _store.Id,
            //							EntityId = customer.Id,
            //							Key = SystemCustomerAttributeNames.SelectedPaymentMethod,
            //							KeyGroup = "Customer",
            //							Value = "test1"
            //						}
            //				});
            //_paymentService.Expect(ps => ps.GetAdditionalHandlingFee(cart, "test1")).Return(20);

            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

            //56 - items, 10 - shipping (fixed), 20 - payment fee = 86
            //56 - items, 10 - shipping (fixed) = 66

            //1. shipping is taxable, payment fee is taxable
            _taxSettings.ShippingIsTaxable = true;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

            _orderTotalCalcService.GetTaxTotal(cart, out SortedDictionary<decimal, decimal> taxRates).ShouldEqual(6.6m);

            taxRates.ShouldNotBeNull();
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(6.6);

            //2. shipping is taxable, payment fee is not taxable
            _taxSettings.ShippingIsTaxable = true;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = false;

            _orderTotalCalcService.GetTaxTotal(cart, out taxRates).ShouldEqual(6.6);

            taxRates.ShouldNotBeNull();
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(6.6);

            //3. shipping is not taxable, payment fee is taxable
            _taxSettings.ShippingIsTaxable = false;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

            _orderTotalCalcService.GetTaxTotal(cart, out taxRates).ShouldEqual(5.6);

            taxRates.ShouldNotBeNull();
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(5.6);

            //3. shipping is not taxable, payment fee is not taxable
            _taxSettings.ShippingIsTaxable = false;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = false;

            _orderTotalCalcService.GetTaxTotal(cart, out taxRates).ShouldEqual(5.6);

            taxRates.ShouldNotBeNull();
            taxRates.Count.ShouldEqual(1);
            taxRates.ContainsKey(10).ShouldBeTrue();
            taxRates[10].ShouldEqual(5.6);
        }

        //[Test]
        //public void Can_get_shopping_cart_total_without_shipping_required()
        //{
        //	//customer
        //	var customer = new Customer()
        //	{
        //		Id = 10,
        //	};

        //	//shopping cart
        //	var product1 = new Product
        //	{
        //		Id = 1,
        //		Name = "Product name 1",
        //		Price = 10M,
        //		Published = true,
        //		IsShipEnabled = false,
        //	};
        //	var sci1 = new ShoppingCartItem()
        //	{
        //		Product = product1,
        //		ProductId = product1.Id,
        //		Quantity = 2,
        //	};
        //	var product2 = new Product
        //	{
        //		Id = 2,
        //		Name = "Product name 2",
        //		Price = 12M,
        //		Published = true,
        //		IsShipEnabled = false,
        //	};
        //	var sci2 = new ShoppingCartItem()
        //	{
        //		Product = product2,
        //		ProductId = product2.Id,
        //		Quantity = 3
        //	};

        //	var cart = new List<ShoppingCartItem>() { sci1, sci2 };
        //	cart.ForEach(sci => sci.Customer = customer);
        //	cart.ForEach(sci => sci.CustomerId = customer.Id);



        //	_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
        //		.Return(new List<GenericAttribute>()
        //					{
        //						new GenericAttribute()
        //							{
        //								StoreId = _store.Id,
        //								EntityId = customer.Id,
        //								Key = SystemCustomerAttributeNames.SelectedPaymentMethod,
        //								KeyGroup = "Customer",
        //								Value = "test1"
        //							}
        //					});
        //	_paymentService.Expect(ps => ps.GetAdditionalHandlingFee(cart, "test1")).Return(20);

        //	_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());

        //	decimal discountAmount;
        //	Discount appliedDiscount;
        //	List<AppliedGiftCard> appliedGiftCards;
        //	int redeemedRewardPoints;
        //	decimal redeemedRewardPointsAmount;


        //	//shipping is taxable, payment fee is taxable
        //	_taxSettings.ShippingIsTaxable = true;
        //	_taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

        //	//56 - items, 20 - payment fee, 7.6 - tax
        //	_orderTotalCalcService.GetShoppingCartTotal(cart,  out discountAmount, out appliedDiscount, 
        //		out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount)
        //		.ShouldEqual(83.6M);
        //}

        //[Test]
        //public void Can_get_shopping_cart_total_with_shipping_required()
        //{
        //	//customer
        //	var customer = new Customer()
        //	{
        //		Id = 10,
        //	};

        //	//shopping cart
        //	var product1 = new Product
        //	{
        //		Id = 1,
        //		Name = "Product name 1",
        //		Price = 10M,
        //		Published = true,
        //		IsShipEnabled = true,
        //	};
        //	var sci1 = new ShoppingCartItem()
        //	{
        //		Product = product1,
        //		ProductId = product1.Id,
        //		Quantity = 2,
        //	};
        //	var product2 = new Product
        //	{
        //		Id = 2,
        //		Name = "Product name 2",
        //		Price = 12M,
        //		Published = true,
        //		IsShipEnabled = true,
        //	};
        //	var sci2 = new ShoppingCartItem()
        //	{
        //		Product = product2,
        //		ProductId = product2.Id,
        //		Quantity = 3
        //	};

        //	var cart = new List<ShoppingCartItem>() { sci1, sci2 };
        //	cart.ForEach(sci => sci.Customer = customer);
        //	cart.ForEach(sci => sci.CustomerId = customer.Id);

        //	_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
        //		.Return(new List<GenericAttribute>()
        //					{
        //						new GenericAttribute()
        //							{
        //								StoreId = _store.Id,
        //								EntityId = customer.Id,
        //								Key = SystemCustomerAttributeNames.SelectedPaymentMethod,
        //								KeyGroup = "Customer",
        //								Value = "test1"
        //							}
        //					});
        //	_paymentService.Expect(ps => ps.GetAdditionalHandlingFee(cart, "test1")).Return(20);

        //	_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());

        //	decimal discountAmount;
        //	Discount appliedDiscount;
        //	List<AppliedGiftCard> appliedGiftCards;
        //	int redeemedRewardPoints;
        //	decimal redeemedRewardPointsAmount;


        //	//shipping is taxable, payment fee is taxable
        //	_taxSettings.ShippingIsTaxable = true;
        //	_taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

        //	//56 - items, 10 - shipping (fixed), 20 - payment fee, 8.6 - tax
        //	_orderTotalCalcService.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscount,
        //		out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount)
        //		.ShouldEqual(94.6M);
        //}

        //[Test]
        //public void Can_get_shopping_cart_total_with_applied_reward_points()
        //{
        //   //customer
        //	var customer = new Customer()
        //	{
        //		Id = 10,
        //	};

        //	//shopping cart
        //	var product1 = new Product
        //	{
        //		Id = 1,
        //		Name = "Product name 1",
        //		Price = 10M,
        //		Published = true,
        //		IsShipEnabled = true,
        //	};
        //	var sci1 = new ShoppingCartItem()
        //	{
        //		Product = product1,
        //		ProductId = product1.Id,
        //		Quantity = 2,
        //	};
        //	var product2 = new Product
        //	{
        //		Id = 2,
        //		Name = "Product name 2",
        //		Price = 12M,
        //		Published = true,
        //		IsShipEnabled = true,
        //	};
        //	var sci2 = new ShoppingCartItem()
        //	{
        //		Product = product2,
        //		ProductId = product2.Id,
        //		Quantity = 3
        //	};

        //	var cart = new List<ShoppingCartItem>() { sci1, sci2 };
        //	cart.ForEach(sci => sci.Customer = customer);
        //	cart.ForEach(sci => sci.CustomerId = customer.Id);



        //	_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
        //		.Return(new List<GenericAttribute>()
        //					{
        //						new GenericAttribute()
        //							{
        //								StoreId = _store.Id,
        //								EntityId = customer.Id,
        //								Key = SystemCustomerAttributeNames.SelectedPaymentMethod,
        //								KeyGroup = "Customer",
        //								Value = "test1"
        //							},
        //						new GenericAttribute()
        //								{
        //								StoreId = 1,
        //								EntityId = customer.Id,
        //								Key = SystemCustomerAttributeNames.UseRewardPointsDuringCheckout,
        //								KeyGroup = "Customer",
        //								Value = true.ToString()
        //								}
        //					});
        //	_paymentService.Expect(ps => ps.GetAdditionalHandlingFee(cart, "test1")).Return(20);


        //	_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());

        //	decimal discountAmount;
        //	Discount appliedDiscount;
        //	List<AppliedGiftCard> appliedGiftCards;
        //	int redeemedRewardPoints;
        //	decimal redeemedRewardPointsAmount;


        //	//shipping is taxable, payment fee is taxable
        //	_taxSettings.ShippingIsTaxable = true;
        //	_taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

        //	//reward points
        //	_rewardPointsSettings.Enabled = true;
        //	_rewardPointsSettings.ExchangeRate = 2; //1 reward point = 2
        //	customer.AddRewardPointsHistoryEntry(15); //15*2=30

        //	//56 - items, 10 - shipping (fixed), 20 - payment fee, 8.6 - tax, -30 (reward points)
        //	_orderTotalCalcService.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscount,
        //		out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount)
        //		.ShouldEqual(64.6M);
        //}

        [Test]
        public void Can_get_shopping_cart_total()
        {
            var customer = new Customer
            {
                Id = 10,
            };

            // Shopping cart
            var product1 = new Product
            {
                Id = 1,
                Name = "Product name 1",
                Price = 10M,
                Published = true,
                IsShipEnabled = true,
            };
            var sci1 = new ShoppingCartItem
            {
                Product = product1,
                ProductId = product1.Id,
                Quantity = 2,
            };

            var product2 = new Product
            {
                Id = 2,
                Name = "Product name 2",
                Price = 12M,
                Published = true,
                IsShipEnabled = true,
            };
            var sci2 = new ShoppingCartItem
            {
                Product = product2,
                ProductId = product2.Id,
                Quantity = 3
            };

            var cart = new List<OrganizedShoppingCartItem>();
            cart.Add(new OrganizedShoppingCartItem(sci1));
            cart.Add(new OrganizedShoppingCartItem(sci2));

            cart.ForEach(sci => sci.Item.Customer = customer);
            cart.ForEach(sci => sci.Item.CustomerId = customer.Id);

            // Discounts
            var discount1 = new Discount
            {
                Id = 1,
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToOrderTotal,
                DiscountAmount = 3,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            _discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToOrderTotal)).Return(new List<Discount>() { discount1 });
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
            _discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());


            //_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
            //	.Return(new List<GenericAttribute>
            //	{
            //		new GenericAttribute
            //		{
            //			StoreId = _store.Id,
            //			EntityId = customer.Id,
            //			Key = SystemCustomerAttributeNames.SelectedPaymentMethod,
            //			KeyGroup = "Customer",
            //			Value = "test1"
            //		}
            //	});

            //_paymentService.Expect(ps => ps.GetAdditionalHandlingFee(cart, "test1")).Return(20);

            // Shipping is taxable, payment fee is taxable
            _taxSettings.ShippingIsTaxable = true;
            _taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;

            // 56 - items, 10 - shipping (fixed), 20 - payment fee, 8.6 - tax, [-3] - discount = 91.6
            // 56 - items, 10 - shipping (fixed), 6.6 - tax, [-3] - discount = 69.6
            var cartTotal = _orderTotalCalcService.GetShoppingCartTotal(cart);
            cartTotal.TotalAmount.ShouldEqual(69.6M);
            cartTotal.DiscountAmount.ShouldEqual(3);
            cartTotal.AppliedDiscount.ShouldNotBeNull();
            cartTotal.AppliedDiscount.Name.ShouldEqual("Discount 1");

            // Test implicit operators
            decimal? totalAmount = null;
            totalAmount = _orderTotalCalcService.GetShoppingCartTotal(cart);
            totalAmount.ShouldEqual(69.6M);

            ShoppingCartTotal cartTotalObject = 123.45M;
            cartTotalObject.TotalAmount.ShouldEqual(123.45M);
        }

        [Test]
        public void Can_convert_reward_points_to_amount()
        {
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 15M;

            _orderTotalCalcService.ConvertRewardPointsToAmount(100).ShouldEqual(1500);
        }

        [Test]
        public void Can_convert_amount_to_reward_points()
        {
            _rewardPointsSettings.Enabled = true;
            _rewardPointsSettings.ExchangeRate = 15M;

            //we calculate ceiling for reward points
            _orderTotalCalcService.ConvertAmountToRewardPoints(100).ShouldEqual(7);
        }
    }
}
