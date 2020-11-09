using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Orders
{
    [TestFixture]
    public class OrderNotePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_orderNote()
        {
            var on = new OrderNote()
            {
                Order = GetTestOrder(),
                Note = "Note1",
                DisplayToCustomer = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
            };

            var fromDb = SaveAndLoadEntity(on);
            fromDb.ShouldNotBeNull();
            fromDb.Note.ShouldEqual("Note1");
            fromDb.DisplayToCustomer.ShouldEqual(true);
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));

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
    }
}