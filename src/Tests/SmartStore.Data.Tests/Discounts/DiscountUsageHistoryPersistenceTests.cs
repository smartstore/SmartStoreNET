﻿using System;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Discounts
{
    [TestFixture]
    public class DiscountUsageHistoryPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_discountUsageHistory()
        {
            var discount = new DiscountUsageHistory()
                    {
                        Discount = GetTestDiscount(),
                        Order = GetTestOrder(),
                        CreatedOnUtc = new DateTime(2010, 01, 01)
                    };

            var fromDb = SaveAndLoadEntity(discount);
            fromDb.ShouldNotBeNull();
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));

            fromDb.Discount.ShouldNotBeNull();
            fromDb.Order.ShouldNotBeNull();
        }

        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                AdminComment = "some comment here",
                Active = true,
                Deleted = false,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }

        protected Order GetTestOrder()
        {
            return new Order()
                {
                    OrderGuid = Guid.NewGuid(),
                    Customer = GetTestCustomer(),
                    BillingAddress = new Address()
                    {
                        Country = new Country()
                        {
                            Name = "United States",
                            TwoLetterIsoCode = "US",
                            ThreeLetterIsoCode = "USA",
                        },
                        CreatedOnUtc = new DateTime(2010, 01, 01),
                    },
                    Deleted = true,
					CreatedOnUtc = new DateTime(2010, 01, 01),
					UpdatedOnUtc = new DateTime(2010, 01, 01)
                };
        }

        protected Discount GetTestDiscount()
        {
            return new Discount
            {
                Name = "Discount 1",
                DiscountType = DiscountType.AssignedToCategories,
                UsePercentage = true,
                DiscountPercentage = 1,
                DiscountAmount = 2,
                StartDateUtc = new DateTime(2010, 01, 01),
                EndDateUtc = new DateTime(2010, 01, 02),
                RequiresCouponCode = true,
                CouponCode = "SecretCode",
                DiscountLimitation = DiscountLimitationType.Unlimited,
                LimitationTimes = 3,
            };
        }
    }
}