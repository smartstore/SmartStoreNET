using NUnit.Framework;
using SmartStore.Core.Domain.Security;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Security
{
    [TestFixture]
    public class PermissionRecordPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_permissionRecord()
        {
            var permissionRecord = GetTestPermissionRecord();

            var fromDb = SaveAndLoadEntity(permissionRecord);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.SystemName.ShouldEqual("SystemName 2");
            fromDb.Category.ShouldEqual("Category 4");
        }

        protected PermissionRecord GetTestPermissionRecord()
        {
            return new PermissionRecord
            {
                Name = "Name 1",
                SystemName = "SystemName 2",
                Category = "Category 4",
            };
        }
    }
}
