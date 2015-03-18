﻿using System;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Customers
{
    [TestFixture]
    public class RewardPointsHistoryPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_rewardPointsHistory()
        {
            var rewardPointsHistory = new RewardPointsHistory()
            {
                Customer = GetTestCustomer(),
                Points = 1,
                Message = "Points for registration",
                PointsBalance = 2,
                UsedAmount = 3.1M,
                CreatedOnUtc = new DateTime(2010, 01, 01)
            };

            var fromDb = SaveAndLoadEntity(rewardPointsHistory);
            fromDb.ShouldNotBeNull();
            fromDb.Points.ShouldEqual(1);
            fromDb.Message.ShouldEqual("Points for registration");
            fromDb.PointsBalance.ShouldEqual(2);
            fromDb.UsedAmount.ShouldEqual(3.1M);
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));

            fromDb.Customer.ShouldNotBeNull();
        }
        [Test]
        public void Can_save_and_load_rewardPointsHistory_with_order()
        {
            var rewardPointsHistory = new RewardPointsHistory()
            {
                Customer = GetTestCustomer(),
                UsedWithOrder = GetTestOrder(),
                Points = 1,
                Message = "Points for registration",
                PointsBalance = 2,
                UsedAmount = 3,
                CreatedOnUtc = new DateTime(2010, 01, 01)
            };

            var fromDb = SaveAndLoadEntity(rewardPointsHistory);
            fromDb.ShouldNotBeNull();

            fromDb.UsedWithOrder.ShouldNotBeNull();
            fromDb.UsedWithOrder.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));
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
    }
}