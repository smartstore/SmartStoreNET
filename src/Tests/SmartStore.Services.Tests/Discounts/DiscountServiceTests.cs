using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Common;
using SmartStore.Services.Discounts;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Discounts
{
    [TestFixture]
    public class DiscountServiceTests : ServiceTest
    {
        IRepository<Discount> _discountRepo;
        IRepository<DiscountUsageHistory> _discountUsageHistoryRepo;
        IGenericAttributeService _genericAttributeService;
        IDiscountService _discountService;
        IStoreContext _storeContext;
        ICartRuleProvider _cartRuleProvider;

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

            _discountRepo.Expect(x => x.Table).Return(new List<Discount> { discount1, discount2 }.AsQueryable());

            _storeContext = MockRepository.GenerateMock<IStoreContext>();
            _storeContext.Expect(x => x.CurrentStore).Return(new Store
            {
                Id = 1,
                Name = "MyStore"
            });

            _discountUsageHistoryRepo = MockRepository.GenerateMock<IRepository<DiscountUsageHistory>>();
            _genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();
            _cartRuleProvider = MockRepository.GenerateMock<ICartRuleProvider>();

            _discountService = new DiscountService(NullRequestCache.Instance, _discountRepo, _discountUsageHistoryRepo, _storeContext, _genericAttributeService, _cartRuleProvider);
        }

        [Test]
        public void Can_get_all_discount()
        {
            var discounts = _discountService.GetAllDiscounts(null);
            discounts.ShouldNotBeNull();
            (discounts.Count() > 0).ShouldBeTrue();
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

            _genericAttributeService.Expect(x => x.GetAttribute<string>(nameof(Customer), customer.Id, SystemCustomerAttributeNames.DiscountCouponCode, 0))
                .Return("CouponCode 1");

            _cartRuleProvider.Expect(x => x.RuleMatches(discount)).Return(true);

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

            _genericAttributeService.Expect(x => x.GetAttribute<string>(nameof(Customer), customer.Id, SystemCustomerAttributeNames.DiscountCouponCode, 0))
                .Return("CouponCode 2");

            var result2 = _discountService.IsDiscountValid(discount, customer);
            result2.ShouldEqual(false);
        }

        [Test]
        public void Can_validate_discount_dateRange()
        {
            var customer = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };

            var discount1 = new Discount
            {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 1",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                StartDateUtc = DateTime.UtcNow.AddDays(-1),
                EndDateUtc = DateTime.UtcNow.AddDays(1),
                RequiresCouponCode = false,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };

            var discount2 = new Discount
            {
                DiscountType = DiscountType.AssignedToSkus,
                Name = "Discount 2",
                UsePercentage = false,
                DiscountPercentage = 0,
                DiscountAmount = 5,
                StartDateUtc = DateTime.UtcNow.AddDays(1),
                EndDateUtc = DateTime.UtcNow.AddDays(2),
                RequiresCouponCode = false,
                DiscountLimitation = DiscountLimitationType.Unlimited,
            };

            _cartRuleProvider.Expect(x => x.RuleMatches(discount1)).Return(true);

            var result1 = _discountService.IsDiscountValid(discount1, customer);
            result1.ShouldEqual(true);

            var result2 = _discountService.IsDiscountValid(discount2, customer);
            result2.ShouldEqual(false);
        }
    }
}
