using System;
using SmartStore.Core.Domain.Media;
using SmartStore.Tests;
using NUnit.Framework;

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
				MediaStorage = new MediaStorage { Data = new byte[] { 1, 2, 3 } },
				UpdatedOnUtc = DateTime.UtcNow,
				ContentType = "application/x-zip-co",
                Filename = "file",
                Extension = ".zip",
                IsNew = true
            };

            var fromDb = SaveAndLoadEntity(download);
            fromDb.ShouldNotBeNull();
            fromDb.DownloadGuid.ShouldEqual(guid);
            fromDb.UseDownloadUrl.ShouldEqual(true);
            fromDb.DownloadUrl.ShouldEqual("http://www.someUrl.com/file.zip");
			fromDb.MediaStorage.Data.ShouldEqual(new byte[] { 1, 2, 3 });
			fromDb.ContentType.ShouldEqual("application/x-zip-co");
            fromDb.Filename.ShouldEqual("file");
            fromDb.Extension.ShouldEqual(".zip");
            fromDb.IsNew.ShouldEqual(true);
        }
    }
}
