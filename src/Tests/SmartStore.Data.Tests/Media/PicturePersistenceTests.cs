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
            var picture = new Picture
            {
				MediaStorage = new MediaStorage
				{
					Data = new byte[] { 1, 2, 3 }
				},
				UpdatedOnUtc = DateTime.UtcNow,
				MimeType = "image/pjpeg",
                SeoFilename = "seo filename 1",
                IsNew = true
            };

            var fromDb = SaveAndLoadEntity(picture);
            fromDb.ShouldNotBeNull();
            fromDb.MediaStorage.Data.ShouldEqual(new byte[] { 1, 2, 3 });
            fromDb.MimeType.ShouldEqual("image/pjpeg");
            fromDb.SeoFilename.ShouldEqual("seo filename 1");
            fromDb.IsNew.ShouldEqual(true);
        }
    }
}
