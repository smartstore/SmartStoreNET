using NUnit.Framework;
using SmartStore.Core.Domain.Stores;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Stores
{
    [TestFixture]
    public class StoreMappingPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_urlRecord()
        {
            var storeMapping = new StoreMapping
            {
                EntityId = 1,
                EntityName = "EntityName 1",
                StoreId = 2,
            };

            var fromDb = SaveAndLoadEntity(storeMapping);
            fromDb.ShouldNotBeNull();
            fromDb.EntityId.ShouldEqual(1);
            fromDb.EntityName.ShouldEqual("EntityName 1");
            fromDb.StoreId.ShouldEqual(2);
        }
    }
}
