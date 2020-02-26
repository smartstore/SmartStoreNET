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
using System.Runtime.CompilerServices;
using SmartStore.Core.Domain.Messages;

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

            var downloadsFolderId = _albumRegistry.GetAlbumByName(SystemAlbumProvider.Downloads)?.Id;
            var messagesFolderId = _albumRegistry.GetAlbumByName(SystemAlbumProvider.Messages)?.Id;
            var newFiles = new List<MediaFile>();

            using (var scope = new DbContextScope(ctx, 
                validateOnSave: false, 
                hooksEnabled: false, 
                autoCommit: false, 
                autoDetectChanges: false))
            {
                var messageTemplates = ctx.Set<MessageTemplate>()
                    .Where(x => x.Attachment1FileId.HasValue || x.Attachment2FileId.HasValue || x.Attachment3FileId.HasValue)
                    .ToList();

                // Key = Download.Id
                var messageTemplatesDict = new Dictionary<int, MessageTemplate>();
                foreach (var mt in messageTemplates)
                {
                    if (mt.Attachment1FileId.HasValue) messageTemplatesDict[mt.Attachment1FileId.Value] = mt;
                    if (mt.Attachment2FileId.HasValue) messageTemplatesDict[mt.Attachment2FileId.Value] = mt;
                    if (mt.Attachment3FileId.HasValue) messageTemplatesDict[mt.Attachment3FileId.Value] = mt;
                }

                var hasPostProcessor = _isFsProvider || messageTemplatesDict.Count > 0;

                var query = ctx.Set<Download>().Where(x => x.MediaFileId == null && !x.UseDownloadUrl);
                var pager = new FastPager<Download>(query, 250);

                while (pager.ReadNextPage(out var downloads))
                {
                    foreach (var d in downloads)
                    {
                        var stub = downloadStubs.Get(d.Id);
                        if (stub == null)
                            continue;

                        var isMailAttachment = false;
                        if (messageTemplatesDict.TryGetValue(stub.Id, out var mt))
                        {
                            isMailAttachment = true;
                        }

                        // Create and insert new MediaFile entity for the download
                        var file = new MediaFile
                        {
                            CreatedOnUtc = stub.UpdatedOnUtc,
                            UpdatedOnUtc = stub.UpdatedOnUtc,
                            Extension = stub.Extension.TrimStart('.'),
                            Name = stub.Filename, // Extension appended later in MigrateFiles()
                            MimeType = stub.ContentType,
                            MediaType = MediaType.Image, // Resolved later in MigrateFiles()
                            FolderId = isMailAttachment ? messagesFolderId : downloadsFolderId,
                            IsNew = stub.IsNew,
                            IsTransient = stub.IsTransient,
                            MediaStorageId = stub.MediaStorageId,
                            Version = 0 // Ensure that this record gets processed by MigrateFiles()
                        };

                        // Add a track for the new file
                        if (mt != null)
                        {
                            // Is referenced by a message template: move file to "Messages" album
                            file.Tracks.Add(new MediaTrack
                            {
                                Album = SystemAlbumProvider.Messages,
                                EntityId = mt.Id,
                                EntityName = mt.GetEntityName()
                            });
                        }
                        else
                        {
                            file.Tracks.Add(new MediaTrack
                            {
                                Album = SystemAlbumProvider.Downloads,
                                EntityId = d.Id,
                                EntityName = d.GetEntityName()
                            });
                        }

                        // Assign new file to download
                        d.MediaFile = file;

                        // To be able to move files later
                        if (hasPostProcessor)
                        {
                            newFiles.Add(file);
                        } 
                    }

                    // Save to DB
                    scope.Commit();

                    if (hasPostProcessor)
                    {
                        // MessageTemplate attachments (Download > MediaFile)
                        if (messageTemplatesDict.Count > 0)
                        {
                            ReRefMessageTemplateAttachments(ctx, messageTemplatesDict, downloads.ToDictionary(x => x.Id));
                        }

                        if (_isFsProvider)
                        {
                            // Copy files from "Media/Downloads" to "Media/Storage" folder
                            MoveDownloadFiles(newFiles.ToDictionary(x => x.Id), downloads, downloadStubs);
                        }

                        newFiles.Clear();
                    }

                    // Breathe
                    ctx.DetachEntities(x => x is Download || x is MediaFile || x is MediaStorage || x is MediaTrack, false);
                }
            }
        }

        private void ReRefMessageTemplateAttachments(
            SmartObjectContext ctx,
            Dictionary<int, MessageTemplate> messageTemplatesDict, 
            Dictionary<int, Download> downloads)
        {
            bool hasChanges = false;
            
            foreach (var kvp in messageTemplatesDict)
            {
                var downloadId = kvp.Key;
                var mt = kvp.Value;
                var idxProp = Array.IndexOf(messageTemplatesDict.Select(x => new int?[] { mt.Attachment1FileId, mt.Attachment2FileId, mt.Attachment3FileId }).ToArray(), downloadId) + 1;

                if (idxProp > 0)
                {
                    var d = downloads.Get(downloadId);
                    if (d?.MediaFileId != null)
                    {
                        // Change Download.Id ref to MediaFile.Id
                        if (idxProp == 1) mt.Attachment1FileId = d.MediaFileId;
                        if (idxProp == 2) mt.Attachment2FileId = d.MediaFileId;
                        if (idxProp == 3) mt.Attachment3FileId = d.MediaFileId;

                        // We don't need Download entity anymore
                        ctx.Set<Download>().Remove(d);

                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                ctx.SaveChanges();
            }
        }

        private void MoveDownloadFiles(
            Dictionary<int, MediaFile> newFilesDict, 
            IList<Download> downloads, 
            Dictionary<int, DownloadStub> downloadStubs)
        {
            // Copy files from "Media/Downloads" to "Media/Storage" folder
            foreach (var d in downloads)
            {
                var stub = downloadStubs.Get(d.Id);
                if (stub == null)
                    continue;

                var oldPath = GetStoragePath(stub);
                if (d.MediaFileId.HasValue)
                {
                    var file = newFilesDict.Get(d.MediaFileId.Value);
                    if (file != null)
                    {
                        var newPath = GetStoragePath(file);

                        try
                        {
                            // Copy now
                            _mediaFileSystem.CopyFile(oldPath, newPath);
                        }
                        catch { }
                    }
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
                if (albumName == SystemAlbumProvider.Downloads || albumName == SystemAlbumProvider.Messages)
                    continue; // Download and MessageTemplate tracks already added in MigrateDownload()
                
                _mediaTracker.DetectAllTracks(albumName, true);
            }
        }

        private string GetStoragePath(DownloadStub stub)
        {
            var fileName = BuildFileName(stub.Id, stub.Extension, stub.ContentType);
            return _mediaFileSystem.Combine("Downloads", fileName);
        }

        private string GetStoragePath(QueuedEmailAttachmentStub stub)
        {
            var fileName = BuildFileName(stub.Id, Path.GetExtension(stub.Name), stub.MimeType);
            return _mediaFileSystem.Combine("QueuedEmailAttachment", fileName);
        }

        private string GetStoragePath(MediaFile file)
        {
            var fileName = BuildFileName(file.Id, file.Extension, file.MimeType);
            var subfolder = _mediaFileSystem.Combine("Storage", fileName.Substring(0, ImageCache.MaxDirLength));
            _mediaFileSystem.TryCreateFolder(subfolder);

            return _mediaFileSystem.Combine(subfolder, fileName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string BuildFileName(int id, string ext, string mime)
        {
            if (ext.IsEmpty())
                ext = MimeTypes.MapMimeTypeToExtension(mime);

            return id.ToString(ImageCache.IdFormatString) + "." + ext.EmptyNull().TrimStart('.');
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

        public class QueuedEmailAttachmentStub
        {
            public int Id { get; set; }
            public int QueuedEmailId { get; set; }
            public string MimeType { get; set; }
            public string Name { get; set; }
            public int? MediaStorageId { get; set; }
            public int? FileId { get; set; }
        }
    }
}
