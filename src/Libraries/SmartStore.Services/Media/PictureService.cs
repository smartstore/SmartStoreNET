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
using SmartStore.Services.Configuration;
using SmartStore.Utilities;
using SmartStore.Data;

namespace SmartStore.Services.Media
{   
    /// <summary>
    /// Picture service
    /// </summary>
    public partial class PictureService : IPictureService
    {
        #region Const

        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;

        #endregion
        
        #region Fields

        private static readonly object s_lock = new object();

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

		private string _mediaPath;
		private string _imagesPath;

        #endregion

        #region Ctor


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
			INotifier notifier)
        {
            this._pictureRepository = pictureRepository;
            this._productPictureRepository = productPictureRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._logger = logger;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
            this._imageResizerService = imageResizerService;
            this._imageCache = imageCache;
			this._notifier = notifier;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Save picture on file system
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        protected virtual void SavePictureInFile(int pictureId, byte[] pictureBinary, string mimeType)
        {
			string filePath;
			SavePictureInFile(pictureId, pictureBinary, mimeType, out filePath);
        }

		private void SavePictureInFile(int pictureId, byte[] pictureBinary, string mimeType, out string filePath)
		{
			filePath = null;
			string lastPart = MimeTypes.MapMimeTypeToExtension(mimeType);
			string fileName = string.Format("{0}-0.{1}", pictureId.ToString("0000000"), lastPart);
			filePath = GetPictureLocalPath(fileName);
			File.WriteAllBytes(filePath, pictureBinary);
		}

        /// <summary>
        /// Delete a picture on file system
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual void DeletePictureOnFileSystem(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            string lastPart = MimeTypes.MapMimeTypeToExtension(picture.MimeType);
            string fileName = string.Format("{0}-0.{1}", picture.Id.ToString("0000000"), lastPart);
            string filePath = GetPictureLocalPath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

		private string GetDefaultImageFileName(PictureType defaultPictureType = PictureType.Entity)
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

        public virtual byte[] ValidatePicture(byte[] pictureBinary)
        {
            Size originalSize = this.GetPictureSize(pictureBinary);
  
            int maxSize = _mediaSettings.MaximumImageSize;
            if (originalSize.IsEmpty || (originalSize.Height <= maxSize && originalSize.Width <= maxSize))
            {
                return pictureBinary;
            }

            using (var resultStream = _imageResizerService.ResizeImage(new MemoryStream(pictureBinary), maxSize, maxSize, _mediaSettings.DefaultImageQuality))
            {
                return resultStream.GetBuffer();
            }
        }

		public byte[] FindEqualPicture(string path, IEnumerable<Picture> productPictures, out int equalPictureId)
		{
			return FindEqualPicture(File.ReadAllBytes(path), productPictures, out equalPictureId);
		}

		public byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<Picture> productPictures, out int equalPictureId)
		{
			equalPictureId = 0;
			try
			{
				foreach (var picture in productPictures)
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

        #endregion

        #region Methods

        public virtual string GetPictureSeName(string name)
        {
            return SeoHelper.GetSeName(name, true, false);
        }

        public virtual string GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
        {
            // 'GetPictureUrl' takes care of creating the thumb when not created already
            string url = this.GetPictureUrl(picture, targetSize, showDefaultPicture);

            if (url.HasValue())
            {
                var settings = this.CreateResizeSettings(targetSize);

                var cachedImage = _imageCache.GetCachedImage(picture, settings);
                if (cachedImage.Exists)
                {
                    return cachedImage.LocalPath;
                }

                if (showDefaultPicture)
                {
                    var fileName = this.GetDefaultImageFileName();
                    cachedImage = _imageCache.GetCachedImage(
                        0,
                        Path.GetFileNameWithoutExtension(fileName),
                        Path.GetExtension(fileName).TrimStart('.'),
                        settings);
                    if (cachedImage.Exists)
                    {
                        return cachedImage.LocalPath;
                    }
                }  
            }

            return string.Empty;

        }

        public virtual string GetDefaultPictureUrl(int targetSize = 0, PictureType defaultPictureType = PictureType.Entity, string storeLocation = null)
        {
            string defaultImageFileName = GetDefaultImageFileName(defaultPictureType);

            string filePath = GetDefaultPictureLocalPath(defaultImageFileName);
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
             
            var url = this.GetProcessedImageUrl(
                filePath, 
                0,
                Path.GetFileNameWithoutExtension(filePath),
                Path.GetExtension(filePath), 
                targetSize, 
                storeLocation);

            return url;
        }

        protected internal virtual string GetProcessedImageUrl(object source, int? pictureId, string seoFileName, string extension, int targetSize = 0, string storeLocation = null)
        {   
            var cachedImage = _imageCache.GetCachedImage(
                pictureId,
                seoFileName,
                extension,
                this.CreateResizeSettings(targetSize));

            if (!cachedImage.Exists)
            {
                lock (s_lock)
                {
                    if (!File.Exists(cachedImage.LocalPath)) // check again
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
                            catch (Exception ex)
                            {
                                _logger.Error("Error reading media file '{0}'.".FormatInvariant(source), ex);
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
                        catch (Exception ex)
                        {
                            _logger.Error("Error processing/writing media file '{0}'.".FormatInvariant(cachedImage.LocalPath), ex);
                            return string.Empty;
                        }
                    }
                }
            }

            var url = _imageCache.GetImageUrl(cachedImage.Path, storeLocation);
            return url;
        }

        /// <summary>
        /// Loads a picture from file
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary</returns>
        protected virtual byte[] LoadPictureFromFile(int pictureId, string mimeType)
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

        public virtual byte[] LoadPictureBinary(Picture picture)
        {
            return LoadPictureBinary(picture, this.StoreInDb);
        }

        public virtual byte[] LoadPictureBinary(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            byte[] result = null;
            if (fromDb)
            {
                result = picture.PictureBinary;
            }
            else
            {
                result = LoadPictureFromFile(picture.Id, picture.MimeType);
            }

            return result;
        }

        public virtual Size GetPictureSize(Picture picture)
        {
            byte[] pictureBinary = LoadPictureBinary(picture);
            return GetPictureSize(pictureBinary);
        }

        internal Size GetPictureSize(byte[] pictureBinary)
        {
            if (pictureBinary == null || pictureBinary.Length == 0)
            {
                return new Size();
            }
            
            var stream = new MemoryStream(pictureBinary);
            
            Size size;

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

        public virtual string GetPictureUrl(
            int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = GetPictureById(pictureId);
            return GetPictureUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }

        public virtual string GetPictureUrl(
            Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            string url = string.Empty;
            byte[] pictureBinary = null;
            if (picture != null)
                pictureBinary = LoadPictureBinary(picture);
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

            url = this.GetProcessedImageUrl(
                pictureBinary,
                picture.Id,
                picture.SeoFilename,
                MimeTypes.MapMimeTypeToExtension(picture.MimeType),
                targetSize,
                storeLocation);

            return url;
        }

        private ResizeSettings CreateResizeSettings(int targetSize)
        {
            var settings = new ResizeSettings();
            if (targetSize > 0)
            {
                settings.MaxWidth = targetSize;
                settings.MaxHeight = targetSize;
            }

            return settings;
        }

        /// <summary>
        /// Get picture local path. Used when images stored on file system (not in the database)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetPictureLocalPath(string fileName)
        {
			var path = _mediaPath ?? (_mediaPath = _webHelper.MapPath("~/Media/"));
            var filePath = Path.Combine(path, fileName);
            return filePath;
        }

        protected virtual string GetDefaultPictureLocalPath(string fileName)
        {
            var path = _imagesPath ?? (_imagesPath = _webHelper.MapPath("~/Content/Images"));
            var filePath = Path.Combine(path, fileName);
            return filePath;
        }

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual Picture GetPictureById(int pictureId)
        {
            if (pictureId == 0)
                return null;

            var picture = _pictureRepository.GetById(pictureId);
            return picture;
        }

        public virtual void DeletePicture(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException("picture");

            // delete thumbs
            _imageCache.DeleteCachedImages(picture);

            // delete from file system
            if (!this.StoreInDb)
            {
                DeletePictureOnFileSystem(picture);
            }

            // delete from database
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

        public virtual Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename, bool isNew, bool isTransient = true, bool validateBinary = true)
        {
			mimeType = mimeType.EmptyNull();
			mimeType = mimeType.Truncate(20);

			seoFilename = seoFilename.Truncate(100);

            if (validateBinary)
            {
                pictureBinary = ValidatePicture(pictureBinary);
            }

            var picture = _pictureRepository.Create();
            picture.PictureBinary = this.StoreInDb ? pictureBinary : new byte[0];
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.IsTransient = isTransient;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Insert(picture);

            if (!this.StoreInDb)
            {
                SavePictureInFile(picture.Id, pictureBinary, mimeType);
            }

            //event notification
            _eventPublisher.EntityInserted(picture);

            return picture;
        }

        public virtual Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType, string seoFilename, bool isNew, bool validateBinary = true)
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

            //delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
            {
                _imageCache.DeleteCachedImages(picture);
            }

            picture.PictureBinary = (this.StoreInDb ? pictureBinary : new byte[0]);
            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.IsNew = isNew;
			picture.UpdatedOnUtc = DateTime.UtcNow;

            _pictureRepository.Update(picture);

            if (!this.StoreInDb)
            {
                SavePictureInFile(picture.Id, pictureBinary, mimeType);
            }

            //event notification
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

        #region Properties

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

		protected int MovePictures(bool toDb)
		{
			// long running operation, therefore some code chunks are redundant here in order to boost performance

			// a list of ALL file paths that were either deleted or created
			var affectedFiles = new List<string>(1000);

			var ctx = _pictureRepository.Context;
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
									if (picture.PictureBinary != null && picture.PictureBinary.Length > 0)
									{
										// save picture as file
										SavePictureInFile(picture.Id, picture.PictureBinary, picture.MimeType, out filePath);
									}
									// remove picture binary from DB
									picture.PictureBinary = new byte[0];
								}
								else
								{
									// load picture binary from file and set in DB
									var picBinary = LoadPictureFromFile(picture.Id, picture.MimeType, out filePath);
									if (picBinary.Length > 0)
									{
										picture.PictureBinary = picBinary;
									}
								}

								// remember file path: we must be able to rollback IO operations on transaction failure
								if (filePath.HasValue())
								{
									affectedFiles.Add(filePath);
									//picture.IsNew = true;
								}		

								// explicitly attach modified entity to context, because we disabled AutoCommit
								picture.UpdatedOnUtc = DateTime.UtcNow;
								_pictureRepository.Update(picture);

								i++;
							}

							// save the current batch to DB
							ctx.SaveChanges();

						} while (pictures.HasNextPage);

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
