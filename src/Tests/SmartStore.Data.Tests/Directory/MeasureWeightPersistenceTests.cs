using NUnit.Framework;
using SmartStore.Core.Domain.Directory;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Directory
{
    [TestFixture]
    public class MeasureWeightPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_measureWeight()
        {
            var measureWeight = new MeasureWeight
            {
                Name = "ounce(s)",
                SystemKeyword = "oz",
                Ratio = 1.12345678M,
                DisplayOrder = 2,
            };

            var fromDb = SaveAndLoadEntity(measureWeight);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("ounce(s)");
            fromDb.SystemKeyword.ShouldEqual("oz");
            fromDb.Ratio.ShouldEqual(1.12345678M);
            fromDb.DisplayOrder.ShouldEqual(2);
        }
    }
}
