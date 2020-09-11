using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductPicturePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productPicture()
        {
            var productPicture = new ProductMediaFile
            {
                DisplayOrder = 1,
                Product = new Product
                {
                    Name = "Name 1",
                    Published = true,
                    Deleted = false,
                    CreatedOnUtc = new DateTime(2010, 01, 01),
                    UpdatedOnUtc = new DateTime(2010, 01, 02)
                },
                MediaFile = new MediaFile
                {
                    MediaStorage = new MediaStorage
                    {
                        Data = new byte[] { 1, 2, 3 }
                    },
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    MimeType = "image/pjpeg",
                    MediaType = "image"
                }
            };

            var fromDb = SaveAndLoadEntity(productPicture);
            fromDb.ShouldNotBeNull();
            fromDb.DisplayOrder.ShouldEqual(1);

            fromDb.Product.ShouldNotBeNull();
            fromDb.Product.Name.ShouldEqual("Name 1");

            fromDb.MediaFile.ShouldNotBeNull();
            fromDb.MediaFile.MimeType.ShouldEqual("image/pjpeg");
        }
    }
}
