using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Logging;
using System.Web.Hosting;
using SmartStore.Utilities;

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

        public MediaSearchResult SearchFiles(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            var q = PrepareQuery(query, flags);
            var result = new PagedList<MediaFile>(q, query.PageIndex, query.PageSize);
            
            return new MediaSearchResult(result.Load(), ConvertMediaFile);
        }

        public async Task<MediaSearchResult> SearchFilesAsync(MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.None)
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

        protected virtual IQueryable<MediaFile> PrepareQuery(MediaSearchQuery query, MediaLoadFlags flags)
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
            if (query.Term.HasValue())
            {
                // TODO: (mm) Convert pattern to 'LIKE'
                q = q.Where(x => x.Name.Contains(query.Term) || x.Alt.Contains(query.Term));
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

        public MediaFileInfo GetFileByPath(string path)
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

        public IList<MediaFileInfo> GetFilesByIds(int[] ids, MediaLoadFlags flags = MediaLoadFlags.None)
        {
            Guard.NotNull(ids, nameof(ids));

            var query = _fileRepo.Table.Where(x => ids.Contains(x.Id));
            var result = ApplyLoadFlags(query, flags).ToList();

            return result.OrderBySequence(ids).Select(ConvertMediaFile).ToList();
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
                // TODO: (mm) Remove from image cache
                // [...]

                // Delete from storage
                _storageProvider.Remove(file);

                // Delete entity
                _fileRepo.Delete(file);
            }
        }

        #endregion

        #region Copy/Move/Replace/Rename/Update etc.

        public MediaFileInfo CopyFile(MediaFile file, string newPath, bool overwrite = false)
        {
            Guard.NotNull(file, nameof(file));

            if (_mediaHelper.TokenizePath(newPath, out var tokens))
            {
                var newFile = new MediaFile
                {
                    Alt = file.Alt,
                    Deleted = file.Deleted,
                    Name = tokens.FileName,
                    FolderId = tokens.Folder.Id
                };

                // TODO: (mm) Copy localized values (Alt, Title)
                // [...]

                return InsertFile(
                    null, // album
                    newFile,
                    _storageProvider.OpenRead(file),
                    false);
            }

            return ConvertMediaFile(file, tokens.Folder);
        }

        public MediaFileInfo MoveFile(MediaFile file, int destinationFolderId)
        {
            Guard.NotNull(file, nameof(file));

            var folder = _folderService.GetNodeById(destinationFolderId)?.Value;
            if (folder == null)
            {
                // TODO: (mm) throw
            }

            file.FolderId = destinationFolderId;
            _fileRepo.Update(file);

            return ConvertMediaFile(file, folder);
        }

        public MediaFileInfo ReplaceFile(MediaFile file, string fileName, string mimeType, Stream stream)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotNull(stream, nameof(stream));
            Guard.NotEmpty(fileName, nameof(fileName));

            // TODO: (mm) new TYPE must match current TYPE

            throw new NotImplementedException();
        }

        public MediaFileInfo RenameFile(MediaFile file, string newFileName)
        {
            Guard.NotNull(file, nameof(file));
            Guard.NotEmpty(newFileName, nameof(newFileName));

            // TODO: (mm) new Extension must match current Extension

            file.Name = SeoHelper.GetSeName(newFileName, true, false, false);

            return ConvertMediaFile(file);
        }

        #endregion

        #region URL generation

        public string GetUrl(MediaFileInfo file, ProcessImageQuery imageQuery, string host = null)
        {
            // TODO: (mm) DoFallback
            return _mediaUrlGenerator.GenerateUrl(file, imageQuery, host);
        }

        #endregion
    }
}
