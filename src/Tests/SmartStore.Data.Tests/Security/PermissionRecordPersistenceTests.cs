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
            var permissionRecord = new PermissionRecord
            {
                SystemName = "SystemName 2"
            };

            var fromDb = SaveAndLoadEntity(permissionRecord);
            fromDb.ShouldNotBeNull();
            fromDb.SystemName.ShouldEqual("SystemName 2");
        }
    }
}
