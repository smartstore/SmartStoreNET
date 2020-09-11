using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductVariantAttributeCombinationPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productVariantAttributeCombination()
        {
            var pvac = new ProductVariantAttributeCombination
            {
                AttributesXml = "Some XML",
                StockQuantity = 2,
                Sku = "X1000",
                Price = 9.80M,
                AllowOutOfStockOrders = true,
                Product = GetTestProduct()
            };

            var fromDb = SaveAndLoadEntity(pvac);
            fromDb.ShouldNotBeNull();
            fromDb.AttributesXml.ShouldEqual("Some XML");
            fromDb.StockQuantity.ShouldEqual(2);
            fromDb.Sku.ShouldEqual("X1000");
            fromDb.Price.ShouldEqual(9.80M);
            fromDb.AllowOutOfStockOrders.ShouldEqual(true);
        }

        protected Product GetTestProduct()
        {
            return new Product
            {
                Name = "Product name 1",
                CreatedOnUtc = new DateTime(2010, 01, 03),
                UpdatedOnUtc = new DateTime(2010, 01, 04),
            };
        }
    }
}