using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Storage;
using SmartStore.Utilities;
using SmartStore.Collections;

namespace SmartStore.Services.Media
{
    public partial class MediaService : IMediaService
    {
        private readonly IRepository<MediaFile> _fileRepo;
        private readonly IAlbumRegistry _albumRegistry;
        private readonly IFolderService _folderService;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly IMediaUrlGenerator _mediaUrlGenerator;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageCache _imageCache;
        private readonly IMediaStorageProvider _storageProvider;
        private readonly MediaHelper _mediaHelper;

        public MediaService(
            IRepository<MediaFile> fileRepo,
            IAlbumRegistry albumRegistry,
            IFolderService folderService,
            IMediaTypeResolver mediaTypeResolver,
            IMediaUrlGenerator mediaUrlGenerator,
            ISettingService settingService,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IImageProcessor imageProcessor,
            IImageCache imageCache,
            IProviderManager providerManager,
            MediaHelper mediaHelper)
        {
            _fileRepo = fileRepo;
            _albumRegistry = albumRegistry;
            _folderService = folderService;
            _mediaTypeResolver = mediaTypeResolver;
            _mediaUrlGenerator = mediaUrlGenerator;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _imageProcessor = imageProcessor;
            _imageCache = imageCache;
            _mediaHelper = mediaHelper;

            var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
            _storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName).Value;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public IMediaStorageProvider StorageProvider
        {
            get => _storageProvider;
        }

        #region Query

        public int CountFiles(MediaSearchQuery query)
        {
            var q = PrepareQuery(query, MediaLoadFlags.None);
            var count = q.Count();
            return count;
        }

        public Task<int> CountFilesAsync(MediaSearchQuery query)
        {
            var q = PrepareQuery(query, MediaLoadFlags.None);
            return q.CountAsync();
        }

        public MediaSearchResult SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            var q = PrepareQuery(query, flags);
            var result = new PagedList<MediaFile>(q, query.PageIndex, query.PageSize);
            
            return new MediaSearchResult(result.Load(), ConvertMediaFile);
        }

        public async Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            var q = PrepareQuery(query, flags);
            var result = new PagedList<MediaFile>(q, query.PageIndex, query.PageSize);

