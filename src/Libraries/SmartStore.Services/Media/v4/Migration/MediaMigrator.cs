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
using SmartStore.Data.Setup;
using SmartStore.Services.Media.Storage;
using SmartStore.Core.IO;
using SmartStore.Data;

namespace SmartStore.Services.Media.Migration
{
    public class MediaMigrator
    {
        internal static bool Executed;
        
        private readonly ICommonServices _services;
        private readonly IProviderManager _providerManager;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly IAlbumRegistry _albumRegistry;
        //private readonly IAlbumService _albumService;
        private readonly IMediaTracker _mediaTracker;

        public MediaMigrator(
            ICommonServices services, 
            IProviderManager providerManager,
            IMediaTypeResolver mediaTypeResolver,
            IAlbumRegistry albumRegistry,
            //IAlbumService albumService,
            IMediaTracker mediaTracker)
        {
            _services = services;
            _providerManager = providerManager;
            _mediaTypeResolver = mediaTypeResolver;
            _albumRegistry = albumRegistry;
            //_albumService = albumService;
            _mediaTracker = mediaTracker;
        }

        public void Migrate()
        {
            var ctx = _services.DbContext as SmartObjectContext;

            CreateAlbums();
            CreateSettings(ctx);
            MigratePictures(ctx);
            DetectTracks(ctx);

            Executed = true;
        }

        public void CreateSettings(SmartObjectContext ctx)
        {
            var prefix = nameof(MediaSettings) + ".";

            ctx.MigrateSettings(x =>
            {
                x.Add(prefix + nameof(MediaSettings.ImageTypes), MediaType.Image.DefaultExtensions);
                x.Add(prefix + nameof(MediaSettings.VideoTypes), MediaType.Video.DefaultExtensions);
                x.Add(prefix + nameof(MediaSettings.AudioTypes), MediaType.Audio.DefaultExtensions);
                x.Add(prefix + nameof(MediaSettings.DocumentTypes), MediaType.Document.DefaultExtensions);
                x.Add(prefix + nameof(MediaSettings.TextTypes), MediaType.Text.DefaultExtensions);
            });
        }

        public void CreateAlbums()
        {
            // Enforce full album registration
            _albumRegistry.GetAllAlbums();
        }

        public void MigratePictures(SmartObjectContext ctx)
        {
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

                        file.MediaType = _mediaTypeResolver.Resolve(file);

                        file.Version = 1;
                    }

                    // Save to DB
                    scope.Commit();

                    // Breathe
                    ctx.DetachEntities(x => x is MediaFile || x is MediaStorage, false);
                }
            }
        }

        public void DetectTracks(SmartObjectContext ctx)
        {
            foreach (var albumName in _albumRegistry.GetAlbumNames(true))
            {
                _mediaTracker.DetectAllTracks(albumName, true);
            }
        }
    }
}
