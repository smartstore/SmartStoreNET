using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Tests.Payments
{
    [TestFixture]
    public class PaymentServiceTests : ServiceTest
    {
        PaymentSettings _paymentSettings;
        ShoppingCartSettings _shoppingCartSettings;
        IPaymentService _paymentService;
		ISettingService _settingService;
        
        [SetUp]
        public new void SetUp()
        {
            _paymentSettings = new PaymentSettings();
            _paymentSettings.ActivePaymentMethodSystemNames = new List<string>();
            _paymentSettings.ActivePaymentMethodSystemNames.Add("Payments.TestMethod");

            var pluginFinder = new PluginFinder();

            _shoppingCartSettings = new ShoppingCartSettings();
			_settingService = MockRepository.GenerateMock<ISettingService>();

			var localizationService = MockRepository.GenerateMock<ILocalizationService>();
			localizationService.Expect(ls => ls.GetResource(null)).IgnoreArguments().Return("NotSupported").Repeat.Any();

			_paymentService = new PaymentService(_paymentSettings, pluginFinder, _shoppingCartSettings, _settingService, localizationService);
        }

        [Test]
        public void Can_load_paymentMethods()
        {
            var srcm = _paymentService.LoadActivePaymentMethods();
            srcm.ShouldNotBeNull();
            (srcm.Count > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_load_paymentMethod_by_systemKeyword()
        {
            var srcm = _paymentService.LoadPaymentMethodBySystemName("Payments.TestMethod");
            srcm.ShouldNotBeNull();
        }

        [Test]
        public void Can_load_active_paymentMethods()
        {
            var srcm = _paymentService.LoadActivePaymentMethods();
            srcm.ShouldNotBeNull();
            (srcm.Count > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_get_masked_credit_card_number()
        {
            _paymentService.GetMaskedCreditCardNumber("").ShouldEqual("");
            _paymentService.GetMaskedCreditCardNumber("123").ShouldEqual("123");
            _paymentService.GetMaskedCreditCardNumber("1234567890123456").ShouldEqual("************3456");
        }
    }
}
