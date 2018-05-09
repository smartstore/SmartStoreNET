﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Tax;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Orders
{
	[TestFixture]
    public class CheckoutAttributeParserAndFormatterTests : ServiceTest
    {
        IRepository<CheckoutAttribute> _checkoutAttributeRepo;
        IRepository<CheckoutAttributeValue> _checkoutAttributeValueRepo;
		IRepository<StoreMapping> _storeMappingRepo;
		IEventPublisher _eventPublisher;
        ICheckoutAttributeService _checkoutAttributeService;
        ICheckoutAttributeParser _checkoutAttributeParser;

        IWorkContext _workContext;
        ICurrencyService _currencyService;
        ITaxService _taxService;
        IPriceFormatter _priceFormatter;
        IDownloadService _downloadService;
        IWebHelper _webHelper;
        ICheckoutAttributeFormatter _checkoutAttributeFormatter;

        CheckoutAttribute ca1, ca2, ca3;
        CheckoutAttributeValue cav1_1, cav1_2, cav2_1, cav2_2;
        
        [SetUp]
        public new void SetUp()
        {
            #region Test data

            //color (dropdownlist)
            ca1 = new CheckoutAttribute
            {
                Id = 1,
                Name= "Color",
                TextPrompt = "Select color:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.DropdownList,
                DisplayOrder = 1,
            };
            cav1_1 = new CheckoutAttributeValue
            {
                Id = 11,
                Name = "Green",
                DisplayOrder = 1,
                CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id,
            };
            cav1_2 = new CheckoutAttributeValue
            {
                Id = 12,
                Name = "Red",
                DisplayOrder = 2,
                CheckoutAttribute = ca1,
                CheckoutAttributeId = ca1.Id,
            };
            ca1.CheckoutAttributeValues.Add(cav1_1);
            ca1.CheckoutAttributeValues.Add(cav1_2);

            //custom option (checkboxes)
            ca2 = new CheckoutAttribute
            {
                Id = 2,
                Name = "Custom option",
                TextPrompt = "Select custom option:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.Checkboxes,
                DisplayOrder = 2,
            };
            cav2_1 = new CheckoutAttributeValue
            {
                Id = 21,
                Name = "Option 1",
                DisplayOrder = 1,
                CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id,
            };
            cav2_2 = new CheckoutAttributeValue
            {
                Id = 22,
                Name = "Option 2",
                DisplayOrder = 2,
                CheckoutAttribute = ca2,
                CheckoutAttributeId = ca2.Id,
            };
            ca2.CheckoutAttributeValues.Add(cav2_1);
            ca2.CheckoutAttributeValues.Add(cav2_2);

            //custom text
            ca3 = new CheckoutAttribute
            {
                Id = 3,
                Name = "Custom text",
                TextPrompt = "Enter custom text:",
                IsRequired = true,
                AttributeControlType = AttributeControlType.MultilineTextbox,
                DisplayOrder = 3,
            };


            #endregion
            
            _checkoutAttributeRepo = MockRepository.GenerateMock<IRepository<CheckoutAttribute>>();
            _checkoutAttributeRepo.Expect(x => x.Table).Return(new List<CheckoutAttribute>() { ca1, ca2, ca3 }.AsQueryable());
            _checkoutAttributeRepo.Expect(x => x.GetById(ca1.Id)).Return(ca1);
            _checkoutAttributeRepo.Expect(x => x.GetById(ca2.Id)).Return(ca2);
            _checkoutAttributeRepo.Expect(x => x.GetById(ca3.Id)).Return(ca3);

            _checkoutAttributeValueRepo = MockRepository.GenerateMock<IRepository<CheckoutAttributeValue>>();
            _checkoutAttributeValueRepo.Expect(x => x.Table).Return(new List<CheckoutAttributeValue>() { cav1_1, cav1_2, cav2_1, cav2_2 }.AsQueryable());
            _checkoutAttributeValueRepo.Expect(x => x.GetById(cav1_1.Id)).Return(cav1_1);
            _checkoutAttributeValueRepo.Expect(x => x.GetById(cav1_2.Id)).Return(cav1_2);
            _checkoutAttributeValueRepo.Expect(x => x.GetById(cav2_1.Id)).Return(cav2_1);
            _checkoutAttributeValueRepo.Expect(x => x.GetById(cav2_2.Id)).Return(cav2_2);

			_storeMappingRepo = MockRepository.GenerateMock<IRepository<StoreMapping>>();

            var cacheManager = new NullCache();

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _checkoutAttributeService = new CheckoutAttributeService(
                _checkoutAttributeRepo,
                _checkoutAttributeValueRepo,
				_storeMappingRepo,
                _eventPublisher);

            _checkoutAttributeParser = new CheckoutAttributeParser(_checkoutAttributeService);



            var workingLanguage = new Language();
            _workContext = MockRepository.GenerateMock<IWorkContext>();
            _workContext.Expect(x => x.WorkingLanguage).Return(workingLanguage);
            _currencyService = MockRepository.GenerateMock<ICurrencyService>();
            _taxService = MockRepository.GenerateMock<ITaxService>();
            _priceFormatter = MockRepository.GenerateMock<IPriceFormatter>();
            _downloadService = MockRepository.GenerateMock<IDownloadService>();
            _webHelper = MockRepository.GenerateMock<IWebHelper>();

            _checkoutAttributeFormatter = new CheckoutAttributeFormatter(_workContext,
                _checkoutAttributeService,
                _checkoutAttributeParser,
                _currencyService,
                _taxService,
                _priceFormatter,
                _downloadService,
                _webHelper);
        }
        
        [Test]
        public void Can_add_and_parse_checkoutAttributes()
        {
            string attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "Some custom text goes here");

            var parsed_pvaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(attributes);
            parsed_pvaValues.Contains(cav1_1).ShouldEqual(true);
            parsed_pvaValues.Contains(cav1_2).ShouldEqual(false);
            parsed_pvaValues.Contains(cav2_1).ShouldEqual(true);
            parsed_pvaValues.Contains(cav2_2).ShouldEqual(true);
            parsed_pvaValues.Contains(cav2_2).ShouldEqual(true);

            var parsedValues = _checkoutAttributeParser.ParseValues(attributes, ca3.Id);
            parsedValues.Count.ShouldEqual(1);
            parsedValues.Contains("Some custom text goes here").ShouldEqual(true);
            parsedValues.Contains("Some other custom text").ShouldEqual(false);
        }

        [Test]
        public void Can_add_render_attributes_withoutPrices()
        {
            string attributes = "";
            //color: green
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca1, cav1_1.Id.ToString());
            //custom option: option 1, option 2
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_1.Id.ToString());
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca2, cav2_2.Id.ToString());
            //custom text
            attributes = _checkoutAttributeParser.AddCheckoutAttribute(attributes, ca3, "Some custom text goes here");


            var customer = new Customer();
            string formattedAttributes = _checkoutAttributeFormatter.FormatAttributes(attributes, customer, "<br />", false, false);
            formattedAttributes.ShouldEqual("Color: Green<br />Custom option: Option 1<br />Custom option: Option 2<br />Custom text: Some custom text goes here");
        }
    }
}
