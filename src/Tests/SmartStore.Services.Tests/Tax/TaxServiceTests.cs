using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Tax;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Tax
{
    [TestFixture]
    public class TaxServiceTests : ServiceTest
    {
        IAddressService _addressService;
        IWorkContext _workContext;
        TaxSettings _taxSettings;
        ShoppingCartSettings _cartSettings;
        IEventPublisher _eventPublisher;
        ITaxService _taxService;
        IGeoCountryLookup _geoCountryLookup;

        [SetUp]
        public new void SetUp()
        {
            _taxSettings = new TaxSettings();
            _taxSettings.DefaultTaxAddressId = 10;

            _workContext = null;

            _cartSettings = new ShoppingCartSettings();

            _addressService = MockRepository.GenerateMock<IAddressService>();
            //default tax address
            _addressService.Expect(x => x.GetAddressById(_taxSettings.DefaultTaxAddressId)).Return(new Address() { Id = _taxSettings.DefaultTaxAddressId });

            var pluginFinder = PluginFinder.Current;

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _geoCountryLookup = MockRepository.GenerateMock<IGeoCountryLookup>();

            _taxService = new TaxService(_addressService, _workContext, _taxSettings, _cartSettings, pluginFinder, _geoCountryLookup, this.ProviderManager);
        }

        [Test]
        public void Can_load_taxProviders()
        {
            var providers = _taxService.LoadAllTaxProviders();
            providers.ShouldNotBeNull();
            (providers.Any()).ShouldBeTrue();
        }

        [Test]
        public void Can_load_taxProvider_by_systemKeyword()
        {
            var provider = _taxService.LoadTaxProviderBySystemName("FixedTaxRateTest");
            provider.ShouldNotBeNull();
        }

        [Test]
        public void Can_load_active_taxProvider()
        {
            var provider = _taxService.LoadActiveTaxProvider();
            provider.ShouldNotBeNull();
        }

        [Test]
        public void Can_check_taxExempt_product()
        {
            var product = new Product();
            product.IsTaxExempt = true;
            _taxService.IsTaxExempt(product, null).ShouldEqual(true);
            product.IsTaxExempt = false;
            _taxService.IsTaxExempt(product, null).ShouldEqual(false);
        }

        [Test]
        public void Can_check_taxExempt_customer()
        {
            var customer = new Customer();
            customer.IsTaxExempt = true;
            _taxService.IsTaxExempt(null, customer).ShouldEqual(true);
            customer.IsTaxExempt = false;
            _taxService.IsTaxExempt(null, customer).ShouldEqual(false);
        }

        [Test]
        public void Can_check_taxExempt_customer_in_taxExemptCustomerRole()
        {
            var customer = new Customer();
            customer.IsTaxExempt = false;
            _taxService.IsTaxExempt(null, customer).ShouldEqual(false);

            var customerRole = new CustomerRole
            {
                TaxExempt = true,
                Active = true
            };

            customer.CustomerRoleMappings.Add(new CustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRole = customerRole
            });
            _taxService.IsTaxExempt(null, customer).ShouldEqual(true);

            customerRole.TaxExempt = false;
            _taxService.IsTaxExempt(null, customer).ShouldEqual(false);

            // If role is not active, weshould ignore 'TaxExempt' property.
            customerRole.Active = false;
            _taxService.IsTaxExempt(null, customer).ShouldEqual(false);
        }

        protected decimal GetFixedTestTaxRate()
        {
            //10 is a fixed tax rate returned from FixedRateTestTaxProvider. Perhaps, it should be configured in some other way 
            return 10;
        }

        [Test]
        public void Can_get_productPrice_priceIncludesTax_includingTax()
        {
            var customer = new Customer();
            var currency = new Currency();
            var product = new Product();

            decimal taxRate;
            _taxService.GetProductPrice(product, 0, 1000M, true, customer, currency, true, out taxRate).ShouldEqual(1000);
            _taxService.GetProductPrice(product, 0, 1000M, true, customer, currency, false, out taxRate).ShouldEqual(1100);
            _taxService.GetProductPrice(product, 0, 1000M, false, customer, currency, true, out taxRate).ShouldEqual(909.0909090909090909090909091M);
            _taxService.GetProductPrice(product, 0, 1000M, false, customer, currency, false, out taxRate).ShouldEqual(1000);
        }

        [Test]
        public void Can_do_VAT_check()
        {
            Exception ex;

            // Check VAT of DB Vertrieb GmbH (Deutsche Bahn).
            VatNumberStatus vatNumberStatus1 = _taxService.DoVatCheck("DE", "814160246", out var _, out var _, out ex);
            ex.ShouldBeNull();
            vatNumberStatus1.ShouldEqual(VatNumberStatus.Valid);

            VatNumberStatus vatNumberStatus2 = _taxService.DoVatCheck("DE", "000000000", out var _, out var _, out ex);
            vatNumberStatus2.ShouldEqual(VatNumberStatus.Invalid);
            ex.ShouldBeNull();
        }
    }
}
