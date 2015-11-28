using System;
using System.Collections.Generic;
using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Catalog;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Tests.ExportImport
{
    [TestFixture]
    public class ExportManagerTests : ServiceTest
    {
        ICategoryService _categoryService;
        IManufacturerService _manufacturerService;
        IProductService _productService;
		IProductTemplateService _productTemplateService;
        IPictureService _pictureService;
        INewsLetterSubscriptionService _newsLetterSubscriptionService;
        IExportManager _exportManager;
        ILanguageService _languageService;
		MediaSettings _mediaSettings;
		ICommonServices _services;
        IStoreMappingService _storeMapping;

        [SetUp]
        public new void SetUp()
        {
            _categoryService = MockRepository.GenerateMock<ICategoryService>();
            _manufacturerService = MockRepository.GenerateMock<IManufacturerService>();
            _productService = MockRepository.GenerateMock<IProductService>();
			_productTemplateService = MockRepository.GenerateMock<IProductTemplateService>();
            _pictureService = MockRepository.GenerateMock<IPictureService>();
            _newsLetterSubscriptionService = MockRepository.GenerateMock<INewsLetterSubscriptionService>();
            _languageService = MockRepository.GenerateMock<ILanguageService>();
			_mediaSettings = MockRepository.GenerateMock<MediaSettings>();
			_services = MockRepository.GenerateMock<ICommonServices>();
            _storeMapping = MockRepository.GenerateMock<IStoreMappingService>();

            _exportManager = new ExportManager(_categoryService, _manufacturerService, _productService, _productTemplateService, _pictureService,
                _newsLetterSubscriptionService, _languageService, _mediaSettings, _services, _storeMapping);
        }

		//[Test]
		//public void Can_export_manufacturers_to_xml()
		//{
		//    var manufacturers = new List<Manufacturer>()
		//    {
		//        new Manufacturer()
		//        {
		//            Id = 1,
		//            Name = "Name",
		//            Description = "Description 1",
		//            MetaKeywords = "Meta keywords",
		//            MetaDescription = "Meta description",
		//            MetaTitle = "Meta title",
		//            PictureId = 0,
		//            PageSize = 4,
		//            PriceRanges = "1-3;",
		//            Published = true,
		//            Deleted = false,
		//            DisplayOrder = 5,
		//            CreatedOnUtc = new DateTime(2010, 01, 01),
		//            UpdatedOnUtc = new DateTime(2010, 01, 02),
		//        },
		//        new Manufacturer()
		//        {
		//            Id = 2,
		//            Name = "Name 2",
		//            Description = "Description 2",
		//            MetaKeywords = "Meta keywords",
		//            MetaDescription = "Meta description",
		//            MetaTitle = "Meta title",
		//            PictureId = 0,
		//            PageSize = 4,
		//            PriceRanges = "1-3;",
		//            Published = true,
		//            Deleted = false,
		//            DisplayOrder = 5,
		//            CreatedOnUtc = new DateTime(2010, 01, 01),
		//            UpdatedOnUtc = new DateTime(2010, 01, 02),
		//        }
		//    };

		//    string result = _exportManager.ExportManufacturersToXml(manufacturers);
		//    //TODO test it
		//    String.IsNullOrEmpty(result).ShouldBeFalse();
		//}

		[Test]
		public void Can_export_orders_xlsx()
		{
			var orders = new List<Order>()
			{
				new Order()
				{
					OrderGuid = Guid.NewGuid(),
					Customer = GetTestCustomer(),
					OrderStatus = OrderStatus.Complete,
					ShippingStatus = ShippingStatus.Shipped,
					PaymentStatus = PaymentStatus.Paid,
					PaymentMethodSystemName = "PaymentMethodSystemName1",
					CustomerCurrencyCode = "RUR",
					CurrencyRate = 1.1M,
					CustomerTaxDisplayType = TaxDisplayType.ExcludingTax,
					VatNumber = "123456789",
					OrderSubtotalInclTax = 2.1M,
					OrderSubtotalExclTax = 3.1M,
					OrderSubTotalDiscountInclTax = 4.1M,
					OrderSubTotalDiscountExclTax = 5.1M,
					OrderShippingInclTax = 6.1M,
					OrderShippingExclTax = 7.1M,
					OrderShippingTaxRate = 19.0M,
					PaymentMethodAdditionalFeeInclTax = 8.1M,
					PaymentMethodAdditionalFeeExclTax = 9.1M,
					PaymentMethodAdditionalFeeTaxRate = 19.0M,
					TaxRates = "1,3,5,7",
					OrderTax = 10.1M,
					OrderDiscount = 11.1M,
					OrderTotal = 12.1M,
					RefundedAmount  = 13.1M,
					CheckoutAttributeDescription = "CheckoutAttributeDescription1",
					CheckoutAttributesXml = "CheckoutAttributesXml1",
					CustomerLanguageId = 14,
					AffiliateId= 15,
					CustomerIp="CustomerIp1",
					AllowStoringCreditCardNumber= true,
					CardType= "Visa",
					CardName = "John Smith",
					CardNumber = "4111111111111111",
					MaskedCreditCardNumber= "************1111",
					CardCvv2= "123",
					CardExpirationMonth= "12",
					CardExpirationYear = "2010",
					AuthorizationTransactionId = "AuthorizationTransactionId1",
					AuthorizationTransactionCode="AuthorizationTransactionCode1",
					AuthorizationTransactionResult="AuthorizationTransactionResult1",
					CaptureTransactionId= "CaptureTransactionId1",
					CaptureTransactionResult = "CaptureTransactionResult1",
					SubscriptionTransactionId = "SubscriptionTransactionId1",
					PurchaseOrderNumber= "PurchaseOrderNumber1",
					PaidDateUtc= new DateTime(2010, 01, 01),
					BillingAddress = GetTestBillingAddress(),
					ShippingAddress = GetTestShippingAddress(),
					ShippingMethod = "ShippingMethod1",
					ShippingRateComputationMethodSystemName="ShippingRateComputationMethodSystemName1",
					Deleted = false,
					CreatedOnUtc = new DateTime(2010, 01, 04)
			}
		};
		string fileName = Path.GetTempFileName();
		//TODO uncomment
		//_exportManager.ExportOrdersToXlsx(fileName, orders);
		}

		protected Address GetTestBillingAddress()
		{
			return new Address()
			{
				FirstName = "FirstName 1",
				LastName = "LastName 1",
				Email = "Email 1",
				Company = "Company 1",
				City = "City 1",
				Address1 = "Address1a",
				Address2 = "Address1a",
				ZipPostalCode = "ZipPostalCode 1",
				PhoneNumber = "PhoneNumber 1",
				FaxNumber = "FaxNumber 1",
				CreatedOnUtc = new DateTime(2010, 01, 01),
				Country = GetTestCountry()
			};
		}

		protected Address GetTestShippingAddress()
		{
			return new Address()
			{
				FirstName = "FirstName 2",
				LastName = "LastName 2",
				Email = "Email 2",
				Company = "Company 2",
				City = "City 2",
				Address1 = "Address2a",
				Address2 = "Address2b",
				ZipPostalCode = "ZipPostalCode 2",
				PhoneNumber = "PhoneNumber 2",
				FaxNumber = "FaxNumber 2",
				CreatedOnUtc = new DateTime(2010, 01, 01),
				Country = GetTestCountry()
			};
		}

		protected Country GetTestCountry()
		{
			return new Country
			{
				Name = "United States",
				AllowsBilling = true,
				AllowsShipping = true,
				TwoLetterIsoCode = "US",
				ThreeLetterIsoCode = "USA",
				NumericIsoCode = 1,
				SubjectToVat = true,
				Published = true,
				DisplayOrder = 1
			};
		}

		protected Customer GetTestCustomer()
		{
			return new Customer
			{
				CustomerGuid = Guid.NewGuid(),
				AdminComment = "some comment here",
				Active = true,
				Deleted = false,
				CreatedOnUtc = new DateTime(2010, 01, 01)
			};
		}
	}
}
