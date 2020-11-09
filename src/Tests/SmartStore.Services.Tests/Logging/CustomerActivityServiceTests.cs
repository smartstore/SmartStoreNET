using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;
using SmartStore.Services.Logging;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Logging
{
    [TestFixture]
    public class CustomerActivityServiceTests : ServiceTest
    {
        IRepository<ActivityLog> _activityLogRepository;
        IRepository<ActivityLogType> _activityLogTypeRepository;
        IRepository<Customer> _customerRepository;
        IWorkContext _workContext;
        ICustomerActivityService _customerActivityService;
        ActivityLogType _activityType1, _activityType2;
        ActivityLog _activity1, _activity2;
        Customer _customer1, _customer2;

        [SetUp]
        public new void SetUp()
        {
            _activityType1 = new ActivityLogType
            {
                Id = 1,
                SystemKeyword = "TestKeyword1",
                Enabled = true,
                Name = "Test name1"
            };
            _activityType2 = new ActivityLogType
            {
                Id = 2,
                SystemKeyword = "TestKeyword2",
                Enabled = true,
                Name = "Test name2"
            };
            _customer1 = new Customer()
            {
                Id = 1,
                Email = "test1@teststore1.com",
                Username = "TestUser1",
                Deleted = false,
            };
            _customer2 = new Customer()
            {
                Id = 2,
                Email = "test2@teststore2.com",
                Username = "TestUser2",
                Deleted = false,
            };
            _activity1 = new ActivityLog()
            {
                Id = 1,
                ActivityLogType = _activityType1,
                CustomerId = _customer1.Id,
                Customer = _customer1
            };
            _activity2 = new ActivityLog()
            {
                Id = 2,
                ActivityLogType = _activityType1,
                CustomerId = _customer2.Id,
                Customer = _customer2
            };

            _workContext = MockRepository.GenerateMock<IWorkContext>();
            _activityLogRepository = MockRepository.GenerateMock<IRepository<ActivityLog>>();
            _activityLogTypeRepository = MockRepository.GenerateMock<IRepository<ActivityLogType>>();
            _customerRepository = MockRepository.GenerateMock<IRepository<Customer>>();
            _activityLogTypeRepository.Expect(x => x.Table).Return(new List<ActivityLogType>() { _activityType1, _activityType2 }.AsQueryable());
            _activityLogRepository.Expect(x => x.Table).Return(new List<ActivityLog>() { _activity1, _activity2 }.AsQueryable());

            _customerActivityService = new CustomerActivityService(_activityLogRepository, _activityLogTypeRepository, _customerRepository, _workContext, null);
        }

        [Test]
        public void Can_Find_Activities()
        {
            var activities = _customerActivityService.GetAllActivities(null, null, 1, 0, 0, 10);
            activities.Contains(_activity1).ShouldBeTrue();
            activities = _customerActivityService.GetAllActivities(null, null, 2, 0, 0, 10);
            activities.Contains(_activity1).ShouldBeFalse();
            activities = _customerActivityService.GetAllActivities(null, null, 2, 0, 0, 10);
            activities.Contains(_activity2).ShouldBeTrue();
        }

        [Test]
        public void Can_Find_Activity_By_Id()
        {
            var activity = _customerActivityService.GetActivityById(1);
            activity.ShouldBeTheSameAs(_activity1);
            activity = _customerActivityService.GetActivityById(2);
            activity.ShouldBeTheSameAs(_activity2);
        }
    }
}
