using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Common;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;

namespace SmartStore.Services.Tests.Helpers
{
    [TestFixture]
    public class DateTimeHelperTests : ServiceTest
    {
        IWorkContext _workContext;
		IStoreContext _storeContext;
		IGenericAttributeService _genericAttributeService;
        ISettingService _settingService;
        DateTimeSettings _dateTimeSettings;
        IDateTimeHelper _dateTimeHelper;
		Store _store;

        [SetUp]
        public new void SetUp()
        {
			_genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _settingService = MockRepository.GenerateMock<ISettingService>();

			_workContext = MockRepository.GenerateMock<IWorkContext>();

			_store = new Store() { Id = 1 };
			_storeContext = MockRepository.GenerateMock<IStoreContext>();
			_storeContext.Expect(x => x.CurrentStore).Return(_store);

            _dateTimeSettings = new DateTimeSettings()
            {
                AllowCustomersToSetTimeZone = false,
                DefaultStoreTimeZoneId = ""
            };

			_dateTimeHelper = new DateTimeHelper(_workContext, _genericAttributeService,
                _settingService, _dateTimeSettings);
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
            _dateTimeSettings.DefaultStoreTimeZoneId = "E. Europe Standard Time"; //(GMT+02:00) Minsk;

            var customer = new Customer()
            {
				Id = 10
            };

			_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
				 .Return(new List<GenericAttribute>()
                            {
                                new GenericAttribute()
                                    {
                                        StoreId = 0,
                                        EntityId = customer.Id,
                                        Key = SystemCustomerAttributeNames.TimeZoneId,
                                        KeyGroup = "Customer",
                                        Value = "Russian Standard Time" //(GMT+03:00) Moscow, St. Petersburg, Volgograd
                                    }
                            });

            var timeZone = _dateTimeHelper.GetCustomerTimeZone(customer);
            timeZone.ShouldNotBeNull();
            timeZone.Id.ShouldEqual("Russian Standard Time");
        }

        [Test]
        public void Can_get_customer_timeZone_with_customTimeZones_disabled()
        {
            _dateTimeSettings.AllowCustomersToSetTimeZone = false;
            _dateTimeSettings.DefaultStoreTimeZoneId = "E. Europe Standard Time"; //(GMT+02:00) Minsk;

            var customer = new Customer()
            {
				Id = 10
            };

			_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
				 .Return(new List<GenericAttribute>()
                            {
                                new GenericAttribute()
                                    {
                                        StoreId = _store.Id,
                                        EntityId = customer.Id,
                                        Key = SystemCustomerAttributeNames.TimeZoneId,
                                        KeyGroup = "Customer",
                                        Value = "Russian Standard Time" //(GMT+03:00) Moscow, St. Petersburg, Volgograd
                                    }
                            });

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
