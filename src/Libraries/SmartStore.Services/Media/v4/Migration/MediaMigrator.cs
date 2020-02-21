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
        private readonly IMediaFolderService _mediaFolderService;

        public MediaMigrator(
            ICommonServices services, 
            IProviderManager providerManager,
            IMediaTypeResolver mediaTypeResolver,
            IMediaFolderService mediaFolderService)
        {
            _services = services;
            _providerManager = providerManager;
            _mediaTypeResolver = mediaTypeResolver;
            _mediaFolderService = mediaFolderService;
        }

        public void Migrate()
        {
            var ctx = _services.DbContext as SmartObjectContext;

            CreateAlbums();
            CreateSettings(ctx);
            MigratePictures(ctx);
            DetectRelations(ctx);

            Executed = true;
        }

        private void CreateSettings(SmartObjectContext ctx)
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

        private void CreateAlbums()
        {
            var providers = _mediaFolderService.LoadAllAlbumProviders();
            _mediaFolderService.InstallAlbums(providers);
        }

        private void MigratePictures(SmartObjectContext ctx)
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

        private void DetectRelations(SmartObjectContext ctx)
        {
            // Get all installed album names...
            var albumNames = _mediaFolderService.GetAlbumNames(true);

            using (var scope = new DbContextScope(ctx, validateOnSave: false, hooksEnabled: false))
            {
                foreach (var albumName in albumNames)
                {
                    // get the id for an album (necessary later to set FolderId)
                    var albumId = _mediaFolderService.GetAlbumIdByName(albumName);

                    // load corresponding detector provider for current album...
                    var provider = _mediaFolderService.LoadAlbumProvider(albumName) as IMediaRelationDetector;

                    // >>>>> DO detection (potentially a very long process)...
                    var relations = provider.DetectAllRelations(albumName);

                    // (perf) batch result data...
                    foreach (var batch in relations.Slice(500))
                    {
                        // process the batch
                        ProcessRelationsBatch(ctx, batch, albumId, albumName);
                    }
                }
            }
        }

        private void ProcessRelationsBatch(SmartObjectContext ctx, IEnumerable<MediaRelation> batch, int albumId, string albumName)
        {
            // Get distinct ids of all detected files...
            var mediaFileIds = batch.Select(x => x.MediaFileId).Distinct().ToArray();

            // fetch these files from database...
            var files = ctx.Set<MediaFile>()
                .Where(x => mediaFileIds.Contains(x.Id))
                .Include(x => x.Relations)
                .ToDictionary(x => x.Id);

            // for each media file relation to an entity...
            foreach (var relation in batch)
            {
                // fetch the file from local dictionary by its id...
                if (files.TryGetValue(relation.MediaFileId, out var file))
                {
                    // set album id as folder id (during initial migration there are no sub-folders)
                    file.FolderId = albumId;

                    // set album name...
                    relation.Album = albumName;

                    // add the relation to the file entity
                    file.Relations.Add(relation);
                }
            }

            // Save whole batch to database
            ctx.SaveChanges();

            // Breathe
            ctx.DetachEntities(x => x is MediaFile || x is MediaRelation, false);
        }
    }
}
