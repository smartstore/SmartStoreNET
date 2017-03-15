﻿using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Discounts;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Media;
using System.Web;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Tests.Catalog
{
    [TestFixture]
    public class PriceCalculationServiceTests : ServiceTest
    {
		IStoreContext _storeContext;
        IDiscountService _discountService;
        ICategoryService _categoryService;
		IManufacturerService _manufacturerService;
		IProductAttributeParser _productAttributeParser;
		IProductService _productService;
		IProductAttributeService _productAttributeService;
        IPriceCalculationService _priceCalcService;
		IDownloadService _downloadService;
		ICommonServices _services;
		HttpRequestBase _httpRequestBase;
		ITaxService _taxService;
        ShoppingCartSettings _shoppingCartSettings;
        CatalogSettings _catalogSettings;

		Store _store;

        [SetUp]
        public new void SetUp()
        {
			_store = new Store() { Id = 1 };
			_storeContext = MockRepository.GenerateMock<IStoreContext>();
			_storeContext.Expect(x => x.CurrentStore).Return(_store);

            _discountService = MockRepository.GenerateMock<IDiscountService>();

            _categoryService = MockRepository.GenerateMock<ICategoryService>();
			_manufacturerService = MockRepository.GenerateMock<IManufacturerService>();

            _productAttributeParser = MockRepository.GenerateMock<IProductAttributeParser>();
			_productService = MockRepository.GenerateMock<IProductService>();
			_productAttributeService = MockRepository.GenerateMock<IProductAttributeService>();

			_downloadService = MockRepository.GenerateMock<IDownloadService>();
			_services = MockRepository.GenerateMock<ICommonServices>();
			_services.Expect(x => x.StoreContext).Return(_storeContext);
			_httpRequestBase = MockRepository.GenerateMock<HttpRequestBase>();
			_taxService = MockRepository.GenerateMock<ITaxService>();

			_shoppingCartSettings = new ShoppingCartSettings();
            _catalogSettings = new CatalogSettings();

			_priceCalcService = new PriceCalculationService(_discountService, _categoryService, _manufacturerService, _productAttributeParser, _productService,
				_shoppingCartSettings, _catalogSettings, _productAttributeService, _downloadService, _services, _httpRequestBase, _taxService);
        }

        [Test]
        public void Can_get_final_product_price()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

			//customer
			Customer customer = null;

			_priceCalcService.GetFinalPrice(product, customer, 0, false, 1).ShouldEqual(12.34M);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 2).ShouldEqual(12.34M);
        }

        [Test]
        public void Can_get_final_product_price_with_tier_prices()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

			//add tier prices
			product.TierPrices.Add(new TierPrice
			{
				Price = 10,
				Quantity = 2,
				Product = product
			});
			product.TierPrices.Add(new TierPrice
			{
				Price = 8,
				Quantity = 5,
				Product = product
			});

			// set HasTierPrices property
			product.HasTierPrices = true;

			// customer
			Customer customer = null;

			_priceCalcService.GetFinalPrice(product, customer, 0, false, 1).ShouldEqual(12.34M);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 2).ShouldEqual(10);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 3).ShouldEqual(10);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 5).ShouldEqual(8);
        }

        [Test]
        public void Can_get_final_product_price_with_tier_prices_by_customerRole()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

            //customer roles
            var customerRole1 = new CustomerRole()
            {
                Id = 1,
                Name = "Some role 1",
                Active = true,
            };
            var customerRole2 = new CustomerRole()
            {
                Id = 2,
                Name = "Some role 2",
                Active = true,
            };

			//add tier prices
			product.TierPrices.Add(new TierPrice()
			{
				Price = 10,
				Quantity = 2,
				Product = product,
				CustomerRole = customerRole1
			});
			product.TierPrices.Add(new TierPrice()
			{
				Price = 9,
				Quantity = 2,
				Product = product,
				CustomerRole = customerRole2
			});
			product.TierPrices.Add(new TierPrice()
			{
				Price = 8,
				Quantity = 5,
				Product = product,
				CustomerRole = customerRole1
			});
			product.TierPrices.Add(new TierPrice()
			{
				Price = 5,
				Quantity = 10,
				Product = product,
				CustomerRole = customerRole2
			});
			//set HasTierPrices property
			product.HasTierPrices = true;

			//customer
			Customer customer = new Customer();
			customer.CustomerRoles.Add(customerRole1);

			_priceCalcService.GetFinalPrice(product, customer, 0, false, 1).ShouldEqual(12.34M);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 2).ShouldEqual(10);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 3).ShouldEqual(10);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 5).ShouldEqual(8);
			_priceCalcService.GetFinalPrice(product, customer, 0, false, 10).ShouldEqual(8);
        }

        [Test]
        public void Can_get_final_product_price_with_additionalFee()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

			//customer
			Customer customer = null;

			_priceCalcService.GetFinalPrice(product, customer, 5, false, 1).ShouldEqual(17.34M);
        }

        [Test]
        public void Can_get_final_product_price_with_discount()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

			//customer
			Customer customer = null;

			//discounts
			var discount1 = new Discount()
			{
				Id = 1,
				Name = "Discount 1",
				DiscountType = DiscountType.AssignedToSkus,
				DiscountAmount = 3,
				DiscountLimitation = DiscountLimitationType.Unlimited
			};
			discount1.AppliedToProducts.Add(product);
			product.AppliedDiscounts.Add(discount1);
			//set HasDiscountsApplied property
			product.HasDiscountsApplied = true;
			_discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

			_priceCalcService.GetFinalPrice(product, customer, 0, true, 1).ShouldEqual(9.34M);
        }

        [Test]
        public void Can_get_final_product_price_with_special_price()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				SpecialPrice = 10.01M,
				SpecialPriceStartDateTimeUtc = DateTime.UtcNow.AddDays(-1),
				SpecialPriceEndDateTimeUtc = DateTime.UtcNow.AddDays(1),
				CustomerEntersPrice = false,
				Published = true,
			};

			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

			//customer
			Customer customer = null;
			//valid dates
			_priceCalcService.GetFinalPrice(product, customer, 0, true, 1).ShouldEqual(10.01M);

			//invalid date
			product.SpecialPriceStartDateTimeUtc = DateTime.UtcNow.AddDays(1);
			_priceCalcService.GetFinalPrice(product, customer, 0, true, 1).ShouldEqual(12.34M);

			//no dates
			product.SpecialPriceStartDateTimeUtc = null;
			product.SpecialPriceEndDateTimeUtc = null;
			_priceCalcService.GetFinalPrice(product, customer, 0, true, 1).ShouldEqual(10.01M);
        }

        [Test]
        public void Can_get_product_discount()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = false,
				Published = true,
			};

			//customer
			Customer customer = null;

			//discounts
			var discount1 = new Discount()
			{
				Id = 1,
				Name = "Discount 1",
				DiscountType = DiscountType.AssignedToSkus,
				DiscountAmount = 3,
				DiscountLimitation = DiscountLimitationType.Unlimited
			};
			discount1.AppliedToProducts.Add(product);
			product.AppliedDiscounts.Add(discount1);
			//set HasDiscountsApplied property
			product.HasDiscountsApplied = true;
			_discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);
			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToCategories)).Return(new List<Discount>());
			_discountService.Expect(ds => ds.GetAllDiscounts(DiscountType.AssignedToManufacturers)).Return(new List<Discount>());

			var discount2 = new Discount()
			{
				Id = 2,
				Name = "Discount 2",
				DiscountType = DiscountType.AssignedToSkus,
				DiscountAmount = 4,
				DiscountLimitation = DiscountLimitationType.Unlimited
			};
			discount2.AppliedToProducts.Add(product);
			product.AppliedDiscounts.Add(discount2);
			_discountService.Expect(ds => ds.IsDiscountValid(discount2, customer)).Return(true);

			var discount3 = new Discount()
			{
				Id = 3,
				Name = "Discount 3",
				DiscountType = DiscountType.AssignedToOrderSubTotal,
				DiscountAmount = 5,
				DiscountLimitation = DiscountLimitationType.Unlimited,
				RequiresCouponCode = true,
				CouponCode = "SECRET CODE"
			};
			discount3.AppliedToProducts.Add(product);
			product.AppliedDiscounts.Add(discount3);
			//discount is not valid
			_discountService.Expect(ds => ds.IsDiscountValid(discount3, customer)).Return(false);


			Discount appliedDiscount;
			_priceCalcService.GetDiscountAmount(product, customer, 0, 1, out appliedDiscount).ShouldEqual(4);
			appliedDiscount.ShouldNotBeNull();
			appliedDiscount.ShouldEqual(discount2);
        }

        [Test]
        public void Ensure_discount_is_not_applied_to_products_with_prices_entered_by_customer()
        {
			var product = new Product
			{
				Id = 1,
				Name = "Product name 1",
				Price = 12.34M,
				CustomerEntersPrice = true,
				Published = true,
			};

			//customer
			Customer customer = null;

			//discounts
			var discount1 = new Discount()
			{
				Id = 1,
				Name = "Discount 1",
				DiscountType = DiscountType.AssignedToSkus,
				DiscountAmount = 3,
				DiscountLimitation = DiscountLimitationType.Unlimited
			};
			discount1.AppliedToProducts.Add(product);
			product.AppliedDiscounts.Add(discount1);
			_discountService.Expect(ds => ds.IsDiscountValid(discount1, customer)).Return(true);

			Discount appliedDiscount;
			_priceCalcService.GetDiscountAmount(product, customer, 0, 1, out appliedDiscount).ShouldEqual(0);
			appliedDiscount.ShouldBeNull();
        }
        
        [Test]
        public void Can_get_shopping_cart_item_unitPrice()
        {
			//customer
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
			var sci1 = new ShoppingCartItem()
			{
				Customer = customer,
				CustomerId = customer.Id,
				Product = product1,
				ProductId = product1.Id,
				Quantity = 2,
			};

			var item = new OrganizedShoppingCartItem(sci1);

			_priceCalcService.GetUnitPrice(item, false).ShouldEqual(12.34);
		}

        [Test]
        public void Can_get_shopping_cart_item_subTotal()
        {
			//customer
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
			var sci1 = new ShoppingCartItem()
			{
				Customer = customer,
				CustomerId = customer.Id,
				Product = product1,
				ProductId = product1.Id,
				Quantity = 2,
			};

			var item = new OrganizedShoppingCartItem(sci1);

			_priceCalcService.GetSubTotal(item, false).ShouldEqual(24.68);
		}
    }
}
