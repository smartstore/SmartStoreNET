using System;
using NUnit.Framework;
using SmartStore.Core.Domain.Media;
using SmartStore.Tests;

namespace SmartStore.Data.Tests.Media
{
    [TestFixture]
    public class DownloadPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_download()
        {
            var guid = Guid.NewGuid();
            var download = new Download
            {
                DownloadGuid = guid,
                UseDownloadUrl = true,
                DownloadUrl = "http://www.someUrl.com/file.zip",
                UpdatedOnUtc = DateTime.UtcNow,
                EntityName = "Product",
                EntityId = 1,
                MediaFile = new MediaFile
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    MimeType = "application/x-zip-co",
                    MediaType = "bin",
                    Name = "file.zip",
                    Extension = "zip",
                    IsTransient = true,
                    MediaStorage = new MediaStorage { Data = new byte[] { 1, 2, 3 } }
                }
            };

            var fromDb = SaveAndLoadEntity(download);
            fromDb.ShouldNotBeNull();
            fromDb.DownloadGuid.ShouldEqual(guid);
            fromDb.UseDownloadUrl.ShouldEqual(true);
            fromDb.DownloadUrl.ShouldEqual("http://www.someUrl.com/file.zip");
            fromDb.MediaFile.MediaStorage.Data.ShouldEqual(new byte[] { 1, 2, 3 });
            fromDb.MediaFile.MimeType.ShouldEqual("application/x-zip-co");
            fromDb.MediaFile.Name.ShouldEqual("file.zip");
            fromDb.MediaFile.Extension.ShouldEqual("zip");
            fromDb.MediaFile.IsTransient.ShouldEqual(true);
            fromDb.EntityName.ShouldEqual("Product");
            fromDb.EntityId.ShouldEqual(1);
        }
    }
}
