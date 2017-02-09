using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Orders;
using SmartStore.Services.Shipping;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Shipping
{
	[TestFixture]
    public class ShippingServiceTests : ServiceTest
    {
        IRepository<ShippingMethod> _shippingMethodRepository;
        IProductAttributeParser _productAttributeParser;
		IProductService _productService;
        ICheckoutAttributeParser _checkoutAttributeParser;
        ShippingSettings _shippingSettings;
        IEventPublisher _eventPublisher;
		IGenericAttributeService _genericAttributeService;
        IShippingService _shippingService;
        ShoppingCartSettings _shoppingCartSettings;
		ISettingService _settingService;
		ITypeFinder _typeFinder;

        [SetUp]
        public new void SetUp()
        {
            _shippingSettings = new ShippingSettings();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames = new List<string>();
            _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add("FixedRateTestShippingRateComputationMethod");

            _shippingMethodRepository = MockRepository.GenerateMock<IRepository<ShippingMethod>>();
            _productAttributeParser = MockRepository.GenerateMock<IProductAttributeParser>();
			_productService = MockRepository.GenerateMock<IProductService>();
            _checkoutAttributeParser = MockRepository.GenerateMock<ICheckoutAttributeParser>();

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

			_genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
			_settingService = MockRepository.GenerateMock<ISettingService>();
			_typeFinder = MockRepository.GenerateMock<ITypeFinder>();

            _shoppingCartSettings = new ShoppingCartSettings();
            _shippingService = new ShippingService(
                _shippingMethodRepository, 
                _productAttributeParser,
				_productService,
                _checkoutAttributeParser,
				_genericAttributeService,
                _shippingSettings,
				_eventPublisher,
                _shoppingCartSettings,
				_settingService, 
				this.ProviderManager,
				_typeFinder);
        }

        [Test]
        public void Can_load_shippingRateComputationMethods()
        {
            var srcm = _shippingService.LoadAllShippingRateComputationMethods();
            srcm.ShouldNotBeNull();
            (srcm.Count() > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_load_shippingRateComputationMethod_by_systemKeyword()
        {
            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName("FixedRateTestShippingRateComputationMethod");
            srcm.Value.ShouldNotBeNull();
        }

        [Test]
        public void Can_load_active_shippingRateComputationMethods()
        {
            var srcm = _shippingService.LoadActiveShippingRateComputationMethods();
            srcm.ShouldNotBeNull();
            (srcm.Count() > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_get_shoppingCartItem_totalWeight_without_attributes()
        {
            var sci = new ShoppingCartItem()
            {
                AttributesXml = "",
                Quantity = 3,
				Product = new Product()
                {
                    Weight = 1.5M,
                    Height = 2.5M,
                    Length = 3.5M,
                    Width = 4.5M
                }
            };

			var item = new OrganizedShoppingCartItem(sci);

            _shippingService.GetShoppingCartItemTotalWeight(item).ShouldEqual(4.5M);
        }

        [Test]
        public void Can_get_shoppingCart_totalWeight_without_attributes()
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
                    Width = 4.5M
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
                    Width = 14.5M
                }
            };

			var cart = new List<OrganizedShoppingCartItem>();
			cart.Add(new OrganizedShoppingCartItem(sci1));
			cart.Add(new OrganizedShoppingCartItem(sci2));

            _shippingService.GetShoppingCartTotalWeight(cart).ShouldEqual(50.5M);
        }
    }
}
