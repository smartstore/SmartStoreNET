using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Fakes;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Helpers
{
    [TestFixture]
    public class DateTimeHelperTests : ServiceTest
    {
        IRepository<Customer> _customerRepo;
        IRepository<CustomerRole> _customerRoleRepo;
        IRepository<CustomerRoleMapping> _customerRoleMappingRepo;
        IRepository<GenericAttribute> _genericAttributeRepo;
        IRepository<RewardPointsHistory> _rewardPointsHistoryRepo;
        IRepository<ShoppingCartItem> _shoppingCartItemRepo;
        IGenericAttributeService _genericAttributeService;
        IEventPublisher _eventPublisher;
        IWorkContext _workContext;
        IStoreContext _storeContext;
        ISettingService _settingService;
        ICustomerService _customerService;
        ICommonServices _services;
        DateTimeSettings _dateTimeSettings;
        Lazy<RewardPointsSettings> _rewardPointsSettings;
        IDateTimeHelper _dateTimeHelper;
        Store _store;
        IUserAgent _userAgent;
        Lazy<IGdprTool> _gdprTool;

        [SetUp]
        public new void SetUp()
        {
            _settingService = MockRepository.GenerateMock<ISettingService>();
            _workContext = MockRepository.GenerateMock<IWorkContext>();

            _store = new Store { Id = 1 };
            _storeContext = MockRepository.GenerateMock<IStoreContext>();
            _storeContext.Expect(x => x.CurrentStore).Return(_store);

            _dateTimeSettings = new DateTimeSettings
            {
                AllowCustomersToSetTimeZone = false,
                DefaultStoreTimeZoneId = ""
            };

            _rewardPointsSettings = new Lazy<RewardPointsSettings>(() => new RewardPointsSettings
            {
                Enabled = false
            });

            var customer1 = new Customer
            {
                Id = 1,
                TimeZoneId = "Russian Standard Time"    // (GMT+03:00) Moscow, St. Petersburg, Volgograd
            };

            _customerRepo = MockRepository.GenerateMock<IRepository<Customer>>();
            _customerRepo.Expect(x => x.Table).Return(new List<Customer> { customer1 }.AsQueryable());

            _customerRoleRepo = MockRepository.GenerateMock<IRepository<CustomerRole>>();
            _customerRoleMappingRepo = MockRepository.GenerateMock<IRepository<CustomerRoleMapping>>();
            _genericAttributeRepo = MockRepository.GenerateMock<IRepository<GenericAttribute>>();
            _rewardPointsHistoryRepo = MockRepository.GenerateMock<IRepository<RewardPointsHistory>>();
            _shoppingCartItemRepo = MockRepository.GenerateMock<IRepository<ShoppingCartItem>>();
            _userAgent = MockRepository.GenerateMock<IUserAgent>();
            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _gdprTool = MockRepository.GenerateMock<Lazy<IGdprTool>>();

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _services = MockRepository.GenerateMock<ICommonServices>();
            _services.Expect(x => x.StoreContext).Return(_storeContext);
            _services.Expect(x => x.RequestCache).Return(NullRequestCache.Instance);
            _services.Expect(x => x.Cache).Return(NullCache.Instance);
            _services.Expect(x => x.EventPublisher).Return(_eventPublisher);

            _customerService = new CustomerService(
                _customerRepo,
                _customerRoleRepo,
                _customerRoleMappingRepo,
                _genericAttributeRepo,
                _rewardPointsHistoryRepo,
                _shoppingCartItemRepo,
                _genericAttributeService,
                _rewardPointsSettings,
                _services,
                new FakeHttpContext("~/"),
                _userAgent,
                new CustomerSettings(),
                _gdprTool);

            _dateTimeHelper = new DateTimeHelper(_workContext, _settingService, _dateTimeSettings, _customerService);
        }

        [Test]
        public void Can_find_systemTimeZone_by_id()
        {
            var timeZones = _dateTimeHelper.FindTimeZoneById("E. Europe Standard Time");
            timeZones.ShouldNotBeNull();
            timeZones.Id.ShouldEqual("E. Europe Standard Time");
        }

        [Test]
        public void Can_get_all_systemTimeZones()
        {
            var systemTimeZones = _dateTimeHelper.GetSystemTimeZones();
            systemTimeZones.ShouldNotBeNull();
            (systemTimeZones.Count > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_get_customer_timeZone_with_customTimeZones_enabled()
        {
            _dateTimeSettings.AllowCustomersToSetTimeZone = true;
            _dateTimeSettings.DefaultStoreTimeZoneId = "E. Europe Standard Time"; // (GMT+02:00) Minsk;

            var customer = _customerService.GetCustomerById(1);

            var timeZone = _dateTimeHelper.GetCustomerTimeZone(customer);
            timeZone.ShouldNotBeNull();
            timeZone.Id.ShouldEqual("Russian Standard Time");
        }

        [Test]
        public void Can_get_customer_timeZone_with_customTimeZones_disabled()
        {
            _dateTimeSettings.AllowCustomersToSetTimeZone = false;
            _dateTimeSettings.DefaultStoreTimeZoneId = "E. Europe Standard Time"; //(GMT+02:00) Minsk;

            var customer = _customerService.GetCustomerById(1);

            var timeZone = _dateTimeHelper.GetCustomerTimeZone(customer);
            timeZone.ShouldNotBeNull();
            timeZone.Id.ShouldEqual("E. Europe Standard Time");
        }

        [Test]
        public void Can_convert_dateTime_to_userTime()
        {
            var sourceDateTime = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); // (GMT+01:00) Berlin;
            sourceDateTime.ShouldNotBeNull();

            var destinationDateTime = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time"); // (GMT+02:00) Istanbul;
            destinationDateTime.ShouldNotBeNull();

            // Berlin > Istanbul
            _dateTimeHelper
                .ConvertToUserTime(new DateTime(2015, 06, 1, 0, 0, 0), sourceDateTime, destinationDateTime)
                .ShouldEqual(new DateTime(2015, 06, 1, 1, 0, 0));

            // UTC > Istanbul (summer)
            _dateTimeHelper
                .ConvertToUserTime(new DateTime(2015, 06, 1, 0, 0, 0), TimeZoneInfo.Utc, destinationDateTime)
                .ShouldEqual(new DateTime(2015, 06, 1, 3, 0, 0));

            // UTC > Istanbul (winter)
            _dateTimeHelper
                .ConvertToUserTime(new DateTime(2015, 01, 01, 0, 0, 0), TimeZoneInfo.Utc, destinationDateTime)
                .ShouldEqual(new DateTime(2015, 01, 1, 2, 0, 0));
        }

        [Test]
        public void Can_convert_dateTime_to_utc_dateTime()
        {
            var sourceDateTime = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"); //(GMT+02:00) Minsk;
            sourceDateTime.ShouldNotBeNull();

            //summer time
            var dateTime1 = new DateTime(2010, 06, 01, 0, 0, 0);
            var convertedDateTime1 = _dateTimeHelper.ConvertToUtcTime(dateTime1, sourceDateTime);
            convertedDateTime1.ShouldEqual(new DateTime(2010, 05, 31, 21, 0, 0));

            //winter time
            var dateTime2 = new DateTime(2010, 01, 01, 0, 0, 0);
            var convertedDateTime2 = _dateTimeHelper.ConvertToUtcTime(dateTime2, sourceDateTime);
            convertedDateTime2.ShouldEqual(new DateTime(2009, 12, 31, 22, 0, 0));
        }
    }
}
