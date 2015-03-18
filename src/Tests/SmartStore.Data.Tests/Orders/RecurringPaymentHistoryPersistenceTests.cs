﻿using System;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Orders
{
    [TestFixture]
    public class RecurringPaymentHistoryPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_recurringPaymentHistory()
        {
            var rph = new RecurringPaymentHistory()
            {
                CreatedOnUtc = new DateTime(2010, 01, 03),
                RecurringPayment = new RecurringPayment()
                {
                    StartDateUtc = new DateTime(2010, 01, 01),
                    CreatedOnUtc = new DateTime(2010, 01, 02),
                    InitialOrder = GetTestOrder(),
                },
            };

            var fromDb = SaveAndLoadEntity(rph);
            fromDb.ShouldNotBeNull();
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 03));

            fromDb.RecurringPayment.ShouldNotBeNull();
            fromDb.RecurringPayment.StartDateUtc.ShouldEqual(new DateTime(2010, 01, 01));
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