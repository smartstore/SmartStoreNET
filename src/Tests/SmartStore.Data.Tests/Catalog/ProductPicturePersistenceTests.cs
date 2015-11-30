using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests.Catalog
{
    [TestFixture]
    public class ProductPicturePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_productPicture()
        {
            var productPicture = new ProductPicture
                                     {
                                         DisplayOrder = 1,
                                         Product = new Product()
                                                       {
                                                           Name = "Name 1",
                                                           Published = true,
                                                           Deleted = false,
                                                           CreatedOnUtc = new DateTime(2010, 01, 01),
                                                           UpdatedOnUtc = new DateTime(2010, 01, 02)
                                                       },
                                         Picture = new Picture()
                                                                      {
                                                                          PictureBinary = new byte[] { 1, 2, 3 },
                                                                          MimeType = "image/pjpeg",
                                                                          IsNew = true
                                                                      }
                                     };

            var fromDb = SaveAndLoadEntity(productPicture);
            fromDb.ShouldNotBeNull();
            fromDb.DisplayOrder.ShouldEqual(1);

            fromDb.Product.ShouldNotBeNull();
            fromDb.Product.Name.ShouldEqual("Name 1");

            fromDb.Picture.ShouldNotBeNull();
            fromDb.Picture.MimeType.ShouldEqual("image/pjpeg");
        }
    }
}
