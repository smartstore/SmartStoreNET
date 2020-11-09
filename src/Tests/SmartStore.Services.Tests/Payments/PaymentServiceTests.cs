using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Payments
{
    [TestFixture]
    public class PaymentServiceTests : ServiceTest
    {
        IRepository<PaymentMethod> _paymentMethodRepository;
        IRepository<StoreMapping> _storeMappingRepository;
        IStoreMappingService _storeMappingService;
        PaymentSettings _paymentSettings;
        ICartRuleProvider _cartRuleProvider;
        IPaymentService _paymentService;
        ICommonServices _services;
        ITypeFinder _typeFinder;

        [SetUp]
        public new void SetUp()
        {
            _paymentSettings = new PaymentSettings();
            _paymentSettings.ActivePaymentMethodSystemNames = new List<string>();
            _paymentSettings.ActivePaymentMethodSystemNames.Add("Payments.TestMethod");

            _storeMappingRepository = MockRepository.GenerateMock<IRepository<StoreMapping>>();
            _storeMappingService = MockRepository.GenerateMock<IStoreMappingService>();
            _cartRuleProvider = MockRepository.GenerateMock<ICartRuleProvider>();

            _services = MockRepository.GenerateMock<ICommonServices>();
            _services.Expect(x => x.RequestCache).Return(NullRequestCache.Instance);

            var paymentMethods = new List<PaymentMethod> { new PaymentMethod { PaymentMethodSystemName = "Payments.TestMethod" } };

            _paymentMethodRepository = MockRepository.GenerateMock<IRepository<PaymentMethod>>();
            _paymentMethodRepository.Expect(x => x.TableUntracked).Return(paymentMethods.AsQueryable());

            _typeFinder = MockRepository.GenerateMock<ITypeFinder>();
            _typeFinder.Expect(x => x.FindClassesOfType((Type)null, null, true)).IgnoreArguments().Return(Enumerable.Empty<Type>()).Repeat.Any();

            var localizationService = MockRepository.GenerateMock<ILocalizationService>();
            localizationService.Expect(ls => ls.GetResource(null)).IgnoreArguments().Return("NotSupported").Repeat.Any();

            _paymentService = new PaymentService(_paymentMethodRepository, _storeMappingRepository, _storeMappingService, _paymentSettings, _cartRuleProvider,
                this.ProviderManager, _services, _typeFinder);
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
            srcm.Any().ShouldBeTrue();
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
