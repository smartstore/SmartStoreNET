using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageResizer;
using SmartStore.Core;
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
    public partial class PictureService : IPictureService
    {
        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;
		private const string STATIC_IMAGE_PATH = "~/Content/Images";

		private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ISettingService _settingService;
		private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;
        private readonly IImageResizerService _imageResizerService;
        private readonly IImageCache _imageCache;
		private readonly INotifier _notifier;
		private readonly IFileSystem _fileSystem;
		private readonly IBinaryDataService _binaryDataService;

		private readonly Provider<IMediaStorageProvider> _storageProvider;
		private readonly string _storageProviderSystemName;

		private string _mediaPath;
		private string _staticImagePath;

        public PictureService(
            IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService, 
            IWebHelper webHelper,
            ILogger logger, 
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IImageResizerService imageResizerService,
            IImageCache imageCache,
			INotifier notifier,
			IFileSystem fileSystem,
			IBinaryDataService binaryDataService,
			IProviderManager providerManager)
        {
            _pictureRepository = pictureRepository;
            _productPictureRepository = productPictureRepository;
            _settingService = settingService;
            _webHelper = webHelper;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _mediaSettings = mediaSettings;
            _imageResizerService = imageResizerService;
            _imageCache = imageCache;
			_notifier = notifier;
			_fileSystem = fileSystem;
			_binaryDataService = binaryDataService;

			_storageProviderSystemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);

			_storageProvider = providerManager.GetProvider<IMediaStorageProvider>(_storageProviderSystemName);
        }

		#region Utilities

		private bool StoreInDatabase
		{
			get { return (_storageProviderSystemName == DatabaseMediaStorageProvider.SystemName); }
		}

		protected virtual string StaticImagePath
		{
			get { return _staticImagePath ?? (_staticImagePath = CommonHelper.MapPath(STATIC_IMAGE_PATH, false)); }
		}

		protected virtual string GetDefaultImageFileName(PictureType defaultPictureType = PictureType.Entity)
		{
			string defaultImageFileName;
			switch (defaultPictureType)
			{
				case PictureType.Entity:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.jpg");
					break;
				case PictureType.Avatar:
					defaultImageFileName = _settingService.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
					break;
				default:
					defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.jpg");
					break;
			}

			return defaultImageFileName;
		}

		protected virtual Size GetPictureSize(byte[] pictureBinary)
		{
			if (pictureBinary == null || pictureBinary.Length == 0)
			{
				return new Size();
			}

			Size size;
			var stream = new MemoryStream(pictureBinary);

			try
			{
				using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
				{
					size = ImageHeader.GetDimensions(reader);
				}
			}
			catch (Exception)
			{
				// something went wrong with fast image access,
				// so get original size the classic way
				using (var b = new Bitmap(stream))
				{
					size = new Size(b.Width, b.Height);
				}
			}
			finally
			{
				stream.Dispose();
			}

			return size;
		}

		protected internal virtual string GetProcessedImageUrl(
			object source,
			int? pictureId,
			string seoFileName,
			string extension,
			int targetSize = 0,
			string storeLocation = null)
		{
			var resizeSettings = new ResizeSettings();
			if (targetSize > 0)
			{
				resizeSettings.MaxWidth = targetSize;
				resizeSettings.MaxHeight = targetSize;
			}

			var cachedImage = _imageCache.GetCachedImage(
				pictureId,
				seoFileName,
				extension,
				resizeSettings);

			if (!cachedImage.Exists)
			{
				lock (String.Intern(cachedImage.Path))
				{
					var buffer = source as byte[];
					if (buffer == null)
					{
						if (!(source is string))
						{
							return string.Empty;
						}

						try
						{
							buffer = File.ReadAllBytes((string)source);
						}
						catch (Exception exception)
						{
							_logger.Error("Error reading media file '{0}'.".FormatInvariant(source), exception);
							return string.Empty;
						}
					}

					try
					{
						if (targetSize == 0)
						{
							_imageCache.AddImageToCache(cachedImage, buffer);
						}
						else
						{
							var sourceStream = new MemoryStream(buffer);
							using (var resultStream = _imageResizerService.ResizeImage(sourceStream, targetSize, targetSize, _mediaSettings.DefaultImageQuality))
							{
								_imageCache.AddImageToCache(cachedImage, resultStream.GetBuffer());
							}
						}
					}
					catch (Exception exception)
					{
						_logger.Error("Error processing/writing media file '{0}'.".FormatInvariant(cachedImage.Path), exception);
						return string.Empty;
					}
				}
			}

			var url = _imageCache.GetImageUrl(cachedImage.Path, storeLocation);
			return url;
		}

		#endregion

		#region Methods

		public virtual byte[] ValidatePicture(byte[] pictureBinary)
		{
			var originalSize = GetPictureSize(pictureBinary);
			var maxSize = _mediaSettings.MaximumImageSize;

			if (originalSize.IsEmpty || (originalSize.Height <= maxSize && originalSize.Width <= maxSize))
			{
				return pictureBinary;
			}

			using (var resultStream = _imageResizerService.ResizeImage(new MemoryStream(pictureBinary), maxSize, maxSize, _mediaSettings.DefaultImageQuality))
			{
				return resultStream.GetBuffer();
			}
		}

		public virtual byte[] FindEqualPicture(string path, IEnumerable<Picture> pictures, out int equalPictureId)
		{
			return FindEqualPicture(File.ReadAllBytes(path), pictures, out equalPictureId);
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

		public virtual string GetPictureSeName(string name)
        {
            return SeoHelper.GetSeName(name, true, false);
        }

        public virtual byte[] LoadPictureBinary(Picture picture)
        {
			Guard.ArgumentNotNull(() => picture);

			return _storageProvider.Value.Load(picture);
        }

        public virtual Size GetPictureSize(Picture picture)
        {
            var pictureBinary = LoadPictureBinary(picture);
            return GetPictureSize(pictureBinary);
        }

        public virtual string GetPictureUrl(
            int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            return GetPictureUrl(GetPictureById(pictureId), targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

        public virtual string GetPictureUrl(
            Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var url = string.Empty;
            byte[] pictureBinary = null;

			if (picture != null)
			{
				pictureBinary = LoadPictureBinary(picture);
			}

            if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
            {
                if (showDefaultPicture)
                {
                    url = GetDefaultPictureUrl(targetSize, defaultPictureType, storeLocation);
                }

                return url;
            }

            if (picture.IsNew)
            {
                _imageCache.DeleteCachedImages(picture);

                // we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                picture = UpdatePicture(picture.Id,
                    pictureBinary,
                    picture.MimeType,
                    picture.SeoFilename,
                    false,
                    false);
            }

            url = GetProcessedImageUrl(
                pictureBinary,
                picture.Id,
                picture.SeoFilename,
                MimeTypes.MapMimeTypeToExtension(picture.MimeType),
                targetSize,
                storeLocation);

            return url;
        }

		public virtual string GetDefaultPictureUrl(
			int targetSize = 0,
			PictureType defaultPictureType = PictureType.Entity,
			string storeLocation = null)
		{
			var defaultImageFileName = GetDefaultImageFileName(defaultPictureType);
			var filePath = Path.Combine(StaticImagePath, defaultImageFileName);

			if (!File.Exists(filePath))
			{
				return string.Empty;
			}

			var url = GetProcessedImageUrl(
				filePath,
				0,
				Path.GetFileNameWithoutExtension(filePath),
				Path.GetExtension(filePath),
				targetSize,
				storeLocation);

			return url;
		}

        public virtual Picture GetPictureById(int pictureId)
        {
            if (pictureId == 0)
                return null;

            var picture = _pictureRepository.GetById(pictureId);
            return picture;
        }

        public virtual void DeletePicture(Picture picture)
        {
			Guard.ArgumentNotNull(() => picture);

            // delete thumbs
            _imageCache.DeleteCachedImages(picture);

			// delete from storage
			_storageProvider.Value.Remove(picture);

			// delete entity
			_pictureRepository.Delete(picture);

            // event notification
            _eventPublisher.EntityDeleted(picture);
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

		public virtual IList<Picture> GetPicturesByIds(int[] pictureIds)
		{
			Guard.ArgumentNotNull(() => pictureIds);

			var query = _pictureRepository.Table
				.Where(x => pictureIds.Contains(x.Id));

			return query.ToList();
		}

        public virtual Picture InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool isTransient = true,
			bool validateBinary = true)
        {
			mimeType = mimeType.EmptyNull();
			mimeType = mimeType.Truncate(20);

			seoFilename = seoFilename.Truncate(100);

            if (validateBinary)
            {
                pictureBinary = ValidatePicture(pictureBinary);
            }

            var picture = _pictureRepository.Create();
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.IsTransient = isTransient;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Insert(picture);

			// save to storage
			_storageProvider.Value.Save(picture, pictureBinary);

            // event notification
            _eventPublisher.EntityInserted(picture);

            return picture;
        }

        public virtual Picture UpdatePicture
			(int pictureId,
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			bool validateBinary = true)
        {
            mimeType = mimeType.EmptyNull().Truncate(20);
			seoFilename = seoFilename.Truncate(100);

            if (validateBinary)
            {
                pictureBinary = ValidatePicture(pictureBinary);
            }

            var picture = GetPictureById(pictureId);
            if (picture == null)
                return null;

            // delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
            {
                _imageCache.DeleteCachedImages(picture);
            }

            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Update(picture);

			// save to storage
			_storageProvider.Value.Save(picture, pictureBinary);

            // event notification
            _eventPublisher.EntityUpdated(picture);

            return picture;
        }

        public virtual Picture SetSeoFilename(int pictureId, string seoFilename)
        {
            var picture = GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            // update if it has been changed
            if (seoFilename != picture.SeoFilename)
            {
                picture = UpdatePicture(picture.Id, LoadPictureBinary(picture), picture.MimeType, seoFilename, true, false);
            }

            return picture;
        }

        #endregion

        #region Picture moving
		// TODO: move to new interface based class

        public virtual bool StoreInDb
        {
            get
            {
				return _settingService.GetSettingByKey<bool>("Media.Images.StoreInDB", true);
            }
            set
            {
                // check whether the value was changed
                if (this.StoreInDb != value)
                {
                    // save the new setting value
                    _settingService.SetSetting<bool>("Media.Images.StoreInDB", value);

					// move them all
					MovePictures(value);
                }
            }
        }

		protected virtual string GetPictureLocalPath(string fileName)
		{
			var path = _mediaPath ?? (_mediaPath = _webHelper.MapPath("~/Media/"));
			var filePath = Path.Combine(path, fileName);
			return filePath;
		}

		private byte[] LoadPictureFromFile(int pictureId, string mimeType)
		{
			string filePath;
			return LoadPictureFromFile(pictureId, mimeType, out filePath);
		}

		private byte[] LoadPictureFromFile(int pictureId, string mimeType, out string filePath)
		{
			filePath = null;

			string lastPart = MimeTypes.MapMimeTypeToExtension(mimeType);
			string fileName = string.Format("{0}-0.{1}", pictureId.ToString("0000000"), lastPart);
			filePath = GetPictureLocalPath(fileName);
			if (!File.Exists(filePath))
			{
				filePath = null;
				return new byte[0];
			}
			return File.ReadAllBytes(filePath);
		}

		private void SavePictureInFile(int pictureId, byte[] pictureBinary, string mimeType, out string filePath)
		{
			filePath = null;
			string lastPart = MimeTypes.MapMimeTypeToExtension(mimeType);
			string fileName = string.Format("{0}-0.{1}", pictureId.ToString("0000000"), lastPart);
			filePath = GetPictureLocalPath(fileName);
			File.WriteAllBytes(filePath, pictureBinary);
		}

		protected int MovePictures(bool toDb)
		{
			// long running operation, therefore some code chunks are redundant here in order to boost performance

			// a list of ALL file paths that were either deleted or created
			var affectedFiles = new List<string>(1000);

			var ctx = _pictureRepository.Context;
			var utcNow = DateTime.UtcNow;
			var failed = false;
			int i = 0;

			using (var scope = new DbContextScope(ctx: ctx, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			{
				using (var tx = ctx.BeginTransaction())
				{
					// we are about to process data in chunks but want to commit ALL at once when ALL chunks have been processed successfully.
					try
					{
						int pageIndex = 0;
						IPagedList<Picture> pictures = null;

						do
						{
							if (pictures != null)
							{
								// detach all entities from previous page to save memory
								ctx.DetachEntities(pictures);

								// breathe
								pictures.Clear();
								pictures = null;
							}

							// load max 500 picture entities at once
							pictures = this.GetPictures(pageIndex, 500);
							pageIndex++;

							foreach (var picture in pictures)
							{
								string filePath = null;

								if (!toDb)
								{
									if ((picture.BinaryDataId ?? 0) != 0)
									{
										// save picture as file
										SavePictureInFile(picture.Id, picture.BinaryData.Data, picture.MimeType, out filePath);

										// remove picture binary from DB
										try
										{
											_binaryDataService.DeleteBinaryData(picture.BinaryData);
										}
										catch { }

										picture.BinaryDataId = null;
									}
								}
								else
								{
									// load picture binary from file and set in DB
									var picBinary = LoadPictureFromFile(picture.Id, picture.MimeType, out filePath);
									if (picBinary.Length > 0)
									{
										picture.BinaryData = new BinaryData
										{
											Data = picBinary
										};
									}
								}

								// remember file path: we must be able to rollback IO operations on transaction failure
								if (filePath.HasValue())
								{
									affectedFiles.Add(filePath);
									//picture.IsNew = true;
								}		

								// explicitly attach modified entity to context, because we disabled AutoCommit
								picture.UpdatedOnUtc = utcNow;
								_pictureRepository.Update(picture);

								i++;
							}

							// save the current batch to DB
							ctx.SaveChanges();
						}
						while (pictures.HasNextPage);

						// FIRE!
						tx.Commit();
					}
					catch (Exception ex)
					{
						failed = true;
						tx.Rollback();
						_settingService.SetSetting<bool>("Media.Images.StoreInDB", !toDb);
						_notifier.Error(ex.Message);
						_logger.Error(ex);
					}
				}		
			}

			if (affectedFiles.Count > 0)
			{
				if ((toDb && !failed) || (!toDb && failed))
				{
					// FS > DB sucessful OR DB > FS failed: delete all physical files
					// run a background task for the deletion of files (fire & forget)
					Task.Factory.StartNew(state =>
					{
						var files = state as string[];
						foreach (var path in files)
						{
							if (File.Exists(path))
								File.Delete(path);
						}
					}, affectedFiles.ToArray()).ConfigureAwait(false);
				}

				// shrink database (only when DB > FS and success)
				if (!toDb && !failed)
				{
					ctx.ShrinkDatabase();
				}
			}

			return i;
		}

        #endregion
    }
}
