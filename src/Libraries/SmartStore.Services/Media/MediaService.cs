using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Data.Caching;
using SmartStore.Services.Localization;
using SmartStore.Services.Media.Imaging;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class MediaService : IMediaService
    {
        private readonly IRepository<MediaFile> _fileRepo;
        private readonly IFolderService _folderService;
        private readonly IMediaSearcher _searcher;
        private readonly IMediaTypeResolver _typeResolver;
        private readonly IMediaUrlGenerator _urlGenerator;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageCache _imageCache;
        private readonly MediaExceptionFactory _exceptionFactory;
        private readonly IMediaStorageProvider _storageProvider;
        private readonly MediaHelper _helper;

        public MediaService(
            IRepository<MediaFile> fileRepo,
            IFolderService folderService,
            IMediaSearcher searcher,
            IMediaTypeResolver typeResolver,
            IMediaUrlGenerator urlGenerator,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            MediaSettings mediaSettings,
            IImageProcessor imageProcessor,
            IImageCache imageCache,
            MediaExceptionFactory exceptionFactory,
            Func<IMediaStorageProvider> storageProvider,
            MediaHelper helper)
        {
            _fileRepo = fileRepo;
            _folderService = folderService;
            _searcher = searcher;
            _typeResolver = typeResolver;
            _urlGenerator = urlGenerator;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _mediaSettings = mediaSettings;
            _imageProcessor = imageProcessor;
            _imageCache = imageCache;
            _exceptionFactory = exceptionFactory;
            _storageProvider = storageProvider();
            _helper = helper;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public IMediaStorageProvider StorageProvider => _storageProvider;

        public bool ImagePostProcessingEnabled { get; set; } = true;

        #region Query

        public int CountFiles(MediaSearchQuery query)
        {
            Guard.NotNull(query, nameof(query));

            var q = _searcher.PrepareQuery(query, MediaLoadFlags.None);
            var count = q.Count();
            return count;
        }

        public Task<int> CountFilesAsync(MediaSearchQuery query)
        {
            Guard.NotNull(query, nameof(query));

            var q = _searcher.PrepareQuery(query, MediaLoadFlags.None);
            return q.CountAsync();
        }

        public FileCountResult CountFilesGrouped(MediaFilesFilter filter)
        {
            Guard.NotNull(filter, nameof(filter));

            // Base db query
            var q = _searcher.ApplyFilterQuery(filter);

            // Get ids of untrackable folders, 'cause no orphan check can be made for them.
            var untrackableFolderIds = _folderService.GetRootNode()
                .SelectNodes(x => !x.Value.CanDetectTracks)
                .Select(x => x.Value.Id)
                .ToArray();

            // Determine counts
            var result = (from f in q
                          group f by 1 into g
                          select new FileCountResult
                          {
                              Total = g.Count(),
                              Trash = g.Count(x => x.Deleted),
                              Unassigned = g.Count(x => !x.Deleted && x.FolderId == null),
                              Transient = g.Count(x => !x.Deleted && x.IsTransient == true),
                              Orphan = g.Count(x => !x.Deleted && x.FolderId > 0 && !untrackableFolderIds.Contains(x.FolderId.Value) && !x.Tracks.Any())
                          }).FirstOrDefault() ?? new FileCountResult();

            if (result.Total == 0)
            {
                result.Folders = new Dictionary<int, int>();
                return result;
            }

            // Determine file count for each folder
            var byFolders = from f in q
                            where f.FolderId > 0 && !f.Deleted
                            group f by f.FolderId.Value into grp
                            select grp;

            result.Folders = byFolders
                .Select(grp => new { FolderId = grp.Key, Count = grp.Count() })
                .ToDictionary(k => k.FolderId, v => v.Count);

            result.Filter = filter;

            return result;
        }

        public MediaSearchResult SearchFiles(
            MediaSearchQuery query,
            Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier,
            MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            Guard.NotNull(query, nameof(query));

            var files = _searcher.SearchFiles(query, flags);
            if (queryModifier != null)
            {
                files.AlterQuery(queryModifier);
            }

            return new MediaSearchResult(files.Load(), ConvertMediaFile);
        }

        public async Task<MediaSearchResult> SearchFilesAsync(
            MediaSearchQuery query,
            Func<IQueryable<MediaFile>, IQueryable<MediaFile>> queryModifier,
            MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            Guard.NotNull(query, nameof(query));

            var files = _searcher.SearchFiles(query, flags);
            if (queryModifier != null)
            {
                files.AlterQuery(queryModifier);
            }

            return new MediaSearchResult(await files.LoadAsync(), ConvertMediaFile);
        }

        #endregion

        #region Read

        public bool FileExists(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (_helper.TokenizePath(path, false, out var tokens))
            {
                return _fileRepo.Table.Any(x => x.FolderId == tokens.Folder.Id && x.Name == tokens.FileName);
            }

            return false;
        }

        public MediaFileInfo GetFileByPath(string path, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            Guard.NotEmpty(path, nameof(path));

            if (_helper.TokenizePath(path, false, out var tokens))
            {
                var table = _searcher.ApplyLoadFlags(_fileRepo.Table, flags);

                var entity = table.FirstOrDefault(x => x.FolderId == tokens.Folder.Id && x.Name == tokens.FileName);
                if (entity != null)
                {
                    EnsureMetadataResolved(entity, true);
                    return ConvertMediaFile(entity, tokens.Folder);
                }
            }

            return null;
        }

        public MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            if (id <= 0)
                return null;

            var query = _fileRepo.Table.Where(x => x.Id == id);
            var entity = _searcher.ApplyLoadFlags(query, flags).FirstOrDefault();

            if (entity != null)
            {
                EnsureMetadataResolved(entity, true);
                return ConvertMediaFile(entity, _folderService.FindNode(entity)?.Value);
            }

            return null;
        }

        public MediaFileInfo GetFileByName(int folderId, string fileName, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            Guard.IsPositive(folderId, nameof(folderId));
            Guard.NotEmpty(fileName, nameof(fileName));

            var query = _fileRepo.Table.Where(x => x.Name == fileName && x.FolderId == folderId);
            var entity = _searcher.ApplyLoadFlags(query, flags).FirstOrDefault();

            if (entity != null)
            {
                EnsureMetadataResolved(entity, true);
                var dir = _folderService.FindNode(entity)?.Value?.Path;
                return ConvertMediaFile(entity, _folderService.FindNode(entity)?.Value);
            }

            return null;
        }

        public IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            Guard.NotNull(ids, nameof(ids));

            var query = _fileRepo.Table.Where(x => ids.Contains(x.Id));
            var result = _searcher.ApplyLoadFlags(query, flags).ToList();

            return result.OrderBySequence(ids).Select(ConvertMediaFile).ToList();
        }

        public bool CheckUniqueFileName(string path, out string newPath)
        {
            Guard.NotEmpty(path, nameof(path));

            // TODO: (mm) (mc) throw when path is not a file path

            newPath = null;

            if (!_helper.TokenizePath(path, false, out var pathData))
            {
                return false;
            }

            if (CheckUniqueFileName(pathData))
            {
                newPath = pathData.FullPath;
            }

            return newPath != null;
        }

        protected internal virtual bool CheckUniqueFileName(MediaPathData pathData)
        {
            // (perf) First make fast check
            var exists = _fileRepo.Table.Any(x => x.Name == pathData.FileName && x.FolderId == pathData.Folder.Id);
            if (!exists)
            {
                return false;
            }

            var q = new MediaSearchQuery
            {
                FolderId = pathData.Folder.Id,
                Term = string.Concat(pathData.FileTitle, "*.", pathData.Extension),
                Deleted = null
            };

            var query = _searcher.PrepareQuery(q, MediaLoadFlags.AsNoTracking).Select(x => x.Name);
            var files = new HashSet<string>(query.ToList(), StringComparer.CurrentCultureIgnoreCase);

            if (_helper.CheckUniqueFileName(pathData.FileTitle, pathData.Extension, files, out var uniqueName))
            {
                pathData.FileName = uniqueName;
                return true;
            }

            return false;
        }

        public string CombinePaths(params string[] paths)
        {
            return FolderService.NormalizePath(Path.Combine(paths), false);
        }

        public bool FindEqualFile(Stream source, IEnumerable<MediaFile> files, bool leaveOpen, out MediaFile equalFile)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(files, nameof(files));

            equalFile = null;

            try
            {
                foreach (var file in files)
                {
                    source.Seek(0, SeekOrigin.Begin);

                    using (var other = _storageProvider.OpenRead(file))
                    {
                        if (source.ContentsEqual(other, true))
                        {
                            equalFile = file;
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (!leaveOpen)
                {
                    source.Dispose();
                }
            }
        }

        #endregion

        #region Create/Update/Delete/Replace

        public MediaFileInfo ReplaceFile(MediaFile file, Stream inStream, string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(inStream, nameof(inStream));
            Guard.NotEmpty(newFileName, nameof(newFileName));

            var fileInfo = ConvertMediaFile(file);
            var pathData = CreatePathData(fileInfo.Path);
            pathData.FileName = newFileName;

            var storageItem = ProcessFile(ref file, pathData, inStream, false, DuplicateFileHandling.Overwrite, MimeValidationType.MediaTypeMustMatch);

            using (var scope = new DbContextScope(_fileRepo.Context, autoCommit: false))
            {
                try
                {
                    _storageProvider.Save(file, storageItem);
                    scope.Commit();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return fileInfo;
        }

        public async Task<MediaFileInfo> ReplaceFileAsync(MediaFile file, Stream inStream, string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(inStream, nameof(inStream));
            Guard.NotEmpty(newFileName, nameof(newFileName));

            var fileInfo = ConvertMediaFile(file);
            var pathData = CreatePathData(fileInfo.Path);
            pathData.FileName = newFileName;

            var storageItem = ProcessFile(ref file, pathData, inStream, false, DuplicateFileHandling.Overwrite, MimeValidationType.MediaTypeMustMatch);

            using (var scope = new DbContextScope(_fileRepo.Context, autoCommit: false))
            {
                try
                {
                    await _storageProvider.SaveAsync(file, storageItem);
                    await scope.CommitAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return fileInfo;
        }

        public MediaFileInfo SaveFile(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError)
        {
            var pathData = CreatePathData(path);

            var file = _fileRepo.Table.FirstOrDefault(x => x.Name == pathData.FileName && x.FolderId == pathData.Folder.Id);
            var isNewFile = file == null;
            var storageItem = ProcessFile(ref file, pathData, stream, isTransient, dupeFileHandling);

            using (var scope = new DbContextScope(_fileRepo.Context, autoCommit: false))
            {
                if (file.Id == 0)
                {
                    _fileRepo.Insert(file);
                    scope.Commit();
                }

                try
                {
                    _storageProvider.Save(file, storageItem);
                    scope.Commit();
                }
                catch (Exception ex)
                {
                    if (isNewFile)
                    {
                        // New file's metadata should be removed on storage save failure immediately
                        DeleteFile(file, true, true);
                        scope.Commit();
                    }

                    Logger.Error(ex);
                }
            }

            return ConvertMediaFile(file, pathData.Folder);
        }

        public async Task<MediaFileInfo> SaveFileAsync(string path, Stream stream, bool isTransient = true, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError)
        {
            var pathData = CreatePathData(path);

            var file = await _fileRepo.Table.FirstOrDefaultAsync(x => x.Name == pathData.FileName && x.FolderId == pathData.Folder.Id);
            var isNewFile = file == null;
            var storageItem = ProcessFile(ref file, pathData, stream, isTransient, dupeFileHandling);

            using (var scope = new DbContextScope(_fileRepo.Context, autoCommit: false))
            {
                if (file.Id == 0)
                {
                    _fileRepo.Insert(file);
                    await scope.CommitAsync();
                }

                try
                {
                    await _storageProvider.SaveAsync(file, storageItem);
                    await scope.CommitAsync();
                }
                catch (Exception ex)
                {
                    if (isNewFile)
                    {
                        // New file's metadata should be removed on storage save failure immediately
                        DeleteFile(file, true, true);
                        await scope.CommitAsync();
                    }

                    Logger.Error(ex);
                }
            }

            return ConvertMediaFile(file, pathData.Folder);
        }

        public void DeleteFile(MediaFile file, bool permanent, bool force = false)
        {
            Guard.NotNull(file, nameof(file));

            // Delete thumb
            _imageCache.Delete(file);

            if (!permanent)
            {
                file.Deleted = true;
                _fileRepo.Update(file);
            }
            else
            {
                try
                {
                    if (!force && file.Tracks.Any())
                    {
                        throw _exceptionFactory.DeleteTrackedFile(file, null);
                    }

                    // Delete entity
                    _fileRepo.Delete(file);

                    // Delete from storage
                    _storageProvider.Remove(file);
                }
                catch (DbUpdateException ex)
                {
                    if (ex.IsUniquenessViolationException())
                    {
                        throw _exceptionFactory.DeleteTrackedFile(file, ex);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        protected MediaPathData CreatePathData(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (!_helper.TokenizePath(path, true, out var pathData))
            {
                throw new ArgumentException(T("Admin.Media.Exception.InvalidPathExample", path), nameof(path));
            }

            if (pathData.Extension.IsEmpty())
            {
                throw new ArgumentException(T("Admin.Media.Exception.FileExtension", path), nameof(path));
            }

            return pathData;
        }

        protected MediaStorageItem ProcessFile(
            ref MediaFile file,
            MediaPathData pathData,
            Stream inStream,
            bool isTransient = true,
            DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError,
            MimeValidationType mediaValidationType = MimeValidationType.MimeTypeMustMatch)
        {
            if (file != null)
            {
                if (dupeFileHandling == DuplicateFileHandling.ThrowError)
                {
                    var fullPath = pathData.FullPath;
                    CheckUniqueFileName(pathData);
                    throw _exceptionFactory.DuplicateFile(fullPath, ConvertMediaFile(file, pathData.Folder), pathData.FullPath);
                }
                else if (dupeFileHandling == DuplicateFileHandling.Rename)
                {
                    if (CheckUniqueFileName(pathData))
                    {
                        file = null;
                    }
                }
            }

            if (file != null && mediaValidationType != MimeValidationType.NoValidation)
            {
                if (mediaValidationType == MimeValidationType.MimeTypeMustMatch)
                {
                    ValidateMimeTypes("Save", file.MimeType, pathData.MimeType);
                }
                else if (mediaValidationType == MimeValidationType.MediaTypeMustMatch)
                {
                    ValidateMediaTypes("Save", _typeResolver.Resolve(pathData.Extension), file.MediaType);
                }
                
                // Restore file if soft-deleted
                file.Deleted = false;

                // Delete thumbnail
                _imageCache.Delete(file);
            }

            file = file ?? new MediaFile
            {
                IsTransient = isTransient,
                FolderId = pathData.Node.Value.Id
            };

            // Untrackable folders can never contain transient files.
            if (!pathData.Folder.CanDetectTracks)
            {
                file.IsTransient = false;
            }

            var name = pathData.FileName;
            if (name != pathData.FileName)
            {
                pathData.FileName = name;
            }

            file.Name = pathData.FileName;
            file.Extension = pathData.Extension;
            file.MimeType = pathData.MimeType;
            if (file.MediaType == null)
            {
                file.MediaType = _typeResolver.Resolve(pathData.Extension, pathData.MimeType);
            }

            // Process image
            if (inStream != null && inStream.Length > 0 && file.MediaType == MediaType.Image && ProcessImage(file, inStream, out var outImage))
            {
                file.Width = outImage.Size.Width;
                file.Height = outImage.Size.Height;
                file.PixelSize = outImage.Size.Width * outImage.Size.Height;

                return MediaStorageItem.FromImage(outImage);
            }
            else
            {
                file.RefreshMetadata(inStream);

                return MediaStorageItem.FromStream(inStream);
            }
        }

        protected bool ProcessImage(MediaFile file, Stream inStream, out IImage outImage)
        {
            outImage = null;

            var originalSize = Size.Empty;
            var format = _imageProcessor.Factory.GetImageFormat(file.Extension) ?? new UnsupportedImageFormat(file.MimeType, file.Extension);

            try
            {
                originalSize = ImageHeader.GetDimensions(inStream, file.MimeType);
            }
            catch { }

            if (format is UnsupportedImageFormat)
            {
                outImage = new ImageWrapper(inStream, originalSize, format);
                return true;
            }

            var maxSize = _mediaSettings.MaximumImageSize;

            var query = new ProcessImageQuery(inStream)
            {
                Quality = _mediaSettings.DefaultImageQuality,
                Format = file.Extension,
                DisposeSource = true,
                ExecutePostProcessor = ImagePostProcessingEnabled,
                IsValidationMode = true
            };

            if (originalSize.IsEmpty || (originalSize.Height <= maxSize && originalSize.Width <= maxSize))
            {
                // Give subscribers the chance to (pre)-process
                var evt = new ImageUploadedEvent(query, originalSize);
                _eventPublisher.Publish(evt);
                outImage = evt.ResultImage ?? new ImageWrapper(inStream, originalSize, format);

                return true;
            }

            query.MaxSize = maxSize;

            using (var result = _imageProcessor.ProcessImage(query, false))
            {
                outImage = result.Image;
                return true;
            }
        }

        #endregion

        #region Copy & Move

        public FileOperationResult CopyFile(MediaFileInfo mediaFile, string destinationFileName, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError)
        {
            Guard.NotNull(mediaFile, nameof(mediaFile));
            Guard.NotEmpty(destinationFileName, nameof(destinationFileName));

            var destPathData = CreateDestinationPathData(mediaFile.File, destinationFileName);
            var destFileName = destPathData.FileName;
            var destFolderId = destPathData.Folder.Id;

            var dupe = mediaFile.FolderId == destPathData.Folder.Id
                // Source folder equals dest folder, so same file
                ? mediaFile.File
                // Another dest folder, check for duplicate by file name
                : _fileRepo.Table.FirstOrDefault(x => x.Name == destFileName && x.FolderId == destFolderId);

            var copy = InternalCopyFile(
                mediaFile.File,
                destPathData,
                true /* copyData */,
                (DuplicateEntryHandling)((int)dupeFileHandling),
                () => dupe,
                p => CheckUniqueFileName(p),
                out var isDupe);

            return new FileOperationResult
            {
                Operation = "copy",
                DuplicateFileHandling = dupeFileHandling,
                SourceFile = mediaFile,
                DestinationFile = ConvertMediaFile(copy, destPathData.Folder),
                IsDuplicate = isDupe,
                UniquePath = isDupe ? destPathData.FullPath : (string)null
            };
        }

        private MediaFile InternalCopyFile(
            MediaFile file,
            MediaPathData destPathData,
            bool copyData,
            DuplicateEntryHandling dupeEntryHandling,
            Func<MediaFile> dupeFileSelector,
            Action<MediaPathData> uniqueFileNameChecker,
            out bool isDupe)
        {
            // Find dupe and handle
            isDupe = false;
            var dupe = dupeFileSelector();
            if (dupe != null)
            {
                switch (dupeEntryHandling)
                {
                    case DuplicateEntryHandling.Skip:
                        isDupe = true;
                        uniqueFileNameChecker(destPathData);
                        return dupe;
                    case DuplicateEntryHandling.ThrowError:
                        var fullPath = destPathData.FullPath;
                        uniqueFileNameChecker(destPathData);
                        throw _exceptionFactory.DuplicateFile(fullPath, ConvertMediaFile(dupe), destPathData.FullPath);
                    case DuplicateEntryHandling.Rename:
                        uniqueFileNameChecker(destPathData);
                        dupe = null;
                        break;
                    case DuplicateEntryHandling.Overwrite:
                        if (file.FolderId == destPathData.Folder.Id)
                        {
                            throw new IOException(T("Admin.Media.Exception.Overwrite"));
                        }
                        break;
                }
            }

            isDupe = dupe != null;
            var copy = dupe ?? new MediaFile();

            // Simple clone
            MapMediaFile(file, copy);

            // Set folder id
            copy.FolderId = destPathData.Folder.Id;

            // A copied file cannot stay in deleted state
            copy.Deleted = false;

            // Set name stuff
            if (!copy.Name.IsCaseInsensitiveEqual(destPathData.FileName))
            {
                copy.Name = destPathData.FileName;
                copy.Extension = destPathData.Extension;
                copy.MimeType = destPathData.MimeType;
            }

            // Save to DB
            if (isDupe)
            {
                _fileRepo.Update(copy);
            }
            else
            {
                _fileRepo.Insert(copy);
            }

            // Copy data: blob, alt, title etc.
            if (copyData)
            {
                InternalCopyFileData(file, copy);
            }

            return copy;
        }

        private void InternalCopyFileData(MediaFile file, MediaFile copy)
        {
            _storageProvider.Save(copy, MediaStorageItem.FromStream(_storageProvider.OpenRead(file)));
            _imageCache.Delete(copy);

            using (var scope = new DbContextScope(_fileRepo.Context, autoCommit: false))
            {
                // Tags.
                _fileRepo.Context.LoadCollection(file, (MediaFile x) => x.Tags);

                var existingTagsIds = copy.Tags.Select(x => x.Id).ToList();

                foreach (var tag in file.Tags)
                {
                    if (!existingTagsIds.Contains(tag.Id))
                    {
                        copy.Tags.Add(tag);
                        existingTagsIds.Add(tag.Id);
                    }
                }

                // Localized values.
                var languages = _languageService.GetAllLanguages(true);

                foreach (var language in languages)
                {
                    var title = file.GetLocalized(x => x.Title, language.Id, false, false).Value;
                    if (title.HasValue())
                    {
                        _localizedEntityService.SaveLocalizedValue(copy, x => x.Title, title, language.Id);
                    }

                    var alt = file.GetLocalized(x => x.Alt, language.Id, false, false).Value;
                    if (alt.HasValue())
                    {
                        _localizedEntityService.SaveLocalizedValue(copy, x => x.Alt, alt, language.Id);
                    }
                }

                scope.Commit();
                _fileRepo.Context.DetachEntities<MediaTag>();
            }
        }

        public MediaFileInfo MoveFile(MediaFile file, string destinationFileName, DuplicateFileHandling dupeFileHandling = DuplicateFileHandling.ThrowError)
        {
            if (ValidateMoveOperation(file, destinationFileName, dupeFileHandling, out var nameChanged, out var destPathData))
            {
                file.FolderId = destPathData.Folder.Id;

                // A moved file cannot stay in deleted state
                file.Deleted = false;

                if (nameChanged)
                {
                    var title = destPathData.FileTitle;
                    var ext = destPathData.Extension.ToLower();

                    file.Name = title + "." + ext;

                    if (ext != file.Extension.ToLower())
                    {
                        _storageProvider.ChangeExtension(file, ext);
                        file.Extension = ext;
                    }

                    _imageCache.Delete(file);
                }

                _fileRepo.Update(file);
            }

            return ConvertMediaFile(file, destPathData.Folder);
        }

        private bool ValidateMoveOperation(
            MediaFile file,
            string destinationFileName,
            DuplicateFileHandling dupeFileHandling,
            out bool nameChanged,
            out MediaPathData destPathData)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotEmpty(destinationFileName, nameof(destinationFileName));

            destPathData = CreateDestinationPathData(file, destinationFileName);

            var destFileName = destPathData.FileName;
            var destFolderId = destPathData.Folder.Id;
            var folderChanged = destFolderId != file.FolderId;
            var shouldRestore = false;

            nameChanged = !destFileName.IsCaseInsensitiveEqual(file.Name);

            if (file.FolderId.HasValue && folderChanged)
            {
                // When "Move" operation: ensure file stays in source album.
                ValidateAlbums("Move", file.FolderId.Value, destFolderId);
            }

            if (nameChanged)
            {
                // Ensure both MIME types are equal
                ValidateMimeTypes("Move", file.MimeType, destPathData.MimeType);
            }

            // Check whether destination file exists
            if (!folderChanged && file.Deleted)
            {
                // Special case where a file is moved from trash to its origin location.
                // In this case the file should just be restored without any dupe check.
                shouldRestore = true;
            }
            else
            {
                var dupe = _fileRepo.Table.FirstOrDefault(x => x.Name == destFileName && x.FolderId == destFolderId);
                if (dupe != null)
                {
                    if (!folderChanged)
                    {
                        throw _exceptionFactory.IdenticalPaths(ConvertMediaFile(file, destPathData.Folder));
                    }

                    switch (dupeFileHandling)
                    {
                        case DuplicateFileHandling.ThrowError:
                            var fullPath = destPathData.FullPath;
                            _helper.CheckUniqueFileName(destPathData.FileTitle, destPathData.Extension, dupe.Name, out _);
                            throw _exceptionFactory.DuplicateFile(fullPath, ConvertMediaFile(dupe, destPathData.Folder), destPathData.FullPath);
                        case DuplicateFileHandling.Rename:
                            if (_helper.CheckUniqueFileName(destPathData.FileTitle, destPathData.Extension, dupe.Name, out var uniqueName))
                            {
                                nameChanged = true;
                                destPathData.FileName = uniqueName;
                                return true;
                            }
                            break;
                        case DuplicateFileHandling.Overwrite:
                            DeleteFile(dupe, true);
                            break;
                    }
                }
            }

            return folderChanged || nameChanged || shouldRestore;
        }

        #endregion

        #region URL generation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetUrl(MediaFileInfo file, ProcessImageQuery imageQuery, string host = null, bool doFallback = true)
        {
            return _urlGenerator.GenerateUrl(file, imageQuery, host, doFallback);
        }

        #endregion

        #region Utils

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MediaFileInfo ConvertMediaFile(MediaFile file)
        {
            return ConvertMediaFile(file, _folderService.FindNode(file)?.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MediaFileInfo ConvertMediaFile(MediaFile file, MediaFolderNode folder)
        {
            var mediaFile = new MediaFileInfo(file, _storageProvider, _urlGenerator, folder?.Path)
            {
                ThumbSize = _mediaSettings.ProductThumbPictureSize
            };

            return mediaFile;
        }

        private void EnsureMetadataResolved(MediaFile file, bool saveOnResolve)
        {
            var mediaType = _typeResolver.Resolve(file.Extension, file.MimeType);

            var resolveDimensions = mediaType == MediaType.Image && file.Width == null && file.Height == null;
            var resolveSize = file.Size <= 0;

            Stream stream = null;

            if (resolveDimensions || resolveSize)
            {
                stream = _storageProvider.OpenRead(file);
            }

            // Resolve image dimensions
            if (stream != null)
            {
                try
                {
                    if (resolveSize)
                    {
                        file.Size = (int)stream.Length;
                    }

                    if (resolveDimensions)
                    {
                        var size = ImageHeader.GetDimensions(stream, file.MimeType, true);
                        file.Width = size.Width;
                        file.Height = size.Height;
                        file.PixelSize = size.Width * size.Height;
                    }

                    if (saveOnResolve)
                    {
                        try
                        {
                            _fileRepo.Update(file);
                        }
                        catch (InvalidOperationException ioe)
                        {
                            // Ignore exception for pictures that already have been processed.
                            if (!ioe.IsAlreadyAttachedEntityException())
                            {
                                throw;
                            }
                        }
                    }
                }
                finally
                {
                    stream.Dispose();
                }
            }
        }

        private static void MapMediaFile(MediaFile from, MediaFile to)
        {
            to.Alt = from.Alt;
            to.Deleted = from.Deleted;
            to.Extension = from.Extension;
            to.FolderId = from.FolderId;
            to.Height = from.Height;
            to.Hidden = from.Hidden;
            to.IsTransient = from.IsTransient; // TBD: (mm) really?
            to.MediaType = from.MediaType;
            to.Metadata = from.Metadata;
            to.MimeType = from.MimeType;
            to.Name = from.Name;
            to.PixelSize = from.PixelSize;
            to.Size = from.Size;
            to.Title = from.Title;
            to.Version = from.Version;
            to.Width = from.Width;
        }

        private MediaPathData CreateDestinationPathData(MediaFile file, string destinationFileName)
        {
            if (!_helper.TokenizePath(destinationFileName, true, out var pathData))
            {
                // Passed path is NOT a path, but a file name

                if (IsPath(destinationFileName))
                {
                    // ...but file name includes path chars, which is not allowed
                    throw new ArgumentException(
                        T("Admin.Media.Exception.InvalidPath", Path.GetDirectoryName(destinationFileName)),
                        nameof(destinationFileName));
                }

                if (file.FolderId == null)
                {
                    throw new NotSupportedException(T("Admin.Media.Exception.FolderAssignment"));
                }

                pathData = new MediaPathData(_folderService.GetNodeById(file.FolderId.Value), destinationFileName);
            }

            return pathData;
        }

        private void ValidateMimeTypes(string operation, string mime1, string mime2)
        {
            if (!mime1.Equals(mime2, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: (mm) Create this and all other generic exceptions by MediaExceptionFactory
                throw new NotSupportedException(T("Admin.Media.Exception.MimeType", operation, mime1, mime2));
            }
        }

        private void ValidateMediaTypes(string operation, string mime1, string mime2)
        {
            if (mime1 != mime2)
            {
                // TODO: (mm) Create this and all other generic exceptions by MediaExceptionFactory
                throw new NotSupportedException(T("Admin.Media.Exception.MediaType", operation, mime1, mime2));
            }
        }

        private void ValidateAlbums(string operation, int folderId1, int folderId2)
        {
            if (!_folderService.AreInSameAlbum(folderId1, folderId2))
            {
                throw _exceptionFactory.NotSameAlbum(
                    _folderService.GetNodeById(folderId1).Value.Path,
                    _folderService.GetNodeById(folderId2).Value.Path);
            }
        }

        private static bool IsPath(string path)
        {
            return path.IndexOfAny(new[] { '/', '\\' }) > -1;
        }

        #endregion
    }
}
