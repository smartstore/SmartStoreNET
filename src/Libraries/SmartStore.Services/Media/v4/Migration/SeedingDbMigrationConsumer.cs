using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Data;
using System.Data.Entity;
using SmartStore.Data.Setup;
using SmartStore.Data.Utilities;
using SmartStore.Core.Data;
using SmartStore.Services.Media.Storage;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Media.Migration
{
    public class SeedingDbMigrationConsumer : IConsumer
    {
        public void Handle(
            SeedingDbMigrationEvent message, 
            IProviderManager providerManager, 
            ISettingService settingService)
        {
            if (message.MigrationName != "MediaFileExtend")
                return;

            var ctx = message.DbContext as SmartObjectContext;

            var storageProviderSystemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
            var mediaStorageProvider = providerManager.GetProvider<IMediaStorageProvider>(storageProviderSystemName).Value;

            var query = ctx.Set<MediaFile>().Include(x => x.MediaStorage);
            var pager = new FastPager<MediaFile>(query, 1000);

            using (var scope = new DbContextScope(ctx, hooksEnabled: false, autoCommit: false))
            {
                while (pager.ReadNextPage(out var page))
                {
                    foreach (var file in page)
                    {
                        var mediaItem = file.ToMedia();

                        file.Extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
                        file.Name = file.Name + "." + file.Extension;
                        file.MediaType = MediaType.Image;
                        file.CreatedOnUtc = file.UpdatedOnUtc;
                        file.Size = mediaStorageProvider.GetSize(mediaItem).Convert<int>();

                        if (file.Width.HasValue && file.Height.HasValue)
                        {
                            file.PixelSize = file.Width.Value * file.Height.Value;
                            // TODO: Metadata JSON
                        }
                    }

                    // Save to DB
                    scope.Commit();

                    // Breathe
                    ctx.DetachEntities(x => x is MediaFile || x is MediaStorage, false);
                }
            }
        }
    }
}
