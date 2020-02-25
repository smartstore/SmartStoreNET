using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
        private readonly IMediaStorageProvider _mediaStorageProvider;
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly bool _isFsProvider;

        public MediaMigrator(
            ICommonServices services, 
            IProviderManager providerManager,
            IMediaTypeResolver mediaTypeResolver,
            IAlbumRegistry albumRegistry,
            //IAlbumService albumService,
            IMediaTracker mediaTracker,
            IMediaFileSystem mediaFileSystem)
        {
            _services = services;
            _providerManager = providerManager;
            _mediaTypeResolver = mediaTypeResolver;
            _albumRegistry = albumRegistry;
            //_albumService = albumService;
            _mediaTracker = mediaTracker;
            _mediaFileSystem = mediaFileSystem;

            var storageProviderSystemName = _services.Settings.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
            _mediaStorageProvider = _providerManager.GetProvider<IMediaStorageProvider>(storageProviderSystemName).Value;
            _isFsProvider = _mediaStorageProvider is FileSystemMediaStorageProvider;
        }

        public void Migrate()
        {
            var ctx = _services.DbContext as SmartObjectContext;

            CreateAlbums();
            CreateSettings(ctx);
            MigrateDownloads(ctx);
            MigrateFiles(ctx);
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

        public void MigrateDownloads(SmartObjectContext ctx)
        {
            var sql = "SELECT * FROM [Download] WHERE [MediaFileId] IS NOT NULL";
            var downloadStubs = ctx.SqlQuery<DownloadStub>(sql).ToDictionary(x => x.Id);
            var folderId = _albumRegistry.GetAlbumByName(SystemAlbumProvider.Downloads)?.Id;

            using (var scope = new DbContextScope(ctx, 
                validateOnSave: false, 
                hooksEnabled: false, 
                autoCommit: false, 
                autoDetectChanges: false))
            {
                var query = ctx.Set<Download>().Where(x => x.MediaFileId == null && !x.UseDownloadUrl);
                var pager = new FastPager<Download>(query, 250);

                while (pager.ReadNextPage(out var downloads))
                {
                    foreach (var d in downloads)
                    {
                        var stub = downloadStubs.Get(d.Id);
                        if (stub == null)
                            continue;

                        // Create and insert new MediaFile entity for the download
                        var file = new MediaFile
                        {
                            CreatedOnUtc = stub.UpdatedOnUtc,
                            UpdatedOnUtc = stub.UpdatedOnUtc,
                            Extension = stub.Extension.TrimStart('.'),
                            Name = stub.Filename, // Extension appended later in MigrateFiles()
                            MimeType = stub.ContentType,
                            MediaType = MediaType.Image, // Resolved later in MigrateFiles()
                            FolderId = folderId,
                            IsNew = stub.IsNew,
                            IsTransient = stub.IsTransient,
                            MediaStorageId = stub.MediaStorageId,
                            Version = 0 // Ensure that this record gets processed by MigrateFiles()
                        };

                        // Add a track for the new file
                        file.Tracks.Add(new MediaTrack 
                        { 
                            Album = SystemAlbumProvider.Downloads, 
                            EntityId = d.Id, 
                            EntityName = d.GetEntityName()
                        });

                        // Assign new file to download
                        d.MediaFile = file;

                        if (_isFsProvider)
                        {
                            // Copy file from "Media/Downloads" to "Media/Storage" folder
                        }
                    }

                    // Save to DB
                    scope.Commit();

                    // Breathe
                    ctx.DetachEntities(x => x is Download || x is MediaFile || x is MediaStorage || x is MediaTrack, false);
                }
            }
        }

        public void MigrateFiles(SmartObjectContext ctx)
        {
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

                        if (file.Extension.IsEmpty())
                        {
                            file.Extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
                        }
                        
                        file.Name = file.Name + "." + file.Extension;
                        file.CreatedOnUtc = file.UpdatedOnUtc;

                        if (file.Size == 0)
                        {
                            file.Size = _mediaStorageProvider.GetSize(mediaItem).Convert<int>();
                        }

                        file.MediaType = _mediaTypeResolver.Resolve(file);

                        if (file.MediaType == MediaType.Image && file.Width == null && file.Height == null)
                        {
                            // Resolve image width and height
                            var stream = _mediaStorageProvider.OpenRead(mediaItem);
                            if (stream != null)
                            {
                                try
                                {
                                    var size = ImageHeader.GetDimensions(stream, file.MimeType, true);
                                    file.Width = size.Width;
                                    file.Height = size.Height;
                                }
                                finally
                                {
                                    stream.Dispose();
                                }
                            }
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

        public void DetectTracks(SmartObjectContext ctx)
        {
            foreach (var albumName in _albumRegistry.GetAlbumNames(true))
            {
                if (albumName == SystemAlbumProvider.Downloads)
                    continue; // Download tracks already added in MigrateDownload()
                
                _mediaTracker.DetectAllTracks(albumName, true);
            }
        }

        public class DownloadStub
        {
            public int Id { get; set; }
            public string ContentType { get; set; }
            public string Filename { get; set; }
            public string Extension { get; set; }
            public bool IsNew { get; set; }
            public bool IsTransient { get; set; }
            public DateTime UpdatedOnUtc { get; set; }
            public int? MediaStorageId { get; set; }
        }
    }
}
