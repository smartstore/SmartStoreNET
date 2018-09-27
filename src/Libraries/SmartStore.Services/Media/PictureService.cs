using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
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

	public partial class PictureService : IPictureService
    {
		// 0 = Id
		private const string MEDIACACHE_LOOKUP_KEY = "media:info-{0}";
		private const string MEDIACACHE_LOOKUP_KEY_PATTERN = "media:info-*";

		private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
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
            IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
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
			var size = Size.Empty;
			return ValidatePicture(pictureBinary, mimeType, out size);
		}

		public virtual byte[] ValidatePicture(byte[] pictureBinary, string mimeType, out Size size)
		{
			Guard.NotNull(pictureBinary, nameof(pictureBinary));
			Guard.NotEmpty(mimeType, nameof(mimeType));

			size = Size.Empty;

			var originalSize = ImageHeader.GetDimensions(pictureBinary, mimeType);
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

				if (evt.ResultBuffer != null)
				{
					// Maybe subscriber forgot to set this, so check
					size = evt.ResultSize.IsEmpty ? originalSize : evt.ResultSize;
					return evt.ResultBuffer;
				}
				else
				{
					size = originalSize;
					return pictureBinary;
				}
			}

			query.MaxWidth = maxSize;
			query.MaxHeight = maxSize;

			using (var result = _imageProcessor.ProcessImage(query))
			{
				size = new Size(result.Width, result.Height);
				var buffer = result.OutputStream.GetBuffer();
				return buffer;
			}
		}

		public virtual byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<Picture> pictures, out int equalPictureId)
		{
			equalPictureId = 0;
			try
			{
				foreach (var picture in pictures)
				{
					var otherPictureBinary = LoadPictureBinary(picture);

					using (var myStream = new MemoryStream(pictureBinary))
					using (var otherStream = new MemoryStream(otherPictureBinary))
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
		}

		public virtual Stream OpenPictureStream(Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.OpenRead(picture.ToMedia());
		}

		public virtual byte[] LoadPictureBinary(Picture picture)
        {
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.Load(picture.ToMedia());
        }

		public virtual Task<byte[]> LoadPictureBinaryAsync(Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			return _storageProvider.Value.LoadAsync(picture.ToMedia());
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
							   Info = _cacheManager.Contains(cacheKey) ? _cacheManager.Get<PictureInfo>(cacheKey) : (PictureInfo)null
						   }).ToList();

			var result = new Dictionary<int, PictureInfo>(allRequestedInfos.Count);
			var uncachedPictureIds = allRequestedInfos.Where(x => x.Info == null).Select(x => x.PictureId).ToArray();
			var uncachedPictures = new Dictionary<int, Picture>();

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
					_cacheManager.Put(info.CacheKey, newInfo);
				}
			}

			return result;
		}

		public PictureInfo GetPictureInfo(int? pictureId)
		{
			if (pictureId.GetValueOrDefault() < 1)
				return null;

			var cacheKey = MEDIACACHE_LOOKUP_KEY.FormatInvariant(pictureId.GetValueOrDefault());
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(GetPictureById(pictureId.GetValueOrDefault()));
			});

			return info;
		}

		public PictureInfo GetPictureInfo(Picture picture)
		{
			if (picture == null)
				return null;

			var cacheKey = MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id);
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(picture);
			});

			return info;
		}

		public virtual string GetUrl(int pictureId, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
        {
			return GetUrl(GetPictureInfo(pictureId), targetSize, fallbackType, host);
		}

		public virtual string GetUrl(Picture picture, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null)
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
		protected virtual PictureInfo CreatePictureInfo(Picture picture)
		{
			if (picture == null)
				return null;

			var extension = MimeTypes.MapMimeTypeToExtension(picture.MimeType);

			// Build virtual path with pattern "media/image/{id}/{SeoFileName}.{extension}"
			var path = "{0}{1}/{2}.{3}".FormatInvariant(
				_processedImagesRootPath,
				picture.Id,
				picture.SeoFilename.NullEmpty() ?? picture.Id.ToString(ImageCache.IdFormatString),
				extension);

			// Do some maintenance stuff
			EnsurePictureSizeResolved(picture, true);

			if (picture.IsNew)
			{
				_imageCache.Delete(picture);

				// We do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
				UpdatePicture(
					picture,
					null,
					picture.MimeType,
					picture.SeoFilename,
					false,
					false);
			}
			
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

			var sb = new StringBuilder(host, 100);

			// Strip leading "/", the host/apppath has this already
			if (virtualPath[0] == '/')
			{
				virtualPath = virtualPath.Substring(1);
			}

			// Append media path
			sb.Append(virtualPath);

			// Append query
			if (query != null && query.Length > 0)
			{
				if (query[0] != '?') sb.Append("?");
				sb.Append(query);
			}

			return sb.ToString();
		}

		private void EnsurePictureSizeResolved(Picture picture, bool saveOnResolve)
		{
			if (picture.Width == null && picture.Height == null)
			{
                var mediaItem = picture.ToMedia();
                var stream = _storageProvider.Value.OpenRead(mediaItem);

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

		public int ClearUrlCache()
		{
			return _cacheManager.RemoveByPattern(MEDIACACHE_LOOKUP_KEY_PATTERN);
		}

		#endregion

		#region Metadata Storage

		public virtual string GetPictureSeName(string name)
		{
			return SeoHelper.GetSeName(name, true, false);
		}

		public virtual Picture SetSeoFilename(int pictureId, string seoFilename)
		{
			var picture = GetPictureById(pictureId);

			// update if it has been changed
			if (picture != null && seoFilename != picture.SeoFilename)
			{
				UpdatePicture(picture, null, picture.MimeType, seoFilename, true, false);
			}

			return picture;
		}

		public virtual Picture GetPictureById(int pictureId)
		{
			if (pictureId == 0)
				return null;

			var picture = _pictureRepository.GetById(pictureId);
			return picture;
		}

		public virtual IPagedList<Picture> GetPictures(int pageIndex, int pageSize)
		{
			var query = from p in _pictureRepository.Table
						orderby p.Id descending
						select p;

			var pics = new PagedList<Picture>(query, pageIndex, pageSize);
			return pics;
		}

		public virtual IList<Picture> GetPicturesByProductId(int productId, int recordsToReturn = 0)
		{
			if (productId == 0)
				return new List<Picture>();

			var query = from p in _pictureRepository.Table
						join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
						orderby pp.DisplayOrder
						where pp.ProductId == productId
						select p;

			if (recordsToReturn > 0)
				query = query.Take(recordsToReturn);

			var pics = query.ToList();
			return pics;
		}

		public virtual Multimap<int, Picture> GetPicturesByProductIds(int[] productIds, int? maxPicturesPerProduct = null, bool withBlobs = false)
		{
			Guard.NotNull(productIds, nameof(productIds));

			if (maxPicturesPerProduct.HasValue)
			{
				Guard.IsPositive(maxPicturesPerProduct.Value, nameof(maxPicturesPerProduct));
			}

			var map = new Multimap<int, Picture>();

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
								.Select(x => new { PictureId = x.PictureId, ProductId = x.ProductId })
						};

			var groupingResult = query.ToDictionary(x => x.ProductId, x => x.Pictures);

			using (var scope = new DbContextScope(ctx: _pictureRepository.Context, forceNoTracking: null))
			{
				// EF doesn't support eager loading with grouped queries. We must hack a little bit.
				var pictureIds = groupingResult.SelectMany(x => x.Value).Select(x => x.PictureId).Distinct().ToArray();
				var pictures = GetPicturesByIds(pictureIds, withBlobs).ToDictionarySafe(x => x.Id);

				foreach (var p in groupingResult.SelectMany(x => x.Value))
				{
					if (pictures.ContainsKey(p.PictureId))
					{
						map.Add(p.ProductId, pictures[p.PictureId]);
					}
				}
			}

			return map;
		}

		public virtual IList<Picture> GetPicturesByIds(int[] pictureIds, bool withBlobs = false)
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

		public virtual void DeletePicture(Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			// delete thumbs
			_imageCache.Delete(picture);

			// delete from url cache
			_cacheManager.Remove(MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id));

			// delete from storage
			_storageProvider.Value.Remove(picture.ToMedia());

			// delete entity
			_pictureRepository.Delete(picture);
		}

		public virtual Picture InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			int width,
			int height,
			bool isTransient = true)
		{
			var picture = _pictureRepository.Create();
			picture.MimeType = mimeType.EmptyNull().Truncate(20);
			picture.SeoFilename = seoFilename.Truncate(100);
			picture.IsNew = isNew;
			picture.IsTransient = isTransient;
			picture.UpdatedOnUtc = DateTime.UtcNow;

			if (width > 0 && height > 0)
			{
				picture.Width = width;
				picture.Height = height;
			}

			_pictureRepository.Insert(picture);

			// Save to storage.
			_storageProvider.Value.Save(picture.ToMedia(), pictureBinary);

			return picture;
		}

		public virtual Picture InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool isTransient = true,
			bool validateBinary = true)
		{
			var size = Size.Empty;

			if (validateBinary)
			{
				pictureBinary = ValidatePicture(pictureBinary, mimeType, out size);
			}

			return InsertPicture(pictureBinary, mimeType, seoFilename, isNew, size.Width, size.Height, isTransient);
		}

		public virtual void UpdatePicture(
			Picture picture,
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool validateBinary = true)
		{
			if (picture == null)
				return;

			mimeType = mimeType.EmptyNull().Truncate(20);
			seoFilename = seoFilename.Truncate(100);

			var size = Size.Empty;

			if (validateBinary && pictureBinary != null)
			{
				pictureBinary = ValidatePicture(pictureBinary, mimeType, out size);
			}

			// delete old thumbs if a picture has been changed
			if (seoFilename != picture.SeoFilename)
			{
				_imageCache.Delete(picture);

				// delete from url cache
				_cacheManager.Remove(MEDIACACHE_LOOKUP_KEY.FormatInvariant(picture.Id));
			}

			picture.MimeType = mimeType;
			picture.SeoFilename = seoFilename;
			picture.IsNew = isNew;
			picture.UpdatedOnUtc = DateTime.UtcNow;

			if (!size.IsEmpty)
			{
				picture.Width = size.Width;
				picture.Height = size.Height;
			}

			_pictureRepository.Update(picture);

			// save to storage
			if (pictureBinary != null)
			{
				_storageProvider.Value.Save(picture.ToMedia(), pictureBinary);
			}		
		}

		#endregion
	}
}
