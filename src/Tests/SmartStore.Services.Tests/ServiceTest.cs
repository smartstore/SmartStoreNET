using System.Collections.Generic;
using NUnit.Framework;
using SmartStore.Core.Plugins;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Tests.Directory;
using SmartStore.Services.Tests.Media.Storage;
using SmartStore.Services.Tests.Payments;
using SmartStore.Services.Tests.Shipping;
using SmartStore.Services.Tests.Tax;

namespace SmartStore.Services.Tests
{
    [TestFixture]
    public abstract class ServiceTest
    {
        private MockProviderManager _providerManager = new MockProviderManager();

        [SetUp]
        public void SetUp()
        {
            //init plugins
            InitPlugins();

            InitProviders();
        }

        private void InitProviders()
        {
            _providerManager.RegisterProvider("FixedTaxRateTest", new FixedRateTestTaxProvider());
            _providerManager.RegisterProvider("FixedRateTestShippingRateComputationMethod", new FixedRateTestShippingRateComputationMethod());
            _providerManager.RegisterProvider("CurrencyExchange.TestProvider", new TestExchangeRateProvider());
            _providerManager.RegisterProvider("Payments.TestMethod", new TestPaymentMethod());
            _providerManager.RegisterProvider(DatabaseMediaStorageProvider.SystemName, new TestDatabaseMediaStorageProvider());
        }

        private void InitPlugins()
        {
            var plugins = new List<PluginDescriptor>();
            plugins.Add(new PluginDescriptor(typeof(FixedRateTestTaxProvider).Assembly, null, typeof(FixedRateTestTaxProvider))
            {
                SystemName = "FixedTaxRateTest",
                FriendlyName = "Fixed tax test rate provider",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(FixedRateTestShippingRateComputationMethod).Assembly, null, typeof(FixedRateTestShippingRateComputationMethod))
            {
                SystemName = "FixedRateTestShippingRateComputationMethod",
                FriendlyName = "Fixed rate test shipping computation method",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(TestPaymentMethod).Assembly, null, typeof(TestPaymentMethod))
            {
                SystemName = "Payments.TestMethod",
                FriendlyName = "Test payment method",
                Installed = true,
            });
            plugins.Add(new PluginDescriptor(typeof(TestExchangeRateProvider).Assembly, null, typeof(TestExchangeRateProvider))
            {
                SystemName = "CurrencyExchange.TestProvider",
                FriendlyName = "Test exchange rate provider",
                Installed = true,
            });

            PluginManager.ReferencedPlugins = plugins;
        }

        protected MockProviderManager ProviderManager => _providerManager;
    }
}
