using NUnit.Framework;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Shipping
{
    [TestFixture]
    public class ShippingMethodPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_shippingMethod()
        {
            var shippingMethod = new ShippingMethod
            {
                Name = "Name 1",
                Description = "Description 1",
                DisplayOrder = 1
            };

            var fromDb = SaveAndLoadEntity(shippingMethod);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.Description.ShouldEqual("Description 1");
            fromDb.DisplayOrder.ShouldEqual(1);
        }

        protected Country GetTestCountry()
        {
            return new Country
            {
                Name = "United States",
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
            };
        }
    }
}