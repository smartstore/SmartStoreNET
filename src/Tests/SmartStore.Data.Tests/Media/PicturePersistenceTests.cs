using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Media;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Media
{
    [TestFixture]
    public class PicturePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_picture()
        {
            var picture = new MediaFile
            {
                MediaStorage = new MediaStorage
                {
                    Data = new byte[] { 1, 2, 3 }
                },
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MimeType = "image/pjpeg",
                MediaType = "image",
                Name = "seo filename 1",
                IsTransient = true
            };

            var fromDb = SaveAndLoadEntity(picture);
            fromDb.ShouldNotBeNull();
            fromDb.MediaStorage.Data.ShouldEqual(new byte[] { 1, 2, 3 });
            fromDb.MimeType.ShouldEqual("image/pjpeg");
            fromDb.Name.ShouldEqual("seo filename 1");
            fromDb.IsTransient.ShouldEqual(true);
        }
    }
}
