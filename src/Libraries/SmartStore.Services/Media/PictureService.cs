using System;
using System.Collections.Concurrent;
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
		public string Url { get; set; }
		public string FullSizeUrl { get; set; }
		public int? FullSizeWidth { get; set; }
		public int? FullSizeHeight { get; set; }
		public int MaxSize { get; set; }
		public string MimeType { get; set; }
	}

	public partial class PictureService : IPictureService
    {
		// 0 = Id, 1 = StoreId, 2 = Size, 3 = DefaultPictureType, 4 = StoreLocation
		private const string URLCACHE_LOOKUP_KEY = "image:url-id{0}-st{1}-size{2}-t{3}-l{4}";
		private const string URLCACHE_LOOKUP_KEY_PATTERN = "image:url-id{0}-st*";
		private const string URLCACHE_BYSTORE_KEY_PATTERN = "image:url-id*-st{0}-*";

		private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageCache _imageCache;
		private readonly Provider<IMediaStorageProvider> _storageProvider;
		private readonly IStoreContext _storeContext;
		private readonly HttpContextBase _httpContext;
		private readonly ICacheManager _cacheManager;

		private static readonly string _processedImagesRootPath;
		private static readonly string _defaultImagesRootPath;

		static PictureService()
		{
			// TODO: (mc) make this configurable per web.config
			_processedImagesRootPath = VirtualPathUtility.ToAbsolute("~/media/image/").EnsureEndsWith("/");
			_defaultImagesRootPath = VirtualPathUtility.ToAbsolute("~/content/images/").EnsureEndsWith("/");
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
			_storeContext = storeContext;
			_httpContext = httpContext;
			_cacheManager = cacheManager;

			var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);
			_storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName);

			Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

		#region Utilities

		public static string DefaultImagesRootPath
		{
			get { return _defaultImagesRootPath; }
		}

		protected virtual string GetDefaultImageFileName(PictureType defaultPictureType = PictureType.Entity)
		{
			string defaultImageFileName;

			switch (defaultPictureType)
			{
				case PictureType.Entity:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.png");
					break;
				case PictureType.Avatar:
					defaultImageFileName = _settingService.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
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

			var originalSize = GetPictureSize(pictureBinary);
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

		public virtual Size GetPictureSize(byte[] pictureBinary)
		{
			if (pictureBinary == null || pictureBinary.Length == 0)
			{
				return Size.Empty;
			}

			return GetPictureSize(new MemoryStream(pictureBinary), false);
		}

		protected virtual Size GetPictureSize(Stream input, bool leaveOpen = true)
		{
			Guard.NotNull(input, nameof(input));

			var size = Size.Empty;

			if (!input.CanSeek || input.Length == 0)
			{
				return size;
			}

			try
			{
				using (var reader = new BinaryReader(input, Encoding.UTF8, true))
				{
					size = ImageHeader.GetDimensions(reader);
				}
			}
			catch (Exception)
			{
				// something went wrong with fast image access,
				// so get original size the classic way
				try
				{
					input.Seek(0, SeekOrigin.Begin);
					using (var b = new Bitmap(input))
					{
						size = new Size(b.Width, b.Height);
					}
				}
				catch { }
			}
			finally
			{
				if (!leaveOpen)
				{
					input.Dispose();
				}	
			}

			return size;
		}

		public IDictionary<int, PictureInfo> GetPictureInfos(
			IEnumerable<int> pictureIds,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			Guard.NotNull(pictureIds, nameof(pictureIds));

			var allRequestedInfos = (from id in pictureIds.Distinct()
						   let cacheKey = BuildUrlCacheKey(id, targetSize, showDefaultPicture ? (int)defaultPictureType : 0, storeLocation)
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
					var newInfo = CreatePictureInfo(uncachedPictures.Get(info.PictureId), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
					result.Add(newInfo.Id, newInfo);
					_cacheManager.Put(info.CacheKey, newInfo);
				}
			}

			return result;
		}

		public PictureInfo GetPictureInfo(
			int? pictureId,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var cacheKey = BuildUrlCacheKey(pictureId.GetValueOrDefault(), targetSize, showDefaultPicture ? (int)defaultPictureType : 0, storeLocation);
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(GetPictureById(pictureId.GetValueOrDefault()), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
			});

			return info;
		}

		public PictureInfo GetPictureInfo(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var cacheKey = BuildUrlCacheKey(picture.Id, targetSize, showDefaultPicture ? (int)defaultPictureType : 0, storeLocation);
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
			});

			return info;
		}

		public virtual string GetPictureUrl(
            int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
			var cacheKey = BuildUrlCacheKey(pictureId, targetSize, showDefaultPicture ? (int)defaultPictureType : 0, storeLocation);
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(GetPictureById(pictureId), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
			});

			return info.Url;
		}

		public virtual string GetPictureUrl(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			var cacheKey = BuildUrlCacheKey(picture == null ? 0 : picture.Id, targetSize, showDefaultPicture ? (int)defaultPictureType : 0, storeLocation);
			var info = _cacheManager.Get(cacheKey, () =>
			{
				return CreatePictureInfo(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
			});

			return info.Url;
		}

		public virtual string GetDefaultPictureUrl(
			int targetSize = 0,
			PictureType defaultPictureType = PictureType.Entity,
			string storeLocation = null)
		{
			return GetPictureUrl(0, targetSize, true, storeLocation, defaultPictureType);
		}

		protected virtual PictureInfo CreatePictureInfo(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity)
		{
			int id = 0;
			string url = string.Empty;
			string path = null;
			string mime = null;
			bool isDefaultImage = false;

			if (picture == null)
			{
				if (showDefaultPicture)
				{
					var fileName = GetDefaultImageFileName(defaultPictureType);
					mime = MimeTypes.MapNameToMimeType(fileName);
					path = "{0}/{1}".FormatInvariant(0, fileName);
					isDefaultImage = targetSize == 0;
				}
			}
			else
			{
				id = picture.Id;
				mime = picture.MimeType;
				path = "{0}/{1}.{2}".FormatInvariant(
					id,
					picture.SeoFilename.NullEmpty() ?? picture.Id.ToString(ImageCache.IdFormatString),
					MimeTypes.MapMimeTypeToExtension(picture.MimeType));

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
			}

			string fullSizeUrl = null;

			if (path != null)
			{
				url = (isDefaultImage ? _defaultImagesRootPath : _processedImagesRootPath) + path;

				url = ApplyCdnUrl(url, storeLocation);

				if (id > 0)
				{
					// Don't set fullsize url when there's no picture, even if showDefaultPicture is true.
					fullSizeUrl = url;
				}

				if (targetSize > 0)
				{
					url += "?size={0}".FormatInvariant(targetSize);
				}
			}

			return new PictureInfo
			{
				Id = id,
				Url = url,
				FullSizeUrl = fullSizeUrl,
				MaxSize = targetSize,
				FullSizeWidth = picture?.Width,
				FullSizeHeight = picture?.Height,
				MimeType = mime
			};
		}

		private string ApplyCdnUrl(string url, string storeLocation)
		{
			if (url.IsEmpty())
				return url;

			var root = storeLocation;
			
			if (root.IsEmpty())
			{
				var cdnUrl = _storeContext.CurrentStore.ContentDeliveryNetwork;
				if (cdnUrl.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
				{
					root = cdnUrl;
				}
			}

			if (root.HasValue() && HostingEnvironment.IsHosted)
			{
				// strip out app path from url if needed but do not strip away leading slash
				var appPath = HostingEnvironment.ApplicationVirtualPath.EmptyNull();
				if (appPath.Length > 0 && appPath != "/")
				{
					url = url.Substring(appPath.Length + 1);
				}

				return root.TrimEnd('/', '\\') + url.EnsureStartsWith("/");
			}

			return url;
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
                        var size = GetPictureSize(stream, true);
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

		private string BuildUrlCacheKey(int pictureId, int size, int defaultPictureType /* 0 if showDefaultPicture is false */, string storeLocation = "")
		{
			return URLCACHE_LOOKUP_KEY.FormatInvariant(pictureId, _storeContext.CurrentStore.Id, size, defaultPictureType, storeLocation);
		}

		public int ClearUrlCache(int? storeId = null)
		{
			return _cacheManager.RemoveByPattern(URLCACHE_BYSTORE_KEY_PATTERN.FormatInvariant(storeId.HasValue ? storeId.Value.ToString() : "*"));
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
			_cacheManager.RemoveByPattern(URLCACHE_LOOKUP_KEY_PATTERN.FormatInvariant(picture.Id));

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
				_cacheManager.RemoveByPattern(URLCACHE_LOOKUP_KEY_PATTERN.FormatInvariant(picture.Id));
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
