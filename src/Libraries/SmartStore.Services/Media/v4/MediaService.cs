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

namespace SmartStore.Services.Media
{
    public partial class MediaService : IMediaService
    {
        private readonly IRepository<MediaFile> _fileRepo;
        private readonly IAlbumRegistry _albumRegistry;
        private readonly IFolderService _folderService;
        private readonly IMediaTypeResolver _mediaTypeResolver;
        private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageCache _imageCache;
        private readonly IMediaStorageProvider _storageProvider;
        private readonly HttpContextBase _httpContext;

        private readonly string _host;
        private readonly string _appPath;

        private static readonly string _processedImagesRootPath;
        private static readonly string _fallbackImagesRootPath;

        static MediaService()
        {
            _processedImagesRootPath = MediaFileSystem.GetMediaPublicPath() + "image/";
            _fallbackImagesRootPath = "content/images/";
        }

        public MediaService(
            IRepository<MediaFile> fileRepo,
            IAlbumRegistry albumRegistry,
            IFolderService folderService,
            IMediaTypeResolver mediaTypeResolver,
            ISettingService settingService,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IImageProcessor imageProcessor,
            IImageCache imageCache,
            IProviderManager providerManager,
            IStoreContext storeContext,
            HttpContextBase httpContext)
        {
            _fileRepo = fileRepo;
            _albumRegistry = albumRegistry;
            _folderService = folderService;
            _mediaTypeResolver = mediaTypeResolver;
            _settingService = settingService;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _imageProcessor = imageProcessor;
            _imageCache = imageCache;
            _httpContext = httpContext;

            var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
            _storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName).Value;

            string appPath = "/";

            if (HostingEnvironment.IsHosted)
            {
                appPath = HostingEnvironment.ApplicationVirtualPath.EmptyNull();

                var cdn = storeContext.CurrentStore.ContentDeliveryNetwork;
                if (cdn.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
                {
                    _host = cdn;
                }
                else if (mediaSettings.AutoGenerateAbsoluteUrls)
                {
                    var uri = httpContext.Request.Url;
                    _host = "//{0}{1}".FormatInvariant(uri.Authority, appPath);
                }
                else
                {
                    _host = appPath;
                }
            }

            _host = _host.EmptyNull().EnsureEndsWith("/");
            _appPath = appPath.EnsureEndsWith("/");
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
            var path = _folderService.FindFolder(file)?.Value?.Path;
            return new MediaFileInfo(file, _storageProvider, path);
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

            if (TokenizePath(path, out var folder, out var fileName))
            {
                return _fileRepo.Table.Any(x => x.FolderId == folder.Id && x.Name == fileName);
            }
            
            return false;
        }

        public MediaFileInfo GetFileByPath(string path)
        {
            Guard.NotEmpty(path, nameof(path));

            if (TokenizePath(path, out var folder, out var fileName))
            {
                var table = _fileRepo.Table;

                // TODO: (mm) LoadFlags > Blob | Tags | Tracks

                var entity = table.FirstOrDefault(x => x.FolderId == folder.Id && x.Name == fileName);
                if (entity != null)
                {
                    return new MediaFileInfo(entity, _storageProvider, folder.Path);
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

        private bool TokenizePath(string path, out MediaFolderNode folder, out string fileName)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir.HasValue())
            {
                var node = _folderService.GetNodeByPath(dir);
                if (node != null)
                {
                    folder = node.Value;
                    fileName = path.Substring(dir.Length + 1);
                    return true;
                }
            }

            folder = null;
            fileName = null;

            return false;
        }

        #endregion

        #region Create/Update/Delete

        public MediaFileInfo CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public MediaFileInfo CreateFile(int folderId, string fileName)
        {
            throw new NotImplementedException();
        }

        public MediaFileInfo InsertFile(MediaFile file, Stream stream, bool validate = true)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(MediaFile file, bool permanent)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Copy/Move/Touch

        public void CopyFile(MediaFile file, string newPath, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region URL generation

        public string GetUrl(MediaFileInfo file, ProcessImageQuery query, string host = null)
        {
            throw new NotImplementedException();
            
        }

        #endregion
    }
}
