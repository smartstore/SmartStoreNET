using NUnit.Framework;
using SmartStore.Core.Domain.Directory;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Directory
{
    [TestFixture]
    public class MeasureDimensionPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_measureDimension()
        {
            var measureDimension = new MeasureDimension
            {
                Name = "inch(es)",
                SystemKeyword = "inch",
                Ratio = 1.12345678M,
                DisplayOrder = 2,
            };

            var fromDb = SaveAndLoadEntity(measureDimension);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("inch(es)");
            fromDb.SystemKeyword.ShouldEqual("inch");
            fromDb.Ratio.ShouldEqual(1.12345678M);
            fromDb.DisplayOrder.ShouldEqual(2);
        }
    }
}
