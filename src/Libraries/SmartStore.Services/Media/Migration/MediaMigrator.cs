using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using System.Diagnostics;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media.Migration
{
    public class MediaMigrator
    {
        public class FileMin
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsDupe { get; set; }
            public string UniqueName { get; set; }
            public int UniqueIndex { get; set; }
        }

        internal static bool Executed;
        internal const string MigrationName = "MediaManager";

        private readonly ICommonServices _services;
        private readonly IProviderManager _providerManager;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly IAlbumRegistry _albumRegistry;
        private readonly IFolderService _folderService;
        private readonly IMediaTracker _mediaTracker;
        private readonly IMediaStorageProvider _mediaStorageProvider;
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly IImageCache _imageCache;
        private readonly bool _isFsProvider;

        public MediaMigrator(
            ICommonServices services,
            IProviderManager providerManager,
            Func<IMediaStorageProvider> mediaStorageProvider,
            IMediaTypeResolver mediaTypeResolver,
            IAlbumRegistry albumRegistry,
            IFolderService folderService,
            IMediaTracker mediaTracker,
            IImageCache imageCache,
            IMediaFileSystem mediaFileSystem)
        {
            _services = services;
            _providerManager = providerManager;
            _mediaTypeResolver = mediaTypeResolver;
            _albumRegistry = albumRegistry;
            _folderService = folderService;
            _mediaTracker = mediaTracker;
            _mediaFileSystem = mediaFileSystem;
            _imageCache = imageCache;
            _mediaStorageProvider = mediaStorageProvider();
            _isFsProvider = _mediaStorageProvider is FileSystemMediaStorageProvider;
        }

        public void Migrate()
        {
            var ctx = _services.DbContext as SmartObjectContext;

            // We're going to add new hooked entities, but during migration
            // we don't need any hooking.
            MediaTrackerHook.Silent = true;

            long elapsed = 0;
            var watch = new Stopwatch();

            try
            {
                Execute("CreateAlbums", () => CreateAlbums());
                Execute("CreateSettings", () => CreateSettings(ctx));
                Execute("MigrateDownloads", () => MigrateDownloads(ctx));
                Execute("MigrateMediaFiles", () => MigrateMediaFiles(ctx));
                Execute("MigrateUploadedFiles", () => MigrateUploadedFiles(ctx));
                Execute("DetectTracks", () => DetectTracks());
                Execute("ClearImageCache", () => ClearImageCache());

                _folderService.ClearCache();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                MediaTrackerHook.Silent = false;
                Executed = true;
            }

            void Execute(string step, Action action)
            {
                watch.Start();
                action();
                watch.Stop();
                var time = watch.ElapsedMilliseconds - elapsed;
                var str = "MediaMigrator > {0}: {1} ms.".FormatCurrent(step, time);
                elapsed = watch.ElapsedMilliseconds;
                Debug.WriteLine(str);
            }
        }

        public void CreateSettings(SmartObjectContext ctx)
        {
            var prefix = nameof(MediaSettings) + ".";

            ctx.MigrateSettings(x =>
            {
                x.Add(prefix + nameof(MediaSettings.ImageTypes), string.Join(" ", MediaType.Image.DefaultExtensions));
                x.Add(prefix + nameof(MediaSettings.VideoTypes), string.Join(" ", MediaType.Video.DefaultExtensions));
                x.Add(prefix + nameof(MediaSettings.AudioTypes), string.Join(" ", MediaType.Audio.DefaultExtensions));
                x.Add(prefix + nameof(MediaSettings.DocumentTypes), string.Join(" ", MediaType.Document.DefaultExtensions));
                x.Add(prefix + nameof(MediaSettings.TextTypes), string.Join(" ", MediaType.Text.DefaultExtensions));
            });
        }

        public void CreateAlbums()
        {
            // Enforce full album registration
            _albumRegistry.GetAllAlbums();
        }

        public void MigrateDownloads(SmartObjectContext ctx)
        {
            var sql = "SELECT * FROM [Download] WHERE [MediaFileId] IS NULL AND [UseDownloadUrl] = 0";
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

                var query = ctx.Set<Download>().Where(x => x.MediaFileId == null);
                var pager = new FastPager<Download>(query, 1000);

                while (pager.ReadNextPage(out var downloads))
                {
                    foreach (var d in downloads)
                    {
                        var stub = downloadStubs.Get(d.Id);
                        if (stub == null)
                            continue;

                        if (stub.UseDownloadUrl || string.IsNullOrEmpty(stub.Extension))
                        {
                            // Something weird has happened in the past
                            continue;
                        }

                        if (stub.Filename == "undefined" || string.IsNullOrEmpty(stub.Filename))
                        {
                            stub.Filename = stub.Id.ToString(CultureInfo.InvariantCulture);
                        }

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
                            Name = stub.Filename.Truncate(292), // Extension appended later in MigrateFiles()
                            MimeType = stub.ContentType,
                            MediaType = MediaType.Image, // Resolved later in MigrateFiles()
                            FolderId = isMailAttachment ? messagesFolderId : downloadsFolderId,
                            IsTransient = stub.IsTransient,
                            MediaStorageId = stub.MediaStorageId,
                            Version = 0 // Ensure that this record gets processed by MigrateFiles()
                        };

                        // Assign new file to download
                        d.MediaFile = file;

                        // To be able to move files later
                        if (hasPostProcessor)
                        {
                            newFiles.Add(file);
                        }
                    }

                    // Save to DB
                    int num = scope.Commit();

                    if (hasPostProcessor)
                    {
                        var downloadsDict = downloads.ToDictionary(x => x.Id);

                        if (_isFsProvider)
                        {
                            // Copy files from "Media/Downloads" to "Media/Storage" folder
                            MoveDownloadFiles(newFiles.ToDictionary(x => x.Id), downloadsDict, downloadStubs);
                        }

                        // MessageTemplate attachments (Download > MediaFile)
                        if (messageTemplatesDict.Count > 0)
                        {
                            ReRefMessageTemplateAttachments(ctx, messageTemplatesDict, downloadsDict);
                        }

                        newFiles.Clear();
                    }

                    // Breathe
                    ctx.DetachEntities<MessageTemplate>();
                    ctx.DetachEntities<Download>(deep: true);
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
                var idxProp = Array.IndexOf(new int?[] { mt.Attachment1FileId, mt.Attachment2FileId, mt.Attachment3FileId }, downloadId) + 1;

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
            Dictionary<int, Download> downloadsDict,
            Dictionary<int, DownloadStub> downloadStubs)
        {
            if (_mediaFileSystem.FolderExists("Downloads"))
            {
                var downloadFiles = _mediaFileSystem.ListFiles("Downloads");
                foreach (var downloadFile in downloadFiles)
                {
                    if (int.TryParse(downloadFile.Title, out var downloadId) && downloadId > 0 && downloadsDict.TryGetValue(downloadId, out var d))
                    {
                        var stub = downloadStubs.Get(d.Id);
                        if (stub == null || d.MediaFileId == null)
                            continue;

                        var file = newFilesDict.Get(d.MediaFileId.Value);
                        if (file != null)
                        {
                            try
                            {
                                // Copy now
                                var newPath = GetStoragePath(file);
                                if (!_mediaFileSystem.FileExists(newPath))
                                {
                                    _mediaFileSystem.CopyFile(downloadFile.Path, newPath);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        public void MigrateMediaFiles(SmartObjectContext ctx)
        {
            string prevName = null;
            int fileIndex = 0;

            var query = ctx.Set<MediaFile>().OrderBy(x => x.Name);
            var totalCount = query.Count();
            var pageSize = Math.Max(1000, Math.Min(5000, totalCount / 200));
            var pageIndex = 0;

            using (var scope = new DbContextScope(ctx,
                hooksEnabled: false,
                autoCommit: false,
                proxyCreation: false,
                validateOnSave: false,
                lazyLoading: false))
            {
                while (true)
                {
                    var files = new PagedList<MediaFile>(query, pageIndex, pageSize);
                    if (files.Count == 0)
                        break;

                    foreach (var file in files)
                    {
                        if (file.Version > 0)
                            continue;

                        if (file.Extension.IsEmpty())
                        {
                            file.Extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
                        }

                        var name = file.Name;
                        var fixedName = name;
                        if (name.IsEmpty())
                        {
                            name = fixedName = file.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            name = fixedName = MediaHelper.NormalizeFileName(file.Name.Truncate(292));
                            if (name == prevName)
                            {
                                // Make file name unique
                                fixedName = name + "-" + ++fileIndex;
                            }
                            else
                            {
                                fileIndex = 0;
                            }
                        }

                        prevName = name;

                        file.Name = fixedName + "." + file.Extension;
                        file.CreatedOnUtc = file.UpdatedOnUtc;
                        file.Version = 1;

                        ProcessMediaFile(file);
                    }

                    // Save to DB
                    int num = scope.Commit();

                    // Breathe
                    ctx.DetachEntities<MediaFile>(deep: true);

                    if (!files.HasNextPage)
                        break;

                    pageIndex++;
                }
            }
        }

        public void MigrateMediaFiles_Old(SmartObjectContext ctx)
        {
            var query = ctx.Set<MediaFile>();
            //.Where(x => x.Version == 0)
            //.Include(x => x.MediaStorage);

            var pager = new FastPager<MediaFile>(query, 1000);

            using (var scope = new DbContextScope(ctx,
                hooksEnabled: false,
                autoCommit: false,
                proxyCreation: false,
                validateOnSave: false,
                lazyLoading: false))
            {
                while (pager.ReadNextPage(out var files))
                {
                    foreach (var file in files)
                    {
                        if (file.Version > 0)
                            continue;

                        if (file.Extension.IsEmpty())
                        {
                            file.Extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
                        }

                        var name = file.Name;
                        if (name.IsEmpty())
                        {
                            name = file.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            name = MediaHelper.NormalizeFileName(file.Name.Truncate(292));
                        }

                        file.Name = name + "." + file.Extension;
                        file.CreatedOnUtc = file.UpdatedOnUtc;
                        file.Version = 1;

                        ProcessMediaFile(file);
                    }

                    // Save to DB
                    int num = scope.Commit();

                    // Breathe
                    ctx.DetachEntities<MediaFile>(deep: true);
                }
            }
        }

        public void MigrateUploadedFiles(SmartObjectContext ctx)
        {
            var fileSet = ctx.Set<MediaFile>();
            var folderSet = ctx.Set<MediaFolder>();

            using (var scope = new DbContextScope(ctx,
                hooksEnabled: false,
                autoCommit: false,
                validateOnSave: false,
                lazyLoading: false,
                autoDetectChanges: false))
            {

                var albumId = _albumRegistry.GetAlbumByName(SystemAlbumProvider.Files)?.Id;
                var rootFolder = _mediaFileSystem.GetFolder("Uploaded");
                if (!rootFolder.Exists)
                    return;

                ProcessFolder(rootFolder, albumId.Value);

                void ProcessFolder(IFolder folder, int mediaFolderId)
                {
                    var newFiles = new List<FilePair>();

                    foreach (var uploadedFile in _mediaFileSystem.ListFiles(folder.Path))
                    {
                        var file = new MediaFile
                        {
                            CreatedOnUtc = uploadedFile.LastUpdated,
                            UpdatedOnUtc = uploadedFile.LastUpdated,
                            Extension = uploadedFile.Extension.TrimStart('.'),
                            Name = uploadedFile.Name,
                            MimeType = MimeTypes.MapNameToMimeType(uploadedFile.Name),
                            Size = Convert.ToInt32(uploadedFile.Size),
                            FolderId = mediaFolderId,
                            Version = 2
                        };

                        ProcessMediaFile(file);

                        newFiles.Add(new FilePair { MediaFile = file, UploadedFile = uploadedFile });
                        fileSet.Add(file);
                    }

                    // Process/save files of current folder
                    try
                    {
                        // Save files to DB
                        int num = scope.Commit();

                        // Copy/Move files
                        foreach (var newFile in newFiles)
                        {
                            if (_isFsProvider)
                            {
                                var newPath = GetStoragePath(newFile.MediaFile);
                                if (!_mediaFileSystem.FileExists(newPath))
                                {
                                    // TODO: (mm) (mc) should we actually MOVE the file?
                                    _mediaFileSystem.CopyFile(newFile.UploadedFile.Path, newPath);
                                }
                            }
                            else
                            {
                                _mediaStorageProvider.Save(newFile.MediaFile, MediaStorageItem.FromFile(newFile.UploadedFile));
                            }
                        }

                        if (!_isFsProvider)
                        {
                            // MediaFile.MediaStorageId has been updated, we need to save again.
                            num = scope.Commit();
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        newFiles.Clear();

                        // Breathe
                        ctx.DetachEntities<MediaFile>(deep: true);
                    }

                    foreach (var uploadedFolder in _mediaFileSystem.ListFolders(folder.Path))
                    {
                        var mediaFolder = new MediaFolder
                        {
                            Name = uploadedFolder.Name,
                            ParentId = mediaFolderId
                        };

                        // Add folder and save ASAP, we need the folder id
                        folderSet.Add(mediaFolder);
                        ctx.SaveChanges();

                        ProcessFolder(uploadedFolder, mediaFolder.Id);
                    }
                }
            }
        }

        private void ProcessMediaFile(MediaFile file)
        {
            if (file.Size == 0)
            {
                file.Size = Convert.ToInt32(_mediaStorageProvider.GetSize(file));
            }

            file.MediaType = _mediaTypeResolver.Resolve(file);

            if (file.MediaType == MediaType.Image && file.Width == null && file.Height == null)
            {
                // Resolve image width and height
                var stream = _mediaStorageProvider.OpenRead(file);
                if (stream != null)
                {
                    try
                    {
                        var size = ImageHeader.GetDimensions(stream, file.MimeType, true);
                        file.Width = size.Width;
                        file.Height = size.Height;
                        file.PixelSize = size.Width * size.Height;
                    }
                    catch
                    {
                        // Don't attempt again
                        file.Width = 0;
                        file.Height = 0;
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
        }

        public void DetectTracks()
        {
            foreach (var albumName in _albumRegistry.GetAlbumNames(true))
            {
                //if (albumName == SystemAlbumProvider.Downloads || albumName == SystemAlbumProvider.Messages)
                //    continue; // Download and MessageTemplate tracks already added in MigrateDownload()

                _mediaTracker.DetectAllTracks(albumName, true);
            }
        }

        public void ClearImageCache()
        {
            _imageCache.Clear();
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

        private string GetStoragePath(MediaFile file, bool createFolder = true)
        {
            var fileName = BuildFileName(file.Id, file.Extension, file.MimeType);
            var subfolder = _mediaFileSystem.Combine("Storage", fileName.Substring(0, ImageCache.MaxDirLength));

            if (createFolder)
            {
                _mediaFileSystem.CreateFolder(subfolder);
            }

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
            public bool UseDownloadUrl { get; set; }
            public string ContentType { get; set; }
            public string Filename { get; set; }
            public string Extension { get; set; }
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

        class FilePair
        {
            public MediaFile MediaFile { get; set; }
            public IFile UploadedFile { get; set; }
        }
    }
}
