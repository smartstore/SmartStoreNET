using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class CategoryTemplatePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_categoryTemplate()
        {
            var categoryTemplate = new CategoryTemplate()
            {
                Name = "Name 1",
                ViewPath = "ViewPath 1",
                DisplayOrder = 1,
            };

            var fromDb = SaveAndLoadEntity(categoryTemplate);
            fromDb.ShouldNotBeNull();
            fromDb.Name.ShouldEqual("Name 1");
            fromDb.ViewPath.ShouldEqual("ViewPath 1");
            fromDb.DisplayOrder.ShouldEqual(1);
        }
    }
}
