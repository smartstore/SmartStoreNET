using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;
using SmartStore.Data.Utilities;
using SmartStore.Services.Media.Storage;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media.Migration
{
    public class MediaMigrator
    {
        private readonly ICommonServices _services;
        private readonly IProviderManager _providerManager;

        public MediaMigrator(ICommonServices services, IProviderManager providerManager)
        {
            _services = services;
            _providerManager = providerManager;
        }

        public void Migrate()
        {
            var ctx = _services.DbContext;

            var storageProviderSystemName = _services.Settings.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
            var mediaStorageProvider = _providerManager.GetProvider<IMediaStorageProvider>(storageProviderSystemName).Value;

            var query = ctx.Set<MediaFile>()
                .Where(x => x.Version == 0)
                .Include(x => x.MediaStorage);

            var pager = new FastPager<MediaFile>(query, 1000);

            using (var scope = new DbContextScope(ctx, hooksEnabled: false, autoCommit: false))
            {
                while (pager.ReadNextPage(out var files))
                {
                    foreach (var file in files)
                    {
                        var mediaItem = file.ToMedia();

                        file.Extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
                        file.Name = file.Name + "." + file.Extension;
                        file.MediaType = MediaType.Image;
                        file.CreatedOnUtc = file.UpdatedOnUtc;

                        if (file.Size == 0)
                        {
                            file.Size = mediaStorageProvider.GetSize(mediaItem).Convert<int>();
                        }         

                        if (file.Width.HasValue && file.Height.HasValue)
                        {
                            file.PixelSize = file.Width.Value * file.Height.Value;
                            // TODO: Metadata JSON
                        }

                        file.Version = 1;
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
