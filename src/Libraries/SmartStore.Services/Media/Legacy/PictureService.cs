using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Storage;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
    [Serializable]
	public class PictureInfo
	{
		public int Id { get; set; }
		/// <summary>
		/// The virtual path to the image file to be processed by the media middleware controller, e.g. "/media/image/1234/image.jpg"
		/// </summary>
		public string Path { get; set; }
		public int? Width { get; set; }
		public int? Height { get; set; }
		public string MimeType { get; set; }
		public string Extension { get; set; }
	}

	public partial class PictureService : ScopedServiceBase, IPictureService
    {
		// 0 = Id
		private const string MEDIACACHE_LOOKUP_KEY = "media:info-{0}";
		private const string MEDIACACHE_LOOKUP_KEY_PATTERN = "media:info-*";

		private readonly IRepository<MediaFile> _pictureRepository;
        private readonly IRepository<ProductMediaFile> _productPictureRepository;
		private readonly IAlbumRegistry _albumRegistry;
		private readonly IMediaTypeResolver _mediaTypeResolver;
		private readonly IMediaUrlGenerator _mediaUrlGenerator;
		private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageCache _imageCache;
		private readonly Provider<IMediaStorageProvider> _storageProvider;
		private readonly HttpContextBase _httpContext;
		private readonly ICacheManager _cacheManager;

		private readonly string _host;
		private readonly string _appPath;

		private static readonly string _processedImagesRootPath;
		private static readonly string _fallbackImagesRootPath;

		static PictureService()
		{
			_processedImagesRootPath = MediaFileSystem.GetMediaPublicPath() + "image/";
			_fallbackImagesRootPath = "content/images/";
		}

		public PictureService(
            IRepository<MediaFile> pictureRepository,
            IRepository<ProductMediaFile> productPictureRepository,
			IAlbumRegistry albumRegistry,
			IMediaTypeResolver mediaTypeResolver,
			IMediaUrlGenerator mediaUrlGenerator,
			ISettingService settingService, 
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IImageProcessor imageProcessor,
            IImageCache imageCache,
			IProviderManager providerManager,
            IStoreContext storeContext,
			HttpContextBase httpContext,
			ICacheManager cacheManager)
        {
            _pictureRepository = pictureRepository;
            _productPictureRepository = productPictureRepository;
			_albumRegistry = albumRegistry;
			_mediaTypeResolver = mediaTypeResolver;
			_mediaUrlGenerator = mediaUrlGenerator;
            _settingService = settingService;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _imageProcessor = imageProcessor;
            _imageCache = imageCache;
			_httpContext = httpContext;
			_cacheManager = cacheManager;

			var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
			_storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName);

			Logger = NullLogger.Instance;

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

		public ILogger Logger { get; set; }

		#region Utilities

		public static string FallbackImagesRootPath
		{
			get { return "~/" + _fallbackImagesRootPath; }
		}

		protected virtual string GetFallbackImageFileName(FallbackPictureType defaultPictureType = FallbackPictureType.Entity)
		{
			string defaultImageFileName;

			switch (defaultPictureType)
			{
				case FallbackPictureType.Entity:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
					break;
				default:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
					break;
			}

			return defaultImageFileName;
		}

		#endregion

		#region Imaging

		public virtual byte[] ValidatePicture(byte[] pictureBinary, string mimeType)
		{
			return ValidatePicture(pictureBinary, mimeType, out _);
		}

		public virtual byte[] ValidatePicture(byte[] pictureBinary, string mimeType, out Size size)
		{
			Guard.NotNull(pictureBinary, nameof(pictureBinary));
			Guard.NotEmpty(mimeType, nameof(mimeType));

			size = Size.Empty;

			var originalSize = ImageHeader.GetDimensions(pictureBinary, mimeType);

            if (mimeType == "image/svg+xml")
            {
                size = originalSize;
                return pictureBinary;
            }

			var maxSize = _mediaSettings.MaximumImageSize;

			var query = new ProcessImageQuery(pictureBinary)
			{
				Quality = _mediaSettings.DefaultImageQuality,
				Format = MimeTypes.MapMimeTypeToExtension(mimeType),
				IsValidationMode = true
			};

			if (originalSize.IsEmpty || (originalSize.Height <= maxSize && originalSize.Width <= maxSize))
			{
				// Give subscribers the chance to (pre)-process
				var evt = new ImageUploadValidatedEvent(query, originalSize);
				_eventPublisher.Publish(evt);

				if (evt.ResultStream != null)
				{
					// Maybe subscriber forgot to set this, so check
					size = evt.ResultSize.IsEmpty ? originalSize : evt.ResultSize;
					return evt.ResultStream.ToByteArray();
				}
				else
				{
					size = originalSize;
					return pictureBinary;
				}
			}

			query.MaxSize = maxSize;

			using (var result = _imageProcessor.ProcessImage(query))
			{
				size = new Size(result.Width, result.Height);
				var buffer = result.OutputStream.ToByteArray();
				return buffer;
			}
		}

		public virtual byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<MediaFile> pictures, out int equalPictureId)
		{
			equalPictureId = 0;

			var myStream = new MemoryStream(pictureBinary);

			try
			{
				foreach (var picture in pictures)
				{
					myStream.Seek(0, SeekOrigin.Begin);

					using (var otherStream = OpenPictureStream(picture))
					{
						if (myStream.ContentsEqual(otherStream))
						{
							equalPictureId = picture.Id;
							return null;
						}
					}
				}

				return pictureBinary;
			}
			catch
			{
				return null;
			}
			finally
			{
				myStream.Dispose();
			}
		}

		public virtual Stream OpenPictureStream(MediaFile picture)
		{
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.OpenRead(picture);
		}

		public virtual byte[] LoadPictureBinary(MediaFile picture)
        {
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.Load(picture);
        }

		public virtual Task<byte[]> LoadPictureBinaryAsync(MediaFile picture)
		{
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.LoadAsync(picture);
		}

		public virtual Size GetPictureSize(byte[] pictureBinary, string mimeType = null)
		{
			return ImageHeader.GetDimensions(pictureBinary, mimeType);
		}

		public IDictionary<int, PictureInfo> GetPictureInfos(IEnumerable<int> pictureIds)
		{
			Guard.NotNull(pictureIds, nameof(pictureIds));

			var allRequestedInfos = (from id in pictureIds.Distinct().Where(x => x > 0)
						   let cacheKey = MEDIACACHE_LOOKUP_KEY.FormatInvariant(id)
						   select new
						   {
							   PictureId = id,
							   CacheKey = cacheKey,
							   Info = _cacheManager.Contains(cacheKey) ? _cacheManager.Get<PictureInfo>(cacheKey, independent: true) : (PictureInfo)null
						   }).ToList();

			var result = new Dictionary<int, PictureInfo>(allRequestedInfos.Count);
			var uncachedPictureIds = allRequestedInfos.Where(x => x.Info == null).Select(x => x.PictureId).ToArray();
			var uncachedPictures = new Dictionary<int, MediaFile>();

			if (uncachedPictureIds.Length > 0)
			{
				uncachedPictures = GetPicturesByIds(uncachedPictureIds, false).ToDictionary(x => x.Id);
			}

			foreach (var info in allRequestedInfos)
			{
				if (info.Info != null)
				{
					result.Add(info.PictureId, info.Info);
				}
				else
				{
					// TBD: (mc) Does this need a locking strategy? Apparently yes. But it is hard to accomplish for a random sequence
					// without locking the whole thing and loosing performance. Better no lock (?)
					var newInfo = CreatePictureInfo(uncachedPictures.Get(info.PictureId));
					result.Add(info.PictureId, newInfo);
                    GetCache().Put(info.CacheKey, newInfo);
                    HasChanges = true;
				}
			}

			return result;
		}

		public PictureInfo GetPictureInfo(int? pictureId)
		{
			if (pictureId.GetValueOrDefault() < 1)
				return null;

			var cacheKey = MEDIACACHE_LOOKUP_KEY.FormatInvariant(pictureId.GetValueOrDefault());

            var info = _cacheManager.Get<PictureInfo>(cacheKey, independent: true);
            if (info == null)
            {
                info = CreatePictureInfo(GetPictureById(pictureId.GetValueOrDefault()));
                GetCache().Put(cacheKey, info);
            }

            return info;
		}

		public PictureInfo GetPictureInfo(MediaFile picture)
		{
			if (picture == null)
				return null;

			var cacheKey = MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id);

            var info = _cacheManager.Get<PictureInfo>(cacheKey, independent: true);
            if (info == null)
            {
                info = CreatePictureInfo(picture);
                GetCache().Put(cacheKey, info);
            }

			return info;
		}

		public virtual string GetUrl(int pictureId, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
        {
			return GetUrl(GetPictureInfo(pictureId), targetSize, fallbackType, host);
		}

		public virtual string GetUrl(MediaFile picture, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			return GetUrl(GetPictureInfo(picture), targetSize, fallbackType, host);
		}

		public string GetUrl(PictureInfo info, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			string path = null;
			string query = null;

			if (info?.Path != null)
			{
				path = info.Path;
			}
			else if (fallbackType > FallbackPictureType.NoFallback)
			{
				path = String.Concat(_processedImagesRootPath, "0/", GetFallbackImageFileName(fallbackType));
			}

			if (path != null)
			{
				if (targetSize > 0)
				{
					// TBD: (mc) let pass query string as NameValueCollection (?)
					query = "?size=" + targetSize;
				}

				path = BuildUrlCore(path, query, host);
			}

			return path;
		}

		public virtual string GetFallbackUrl(int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
		{
			return GetUrl((PictureInfo)null, targetSize, fallbackType, host);
		}

		/// <summary>
		/// Creates a cacheable counterpart for a Picture object instance
		/// </summary>
		/// <param name="picture"></param>
		/// <returns></returns>
		protected virtual PictureInfo CreatePictureInfo(MediaFile picture)
		{
			if (picture == null)
				return null;

			// Temp code during v4 media manager dev (START)
			var fileName = picture.Name.NullEmpty() ?? picture.Id.ToString(ImageCache.IdFormatString);
			var extension = string.Empty;
			var nameHasExtension = Path.GetExtension(fileName).Length > 1;
			if (!nameHasExtension)
			{
				extension = picture.Extension;
				if (extension.IsEmpty())
				{
					extension = MimeTypes.MapMimeTypeToExtension(picture.MimeType);
				}
				if (extension.HasValue())
				{
					extension = "." + extension;
				}
			}
			// Temp code during v4 media manager dev (END)

			// Build virtual path with pattern "media/image/{id}/{Name}.{Extension}"
			var path = "{0}{1}/{2}".FormatInvariant(
				_processedImagesRootPath,
				picture.Id,
				fileName + extension);

			// Do some maintenance stuff
			EnsurePictureSizeResolved(picture, true);

			extension = (picture.Extension.NullEmpty() ?? Path.GetExtension(picture.Name).NullEmpty() ?? MimeTypes.MapMimeTypeToExtension(picture.MimeType)).TrimStart('.');

			return new PictureInfo
			{
				Id = picture.Id,
				MimeType = picture.MimeType,
				Extension = extension,
				Path = path,
				Width = picture?.Width,
				Height = picture?.Height,
			};
		}

		protected virtual string BuildUrlCore(string virtualPath, string query, string host)
		{
			// TBD: (mc) No arg check because of performance?!

			if (host == null)
			{
				host = _host;
			}
			else if (host == string.Empty)
			{
				host = _appPath;
			}
			else
			{
				host = host.EnsureEndsWith("/");
			}

            var url = host;

			// Strip leading "/", the host/apppath has this already
			if (virtualPath[0] == '/')
			{
				virtualPath = virtualPath.Substring(1);
			}

			// Append media path
            url += virtualPath;

			// Append query
			if (query != null && query.Length > 0)
			{
				if (query[0] != '?') url += "?";
                url += query;
			}

			return url;
		}

		private void EnsurePictureSizeResolved(MediaFile picture, bool saveOnResolve)
		{
			if (picture.MediaType == MediaType.Image && picture.Width == null && picture.Height == null)
			{
                var stream = _storageProvider.Value.OpenRead(picture);

                if (stream != null)
                {
                    try
                    {
                        var size = ImageHeader.GetDimensions(stream, picture.MimeType, true);
                        picture.Width = size.Width;
                        picture.Height = size.Height;
                        picture.UpdatedOnUtc = DateTime.UtcNow;

                        if (saveOnResolve)
                        {
							try
							{
								_pictureRepository.Update(picture);
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
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICacheManager GetCache()
        {
            return IsInScope ? NullCache.Instance : _cacheManager;
        }

        protected override void OnClearCache()
        {
            _cacheManager.RemoveByPattern(MEDIACACHE_LOOKUP_KEY_PATTERN);
        }

        #endregion

        #region Metadata Storage

        public virtual string GetPictureSeName(string name)
		{
			return SeoHelper.GetSeName(name, true, false, false);
		}

		public virtual MediaFile SetSeoFilename(int pictureId, string seoFilename)
		{
			var picture = GetPictureById(pictureId);

			//// TODO: (mm) Think about file name change requirement thoroughly
			//// update if it has been changed
			//if (picture != null && seoFilename != picture.Name)
			//{
			//	UpdatePicture(picture, null, picture.MimeType, seoFilename, true, false);
			//}

			return picture;
		}

		public virtual MediaFile GetPictureById(int pictureId)
		{
			if (pictureId == 0)
				return null;

			var picture = _pictureRepository.GetById(pictureId);
			return picture;
		}

		public virtual IPagedList<MediaFile> GetPictures(int pageIndex, int pageSize)
		{
			var query = from p in _pictureRepository.Table
						orderby p.Id descending
						select p;

			var pics = new PagedList<MediaFile>(query, pageIndex, pageSize);
			return pics;
		}

		public virtual IList<MediaFile> GetPicturesByProductId(int productId, int recordsToReturn = 0)
		{
			if (productId == 0)
				return new List<MediaFile>();

			var query = from p in _pictureRepository.Table
						join pp in _productPictureRepository.Table on p.Id equals pp.MediaFileId
						orderby pp.DisplayOrder
						where pp.ProductId == productId
						select p;

			if (recordsToReturn > 0)
				query = query.Take(() => recordsToReturn);

			var pics = query.ToList();
			return pics;
		}

		public virtual Multimap<int, MediaFile> GetPicturesByProductIds(int[] productIds, int? maxPicturesPerProduct = null, bool withBlobs = false)
		{
			Guard.NotNull(productIds, nameof(productIds));

			if (maxPicturesPerProduct.HasValue)
			{
				Guard.IsPositive(maxPicturesPerProduct.Value, nameof(maxPicturesPerProduct));
			}

			var map = new Multimap<int, MediaFile>();

			if (!productIds.Any())
				return map;

			int take = maxPicturesPerProduct ?? int.MaxValue;

			var query = from pp in _productPictureRepository.TableUntracked
						where productIds.Contains(pp.ProductId)
						group pp by pp.ProductId into g
						select new
						{
							ProductId = g.Key,
							Pictures = g.OrderBy(x => x.DisplayOrder)
								.Take(take)
								.Select(x => new { x.MediaFileId, x.ProductId })
						};

			var groupingResult = query.ToDictionary(x => x.ProductId, x => x.Pictures);

			using (var scope = new DbContextScope(ctx: _pictureRepository.Context, forceNoTracking: null))
			{
				// EF doesn't support eager loading with grouped queries. We must hack a little bit.
				var pictureIds = groupingResult.SelectMany(x => x.Value).Select(x => x.MediaFileId).Distinct().ToArray();
				var pictures = GetPicturesByIds(pictureIds, withBlobs).ToDictionarySafe(x => x.Id);

				foreach (var p in groupingResult.SelectMany(x => x.Value))
				{
					if (pictures.ContainsKey(p.MediaFileId))
					{
						map.Add(p.ProductId, pictures[p.MediaFileId]);
					}
				}
			}

			return map;
		}

		public virtual IList<MediaFile> GetPicturesByIds(int[] pictureIds, bool withBlobs = false)
		{
			Guard.NotNull(pictureIds, nameof(pictureIds));

			var query = _pictureRepository.Table
				.Where(x => pictureIds.Contains(x.Id));

			if (withBlobs)
			{
				query = query.Include(x => x.MediaStorage);
			}

			return query.ToList();
		}

		public virtual void DeletePicture(MediaFile picture)
		{
			Guard.NotNull(picture, nameof(picture));

			// delete thumbs
			_imageCache.Delete(picture);

			// delete from url cache
			_cacheManager.Remove(MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id));
            HasChanges = true;

			// delete from storage
			_storageProvider.Value.Remove(picture);

			// delete entity
			_pictureRepository.Delete(picture);
		}

		public virtual MediaFile InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string filename,
			bool isTransient = true,
			bool validateBinary = true,
			string album = null)
		{
			var size = Size.Empty;

			if (validateBinary)
			{
				pictureBinary = ValidatePicture(pictureBinary, mimeType, out size);
			}

			return InsertPicture(pictureBinary, mimeType, filename, size.Width, size.Height, isTransient, album);
		}

		public virtual MediaFile InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string fileName,
			int width,
			int height,
			bool isTransient = true,
			string album = null)
		{
			var mime = mimeType.EmptyNull().Truncate(100);
			var name = NormalizeFileName(fileName, ref mime, out string ext);

			var file = new MediaFile
			{
				MimeType = mime,
				Extension = ext,
				Name = name,
				IsTransient = isTransient
			};
			
			file.MediaType = _mediaTypeResolver.Resolve(file);

			if (album.HasValue())
			{
				var albumId = _albumRegistry.GetAlbumByName(album)?.Id;
				if (albumId > 0)
				{
					file.FolderId = albumId;
				}
			}

			var stream = pictureBinary?.ToStream();

			file.RefreshMetadata(stream);
			_pictureRepository.Insert(file);

			// Save to storage.
			_storageProvider.Value.Save(file, stream);

			return file;
		}

		public virtual void UpdatePicture(
			MediaFile picture,
			byte[] pictureBinary,
			string mimeType,
			string fileName,
			bool validateBinary = true)
		{
			if (picture == null)
				return;

			var mime = mimeType.EmptyNull().Truncate(100);
			var name = NormalizeFileName(fileName, ref mime, out string ext);

			var size = Size.Empty;

			if (validateBinary && pictureBinary != null)
			{
				pictureBinary = ValidatePicture(pictureBinary, mime, out size);
				picture.Size = pictureBinary.Length;
			}

			// delete old thumbs if a picture has been changed
			if (name != picture.Name)
			{
				_imageCache.Delete(picture);

				// delete from url cache
				_cacheManager.Remove(MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id));
                HasChanges = true;
            }

			picture.Extension = ext;
			picture.MimeType = mime;
			picture.Name = name;

			if (!size.IsEmpty)
			{
				picture.Width = size.Width;
				picture.Height = size.Height;
				picture.PixelSize = size.Width * size.Height;
			}

			picture.MediaType = _mediaTypeResolver.Resolve(picture);

			var stream = pictureBinary?.ToStream();

			if (stream != null)
			{
				picture.RefreshMetadata(stream);
			}
			
			_pictureRepository.Update(picture);

			// save to storage
			if (stream != null)
			{
				_storageProvider.Value.Save(picture, stream);
			}		
		}

		private string NormalizeFileName(string name, ref string mime, out string ext)
		{
			// TODO: (mm) temp helper only, refactor and remove later
			var title = Path.GetFileNameWithoutExtension(name);

			if (title != name)
			{
				// File name includes extension already
				title = GetPictureSeName(title.Truncate(200));
				ext = Path.GetExtension(name).Trim('.');
				mime = MimeTypes.MapNameToMimeType(name);
				return title + "." + ext;
			}

			// Filename is extensionless
			title = GetPictureSeName(title.Truncate(200));
			ext = MimeTypes.MapMimeTypeToExtension(mime);
			return title.Grow(ext, ".");
		}

		#endregion
	}
}
