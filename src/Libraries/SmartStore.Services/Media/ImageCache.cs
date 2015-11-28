using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ImageResizer;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Media
{

    public class ImageCache : IImageCache
    {
        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;

        private readonly MediaSettings _mediaSettings;
        private readonly IWebHelper _webHelper;
        private readonly DirectoryInfo _cacheRootDir;
		private readonly IStoreContext _storeContext;
		private readonly HttpContextBase _httpContext;

        public ImageCache(MediaSettings mediaSettings, IWebHelper webHelper, IStoreContext storeContext, HttpContextBase httpContext)
        {
            this._mediaSettings = mediaSettings;
            this._webHelper = webHelper;
			this._storeContext = storeContext;
			this._httpContext = httpContext;

            _cacheRootDir = new DirectoryInfo(_webHelper.MapPath("~/Media/Thumbs"));
        }

        public void AddImageToCache(CachedImageResult cachedImage, byte[] buffer)
        {
            Guard.ArgumentNotNull(() => cachedImage);

            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("The image buffer cannot be empty.", "buffer");
            }

            if (cachedImage.Exists)
            {
                File.Delete(cachedImage.LocalPath);
            }

            // create directory if necessary
            string imageDir = Path.GetDirectoryName(cachedImage.LocalPath);
            if (imageDir.HasValue() && !System.IO.Directory.Exists(imageDir))
            {
                System.IO.Directory.CreateDirectory(imageDir);
            }

            // save file
            File.WriteAllBytes(cachedImage.LocalPath, buffer);
        }

        public virtual CachedImageResult GetCachedImage(int? pictureId, string seoFileName, string extension, object settings = null)
        {
            var imagePath = this.GetCachedImagePath(pictureId, seoFileName, extension, ImageResizerUtils.CreateResizeSettings(settings));
            var localPath = this.GetImageLocalPath(imagePath);

            var result = new CachedImageResult
            {
                Path = imagePath, //"Media/Thumbs/" + imagePath,
                LocalPath = localPath,
                FileName = Path.GetFileName(imagePath),
                Extension = GetCleanFileExtension(imagePath),
                Exists = File.Exists(localPath)
            };

            return result;
        }

        public virtual string GetImageUrl(string imagePath, string storeLocation = null)
        {
			if (imagePath.IsEmpty())
                return null;

			var root = storeLocation;

			if (root.IsEmpty())
			{
				var cdnUrl = _storeContext.CurrentStore.ContentDeliveryNetwork;
				if (cdnUrl.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
				{
					root = cdnUrl;
				}
			}

			root = root.NullEmpty() ?? _httpContext.Request.ApplicationPath;
			root = root.TrimEnd('/', '\\');
			var url = root + "/Media/Thumbs/";

            url = url + imagePath;
            return url;
        }

        public virtual void DeleteCachedImages(Picture picture)
        {
            string filter = string.Format("{0}*.*", picture.Id.ToString("0000000"));
            
            var files = _cacheRootDir.EnumerateFiles(filter, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public virtual void DeleteCachedImages()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foreach (var file in _cacheRootDir.GetFiles())
                    {
						if (!file.Name.IsCaseInsensitiveEqual("placeholder") && !file.Name.IsCaseInsensitiveEqual("placeholder.txt"))
						{
							file.Delete();
						}
                    }
                    foreach (var dir in _cacheRootDir.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                    return;
                }
                catch
                {
                }
            }
        }

        public void CacheStatistics(out long fileCount, out long totalSize)
        {
            var allFiles = _cacheRootDir.GetFiles("*.*", SearchOption.AllDirectories);
			var allImageFiles = allFiles.Where(x => !x.Name.IsCaseInsensitiveEqual("placeholder") && !x.Name.IsCaseInsensitiveEqual("placeholder.txt"));

			fileCount = allImageFiles.Count();

            if (fileCount == 0)
                totalSize = 0;
            else
				totalSize = allImageFiles.Sum(x => x.Length);
        }

        #region Utils

        /// <summary>
        /// Returns the file name with the subfolder (when multidirs are enabled)
        /// </summary>
        /// <param name="picture"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal string GetCachedImagePath(int? pictureId, string seoFileName, string extension, ResizeSettings settings = null)
        {
            Guard.ArgumentNotEmpty(extension, "extension");

            string imageFileName = null;

            string firstPart = "";
            if (pictureId.GetValueOrDefault() > 0)
            {
                firstPart = pictureId.Value.ToString("0000000") + (seoFileName.IsEmpty() ? "" : "-");
            }

            if (firstPart.IsEmpty() && seoFileName.IsEmpty())
            {
                // files without name? No way!
                return null;
            }

            seoFileName = seoFileName.EmptyNull();
            extension = extension.TrimStart('.');

            if (!NeedsProcessing(settings))
            {
                imageFileName = "{0}{1}.{2}".FormatInvariant(firstPart, seoFileName, extension);
            }
            else
            {
                string hashedProps = CreateSettingsHash(settings);
                imageFileName = "{0}{1}-{2}.{3}".FormatInvariant(firstPart, seoFileName, hashedProps, extension);
            }

            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first four letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                    imageFileName = subDirectoryName + "/" + imageFileName;
                }
            }

            return imageFileName;
        }

        private string CreateSettingsHash(ResizeSettings settings)
        {
            if (settings.Count == 2 && settings.MaxWidth > 0 && settings.MaxWidth == settings.MaxHeight)
            {
                return settings.MaxWidth.ToString();
            }
			return settings.ToString().Hash(Encoding.ASCII);
        }

        private bool NeedsProcessing(ResizeSettings settings)
        {
            return settings != null && settings.Count > 0;
        }

        private string GetImageLocalPath(string imagePath)
        {
            if (imagePath.IsEmpty())
                return null;
            var imageFilePath = Path.Combine(_cacheRootDir.FullName, imagePath.Replace('/', '\\'));
            return imageFilePath;
        }

        private static string GetCleanFileExtension(string url)
        {
            var extension = Path.GetExtension(url);
            if (extension != null)
            {
                return extension.Replace(".", "").ToLower();
            }
            return string.Empty;
        }

        #endregion

    }

}
