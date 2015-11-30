using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Core.Events;
using SmartStore.Tests;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Services.Common;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Tests.Discounts
{
    [TestFixture]
    public class DiscountServiceTests : ServiceTest
    {
        IRepository<Discount> _discountRepo;
        IRepository<DiscountRequirement> _discountRequirementRepo;
        IRepository<DiscountUsageHistory> _discountUsageHistoryRepo;
        IEventPublisher _eventPublisher;
		IGenericAttributeService _genericAttributeService;
        IDiscountService _discountService;
		IStoreContext _storeContext;
		ISettingService _settingService;
        
        [SetUp]
        public new void SetUp()
        {
            _discountRepo = MockRepository.GenerateMock<IRepository<Discount>>();
            var discount1 = new Discount
            {
                Id = 1,
                DiscountType = DiscountType.AssignedToCategories,
                Name = "Discount 1",
                UsePercentage = true,
                DiscountPercentage = 10,
                DiscountAmount = 0,
                DiscountLimitation = DiscountLimitationType.Unlimited,
                LimitationTimes = 0,
            };
            var discount2 = new Discount
            {
                Id = 2,
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 2",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                RequiresCouponCode = true,
                CouponCode = "SecretCode",
                DiscountLimitation = DiscountLimitationType.NTimesPerCustomer,
                LimitationTimes = 3,
            };

            _discountRepo.Expect(x => x.Table).Return(new List<Discount>() { discount1, discount2 }.AsQueryable());

            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

			_storeContext = MockRepository.GenerateMock<IStoreContext>();
			_storeContext.Expect(x => x.CurrentStore).Return(new Store 
			{ 
				Id = 1,
				Name = "MyStore"
			});

			_settingService = MockRepository.GenerateMock<ISettingService>();

            var cacheManager = new NullCache();
            _discountRequirementRepo = MockRepository.GenerateMock<IRepository<DiscountRequirement>>();
            _discountUsageHistoryRepo = MockRepository.GenerateMock<IRepository<DiscountUsageHistory>>();
            var pluginFinder = new PluginFinder();
			_genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();

			_discountService = new DiscountService(cacheManager, _discountRepo, _discountRequirementRepo,
				_discountUsageHistoryRepo, _storeContext, _genericAttributeService, pluginFinder, _eventPublisher,
				_settingService, base.ProviderManager);
        }

        [Test]
        public void Can_get_all_discount()
        {
            var discounts = _discountService.GetAllDiscounts(null);
            discounts.ShouldNotBeNull();
            (discounts.Count > 0).ShouldBeTrue();
        }

        [Test]
        public void Can_load_discountRequirementRules()
        {
            var rules = _discountService.LoadAllDiscountRequirementRules();
            rules.ShouldNotBeNull();
            (rules.Any()).ShouldBeTrue();
        }

        [Test]
        public void Can_load_discountRequirementRuleBySystemKeyword()
        {
            var rule = _discountService.LoadDiscountRequirementRuleBySystemName("TestDiscountRequirementRule");
            rule.ShouldNotBeNull();
        }

        [Test]
		public void Should_accept_valid_discount_code()
        {
            var discount = new Discount
            {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 2",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                RequiresCouponCode = true,
                CouponCode = "CouponCode 1",
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };
            
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };

			_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
				 .Return(new List<GenericAttribute>()
                            {
                                new GenericAttribute()
                                    {
                                        EntityId = customer.Id,
                                        Key = SystemCustomerAttributeNames.DiscountCouponCode,
                                        KeyGroup = "Customer",
                                        Value = "CouponCode 1"
                                    }
                            });

            var result1 = _discountService.IsDiscountValid(discount, customer);
            result1.ShouldEqual(true);
        }

		[Test]
		public void Should_not_accept_wrong_discount_code()
		{
			var discount = new Discount
			{
				DiscountType = DiscountType.AssignedToSkus,
				Name = "Discount 2",
				UsePercentage = false,
				DiscountPercentage = 0,
				DiscountAmount = 5,
				RequiresCouponCode = true,
				CouponCode = "CouponCode 1",
				DiscountLimitation = DiscountLimitationType.Unlimited,
			};

			var customer = new Customer
			{
				CustomerGuid = Guid.NewGuid(),
				AdminComment = "",
				Active = true,
				Deleted = false,
				CreatedOnUtc = new DateTime(2010, 01, 01),
				LastActivityDateUtc = new DateTime(2010, 01, 02)
			};

			_genericAttributeService.Expect(x => x.GetAttributesForEntity(customer.Id, "Customer"))
				.Return(new List<GenericAttribute>()
                            {
                                new GenericAttribute()
                                    {
                                        EntityId = customer.Id,
                                        Key = SystemCustomerAttributeNames.DiscountCouponCode,
                                        KeyGroup = "Customer",
                                        Value = "CouponCode 2"
                                    }
                            });

			var result2 = _discountService.IsDiscountValid(discount, customer);
			result2.ShouldEqual(false);
		}

        [Test]
        public void Can_validate_discount_dateRange()
        {
            var discount = new Discount
            {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 2",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                StartDateUtc = DateTime.UtcNow.AddDays(-1),
                EndDateUtc = DateTime.UtcNow.AddDays(1),
                RequiresCouponCode = false,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };

            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };

			var result1 = _discountService.IsDiscountValid(discount, customer);
			result1.ShouldEqual(true);

			discount.StartDateUtc = DateTime.UtcNow.AddDays(1);
			var result2 = _discountService.IsDiscountValid(discount, customer);
			result2.ShouldEqual(false);
        }
    }
}
