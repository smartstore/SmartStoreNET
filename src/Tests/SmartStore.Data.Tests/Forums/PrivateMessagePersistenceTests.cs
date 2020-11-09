using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Data.Tests.Stores;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Forums
{
    [TestFixture]
    public class PrivateMessagePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_privatemessage()
        {
            var store = StorePersistenceTests.GetTestStore();
            var storeFromDb = SaveAndLoadEntity(store);
            storeFromDb.ShouldNotBeNull();

            var customer1 = GetTestCustomer();
            var customer1FromDb = SaveAndLoadEntity(customer1);
            customer1FromDb.ShouldNotBeNull();

            var customer2 = GetTestCustomer();
            var customer2FromDb = SaveAndLoadEntity(customer2);
            customer2FromDb.ShouldNotBeNull();

            var privateMessage = new PrivateMessage
            {
                Subject = "Private Message 1 Subject",
                Text = "Private Message 1 Text",
                IsDeletedByAuthor = false,
                IsDeletedByRecipient = false,
                IsRead = false,
                CreatedOnUtc = DateTime.UtcNow,
                FromCustomerId = customer1FromDb.Id,
                ToCustomerId = customer2FromDb.Id,
                StoreId = store.Id
            };

            var fromDb = SaveAndLoadEntity(privateMessage);
            fromDb.ShouldNotBeNull();
            fromDb.Subject.ShouldEqual("Private Message 1 Subject");
            fromDb.Text.ShouldEqual("Private Message 1 Text");
            fromDb.IsDeletedByAuthor.ShouldBeFalse();
            fromDb.IsDeletedByRecipient.ShouldBeFalse();
            fromDb.IsRead.ShouldBeFalse();
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
    }
}