            return new MediaSearchResult(await result.LoadAsync(), ConvertMediaFile);
        }

        protected virtual MediaFileInfo ConvertMediaFile(MediaFile file)
        {
            var folder = _folderService.FindFolder(file)?.Value;
            return new MediaFileInfo(file, _storageProvider, folder?.Path);
        }

        private MediaFileInfo ConvertMediaFile(MediaFile file, MediaFolderNode folder)
        {
            return new MediaFileInfo(file, _storageProvider, folder?.Path);
        }

        public virtual IQueryable<MediaFile> PrepareQuery(MediaSearchQuery query, MediaLoadFlags flags)
        {
            Guard.NotNull(query, nameof(query));

            var q = _fileRepo.TableUntracked;

            // Deleted
            if (query.Deleted != null)
            {
                q = q.Where(x => x.Deleted == query.Deleted.Value);
            }

            // Folder
            if (query.FolderId > 0)
            {
                if (query.DeepSearch)
                {
                    var folderIds = _folderService.GetFoldersFlattened(query.FolderId.Value, true).Select(x => x.Id).ToArray();
                    q = q.Where(x => x.FolderId != null && folderIds.Contains(x.FolderId.Value));
                }
                else
                {
                    q = q.Where(x => x.FolderId == query.FolderId);
                }
            }

            // MimeType
            if (query.MimeTypes != null && query.MimeTypes.Length > 0)
            {
                q = q.Where(x => query.MimeTypes.Contains(x.MimeType));
            }

            // Extension
            if (query.Extensions != null && query.Extensions.Length > 0)
            {
                q = q.Where(x => query.Extensions.Contains(x.Extension));
            }

            // MediaType
            if (query.MediaTypes != null && query.MediaTypes.Length > 0)
            {
                q = q.Where(x => query.MediaTypes.Contains(x.MediaType));
            }

            // Tag
            if (query.Tags != null && query.Tags.Length > 0)
            {
                q = q.Where(x => x.Tags.Any(t => query.Tags.Contains(t.Id)));
            }

            // Hidden
            if (query.Hidden != null)
            {
                q = q.Where(x => x.Hidden == query.Hidden.Value);
            }

            // Term
            if (query.Term.HasValue() && query.Term != "*")
            {
                // Convert file pattern to SQL 'LIKE' expression
                q = ApplySearchTerm(q, query.Term, query.IncludeAltForTerm, query.ExactMatch);
            }

            // Sorting
            if (query.SortBy.HasValue())
            {
                var ordering = query.SortBy;
                if (query.SortDescending) ordering += " descending";

                q = q.OrderBy(ordering);
            }

            return ApplyLoadFlags(q, flags);
        }

        private IQueryable<MediaFile> ApplySearchTerm(IQueryable<MediaFile> query, string term, bool includeAlt, bool exactMatch)
        {
            var hasAnyCharToken = term.IndexOf('*') > - 1;
            var hasSingleCharToken = term.IndexOf('?') > -1;
            var hasAnyWildcard = hasAnyCharToken || hasSingleCharToken;

            if (!hasAnyWildcard)
            {
                return !includeAlt
                    ? (exactMatch ? query.Where(x => x.Name == term) : query.Where(x => x.Name.Contains(term)))
                    : (exactMatch ? query.Where(x => x.Name == term || x.Alt == term) : query.Where(x => x.Name.Contains(term) || x.Alt.Contains(term)));
            }
            else
            {
                // Convert file pattern to SQL LIKE expression:
                // my*new_file-?.png > my%new/_file-_.png
                
                var hasUnderscore = term.IndexOf('_') > -1;

                if (hasUnderscore)
                {
                    term = term.Replace("_", "/_");
                }
                if (hasAnyCharToken)
                {
                    term = term.Replace('*', '%');
                }
                if (hasSingleCharToken)
                {
                    term = term.Replace('?', '_');
                }

                return !includeAlt
                    ? query.Where(x => DbFunctions.Like(x.Name, term, "/"))
                    : query.Where(x => DbFunctions.Like(x.Name, term, "/") || DbFunctions.Like(x.Alt, term, "/"));
            }
        }

        private IQueryable<MediaFile> ApplyLoadFlags(IQueryable<MediaFile> query, MediaLoadFlags flags)
        {
            if (flags == MediaLoadFlags.None)
            {
                return query;
            }

            if (flags.HasFlag(MediaLoadFlags.AsNoTracking))
            {
                query = query.AsNoTracking();
            }

            if (flags.HasFlag(MediaLoadFlags.WithBlob))
            {
                query = query.Include(x => x.MediaStorage);
            }

            if (flags.HasFlag(MediaLoadFlags.WithFolder))
            {
                query = query.Include(x => x.Folder);
            }

            if (flags.HasFlag(MediaLoadFlags.WithTags))
            {
                query = query.Include(x => x.Tags);
            }

            if (flags.HasFlag(MediaLoadFlags.WithTracks))
            {
                query = query.Include(x => x.Tracks);
            }

            return query;
        }

        #endregion

        #region Read

        public bool FileExists(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (_mediaHelper.TokenizePath(path, out var tokens))
            {
                return _fileRepo.Table.Any(x => x.FolderId == tokens.Folder.Id && x.Name == tokens.FileName);
            }
            
            return false;
        }

        public MediaFileInfo GetFileByPath(string path, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            Guard.NotEmpty(path, nameof(path));

            if (_mediaHelper.TokenizePath(path, out var tokens))
            {
                var table = _fileRepo.Table;

                // TODO: (mm) LoadFlags > Blob | Tags | Tracks

                var entity = table.FirstOrDefault(x => x.FolderId == tokens.Folder.Id && x.Name == tokens.FileName);
                if (entity != null)
                {
                    return new MediaFileInfo(entity, _storageProvider, tokens.Folder.Path);
                }
            }

            return null;
        }

        public MediaFileInfo GetFileById(int id, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            if (id <= 0)
                return null;

            var query = _fileRepo.Table.Where(x => x.Id == id);
            var entity = ApplyLoadFlags(query, flags).FirstOrDefault();

            if (entity != null)
            {
                var dir = _folderService.FindFolder(entity)?.Value?.Path;
                return new MediaFileInfo(entity, _storageProvider, dir);
            }

            return null;
        }

        public IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            Guard.NotNull(ids, nameof(ids));

            var query = _fileRepo.Table.Where(x => ids.Contains(x.Id));
            var result = ApplyLoadFlags(query, flags).ToList();

            return result.OrderBySequence(ids).Select(ConvertMediaFile).ToList();
        }

        public bool CheckUniqueFileName(string path, out string newPath)
        {
            Guard.NotEmpty(path, nameof(path));

            // TODO: (mm) throw when path is not a file path

            newPath = null;

            if (!_mediaHelper.TokenizePath(path, out var pathData))
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
                Term = string.Concat(pathData.FileTitle, "-*", pathData.Extension)
            };

            var query = PrepareQuery(q, MediaLoadFlags.AsNoTracking).Select(x => x.Name);
            var files = new HashSet<string>(query.ToList(), StringComparer.OrdinalIgnoreCase);

            if (files.Count == 0)
            {
                return false;
            }

            var title = pathData.FileTitle;
            var ext = pathData.Extension;

            int i = 1;
            while (true)
            {
                var fileName = string.Concat(title, "-", i, ext);
                if (!files.Contains(fileName))
                {
                    // Found our gap
                    pathData.FileName = fileName;
                    return true;
                }

                i++;
            }
        }

        #endregion

        #region Create/Update/Delete

        public MediaFileInfo CreateFile(string path)
        {
            if (_mediaHelper.TokenizePath(path, out var tokens))
            {
                return InsertFile(
                    null, // album
                    new MediaFile { Name = tokens.FileName, FolderId = tokens.Folder.Id },
                    null, // stream
                    false);
            }

            // TODO: (mm) throw
            throw new Exception();
        }

        public MediaFileInfo CreateFile(int folderId, string fileName)
        {
            var folder = _folderService.GetNodeById(folderId)?.Value;
            if (folder == null)
            {
                // TODO: (mm) throw
            }

            return InsertFile(
                null, // album
                new MediaFile { Name = fileName, FolderId = folderId }, 
                null, // stream
                false);
        }

        public MediaFileInfo InsertFile(string album, MediaFile file, Stream stream, bool validate = true)
        {
            Guard.NotNull(file, nameof(file));

            if (file.Name.IsEmpty())
            {
                throw new ArgumentException("The 'Name' property must be a valid file name.", nameof(file));
            }

            if (album.HasValue() && file.FolderId == null)
            {
                var albumId = _albumRegistry.GetAlbumByName(album)?.Id;
                if (albumId > 0)
                {
                    file.FolderId = albumId;
                }
            }

            if (file.FolderId == null)
            {
                // TODO: (mm) throw
            }

            // [...]

            file.Name = SeoHelper.GetSeName(file.Name, true, false, false);
            file.MediaType = _mediaTypeResolver.Resolve(file);

            // [...]

            // Save to DB
            file.RefreshMetadata(stream);
            _fileRepo.Insert(file);

            // Save BLOB to storage.
            _storageProvider.Save(file, stream);

            return ConvertMediaFile(file);
        }

        public void DeleteFile(MediaFile file, bool permanent)
        {
            Guard.NotNull(file, nameof(file));

            if (!permanent)
            {
                file.Deleted = true;
                _fileRepo.Update(file);
            }
            else
            {
                _imageCache.Delete(file);

                // Delete from storage
                _storageProvider.Remove(file);

                // Delete entity
                _fileRepo.Delete(file);
            }
        }

        #endregion

        #region Copy/Move/Replace/Rename/Update etc.

        public MediaFileInfo CopyFile(MediaFile file, string destinationFileName, bool overwrite = false)
        {
            ValidateCopyOperation(file, destinationFileName, overwrite, out var existingFile, out var destPathData);

            var isOverwrite = existingFile != null;
            var newFile = existingFile ?? new MediaFile();

            // Simple clone
            MapMediaFile(file, newFile);

            // Set folder id
            newFile.FolderId = destPathData.Folder.Id;

            // Set name stuff
            if (!newFile.Name.IsCaseInsensitiveEqual(destPathData.FileName))
            {
                newFile.Name = destPathData.FileName;
                newFile.Extension = destPathData.Extension;
                newFile.MimeType = destPathData.MimeType;
            }

            // TODO: (mm) copy tags
            // TODO: (mm) Copy localized values (Alt, Title)

            // Save to DB
            if (isOverwrite)
            {
                _fileRepo.Update(newFile);
            }
            else
            {
                _fileRepo.Insert(newFile);
            }           

            // Save BLOB to storage.
            _storageProvider.Save(newFile, _storageProvider.OpenRead(file));

            return ConvertMediaFile(newFile, destPathData.Folder);
        }

        public MediaFileInfo MoveFile(MediaFile file, string destinationFileName)
        {
            if (ValidateMoveOperation(file, destinationFileName, out var nameChanged, out var destPathData))
            {
                file.FolderId = destPathData.Folder.Id;

                if (nameChanged)
                {
                    var title = destPathData.FileTitle.ToValidFileName();
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

        public MediaFileInfo ReplaceFile(MediaFile file, string fileName, string mimeType, Stream stream)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(stream, nameof(stream));
            Guard.NotEmpty(fileName, nameof(fileName));

            // TODO: (mm) new TYPE must match current TYPE

            throw new NotImplementedException();
        }

        private bool ValidateMoveOperation(MediaFile file, string destinationFileName, out bool nameChanged, out MediaPathData destPathData)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotEmpty(destinationFileName, nameof(destinationFileName));

            destPathData = CreateDestinationPathData(file, destinationFileName);

            var destFileName = destPathData.FileName;
            var destFolderId = destPathData.Folder.Id;
            var folderChanged = destFolderId != file.FolderId;
            nameChanged = !destFileName.IsCaseInsensitiveEqual(file.Name);

            if (file.FolderId.HasValue && folderChanged)
            {
                // When "Move" operation: ensure file stays in source album.
                ValidateAlbums("Move", destFolderId, file.FolderId.Value);
            }

            if (nameChanged)
            {
                // Ensure bot MIME types are equal
                ValidateMimeTypes("Move", file.MimeType, destPathData.MimeType);
            }

            // Check whether destination file exists
            var exists = _fileRepo.Table.Any(x => x.Name == destFileName && x.FolderId == destFolderId);
            if (exists)
            {
                throw new DuplicateMediaFileException(destPathData.FullPath);
            }

            return folderChanged || nameChanged;
        }

        private bool ValidateCopyOperation(MediaFile file, string destinationFileName, bool overwrite, out MediaFile existing, out MediaPathData destPathData)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotEmpty(destinationFileName, nameof(destinationFileName));

            existing = null;
            destPathData = CreateDestinationPathData(file, destinationFileName);

            var destFileName = destPathData.FileName;
            var destFolderId = destPathData.Folder.Id;

            if (file.FolderId == destPathData.Folder.Id)
            {
                CheckUniqueFileName(destPathData);
            }
            else
            {
                // Check whether destination file exists
                existing = _fileRepo.Table.FirstOrDefault(x => x.FolderId == destFolderId && x.Name == destFileName);
                if (existing != null && !overwrite)
                {
                    throw new DuplicateMediaFileException(destPathData.FullPath);
                }
            }

            return true;
        }

        #endregion

        #region URL generation

        public string GetUrl(MediaFileInfo file, ProcessImageQuery imageQuery, string host = null)
        {
            // TODO: (mm) DoFallback
            return _mediaUrlGenerator.GenerateUrl(file, imageQuery, host);
        }

        #endregion

        #region Utils

        private void MapMediaFile(MediaFile from, MediaFile to)
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
            if (!_mediaHelper.TokenizePath(destinationFileName, out var pathData))
            {
                // Passed path is NOT a path, but a file name

                if (IsPath(destinationFileName))
                {
                    // ...but file name includes path chars, which is not allowed
                    throw new ArgumentException("Invalid path '{0}'.".FormatInvariant(Path.GetDirectoryName(destinationFileName)), nameof(destinationFileName));
                }

                if (file.FolderId == null)
                {
                    throw new NotSupportedException("Cannot operate on files without folder assignment.");
                }

                pathData = new MediaPathData(_folderService.GetNodeById(file.FolderId.Value), destinationFileName);
            }

            return pathData;
        }

        private void ValidateMimeTypes(string operation, string mime1, string mime2)
        {
            if (!mime1.Equals(mime2, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"The file operation '{operation}' does not allow MIME type switching. Source mime: ${mime1}, target mime: ${mime2}.");
            }
        }

        private void ValidateAlbums(string operation, int folderId1, int folderId2)
        {
            if (!_folderService.AreInSameAlbum(folderId1, folderId2))
            {
                throw new NotSameAlbumException(
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
