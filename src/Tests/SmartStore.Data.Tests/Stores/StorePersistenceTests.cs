using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Stores
{
    [TestFixture]
    public class StorePersistenceTests : PersistenceTest
    {
        public static Store GetTestStore()
        {
            var currency = new Currency
            {
                Name = "US Dollar",
                CurrencyCode = "USD",
                Rate = 1.1M,
                DisplayLocale = "en-US",
                CustomFormatting = "CustomFormatting 1",
                LimitedToStores = true,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                UpdatedOnUtc = new DateTime(2010, 01, 02),
            };

            var store = new Store
            {
                Name = "Computer store",
                Url = "http://www.yourStore.com",
                Hosts = "yourStore.com,www.yourStore.com",
                LogoMediaFileId = 0,
                DisplayOrder = 1,
                PrimaryStoreCurrency = currency,
                PrimaryExchangeRateCurrency = currency
            };

            return store;
        }

        [Test]
        public void Can_save_and_load_store()
        {
            var store = GetTestStore();

            var fromDb = SaveAndLoadEntity(store);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Computer store");
            fromDb.Url.ShouldEqual("http://www.yourStore.com");
            fromDb.Hosts.ShouldEqual("yourStore.com,www.yourStore.com");
            fromDb.LogoMediaFileId.ShouldEqual(0);
            fromDb.DisplayOrder.ShouldEqual(1);
        }
    }
}
