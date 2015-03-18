﻿using System;
using System.Linq;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Shipping
{
    [TestFixture]
    public class ShipmentPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_shipment()
        {
            var shipment = new Shipment
            {
                Order = GetTestOrder(),
                TrackingNumber = "TrackingNumber 1",
                TotalWeight = 9.87M,
                ShippedDateUtc = new DateTime(2010, 01, 01),
                DeliveryDateUtc = new DateTime(2010, 01, 02),
                CreatedOnUtc = new DateTime(2010, 01, 03),
            };

            var fromDb = SaveAndLoadEntity(shipment);
            fromDb.ShouldNotBeNull();
            fromDb.TrackingNumber.ShouldEqual("TrackingNumber 1");
            fromDb.TotalWeight.ShouldEqual(9.87M);
            fromDb.ShippedDateUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.DeliveryDateUtc.ShouldEqual(new DateTime(2010, 01, 02));
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 03));
        }

        [Test]
        public void Can_save_and_load_shipment_with_items()
        {
            var shipment = new Shipment
            {
                Order = GetTestOrder(),
                TrackingNumber = "TrackingNumber 1",
                ShippedDateUtc = new DateTime(2010, 01, 01),
                DeliveryDateUtc = new DateTime(2010, 01, 02),
                CreatedOnUtc = new DateTime(2010, 01, 03),
            };
            shipment.ShipmentItems.Add(new ShipmentItem()
            {
                OrderItemId = 1,
                Quantity = 2,
            });

            var fromDb = SaveAndLoadEntity(shipment);
            fromDb.ShouldNotBeNull();


            fromDb.ShipmentItems.ShouldNotBeNull();
            (fromDb.ShipmentItems.Count == 1).ShouldBeTrue();
            fromDb.ShipmentItems.First().Quantity.ShouldEqual(2);
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
				CreatedOnUtc = new DateTime(2010, 05, 06),
				UpdatedOnUtc = new DateTime(2010, 01, 01)
            };
        }
    }
}