using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using ImageResizer;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Media
{
    public class ImageCache : IImageCache
    {
        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 4;

        private readonly MediaSettings _mediaSettings;
		private readonly string _thumbsRootDir;
		private readonly IStoreContext _storeContext;
		private readonly HttpContextBase _httpContext;
		private readonly IFileSystem _fileSystem;

		public ImageCache(
			MediaSettings mediaSettings, 
			IStoreContext storeContext, 
			HttpContextBase httpContext,
			IFileSystem fileSystem)
        {
            _mediaSettings = mediaSettings;
			_storeContext = storeContext;
			_httpContext = httpContext;
			_fileSystem = fileSystem;

			_thumbsRootDir = "Media/Thumbs/";

			_fileSystem.TryCreateFolder("Media");
			_fileSystem.TryCreateFolder("Media/Thumbs");
		}

        public void AddImageToCache(CachedImageResult cachedImage, byte[] buffer)
        {
            Guard.NotNull(cachedImage, nameof(cachedImage));

            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("The image buffer cannot be empty.", "buffer");
            }

            if (cachedImage.Exists)
            {
				_fileSystem.DeleteFile(cachedImage.Path);
            }

			// create folder if needed
			string imageDir = System.IO.Path.GetDirectoryName(cachedImage.Path);
			if (imageDir.HasValue())
			{
				_fileSystem.TryCreateFolder(BuildPath(imageDir));
			}
			
            // save file
			_fileSystem.WriteAllBytes(BuildPath(cachedImage.Path), buffer);
        }

        public virtual CachedImageResult GetCachedImage(int? pictureId, string seoFileName, string extension, object settings = null)
        {
            var imagePath = this.GetCachedImagePath(pictureId, seoFileName, extension, ImageResizerUtil.CreateResizeSettings(settings));

            var result = new CachedImageResult
            {
                Path = imagePath, //"Media/Thumbs/" + imagePath,
                FileName = System.IO.Path.GetFileName(imagePath),
                Extension = GetCleanFileExtension(imagePath),
				Exists = _fileSystem.FileExists(BuildPath(imagePath))
            };

            return result;
        }

        public virtual string GetImageUrl(string imagePath, string storeLocation = null)
        {
			if (imagePath.IsEmpty())
                return null;

			var publicUrl = _fileSystem.GetPublicUrl(BuildPath(imagePath)).EmptyNull();
			if (publicUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || publicUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				// absolute url
				return publicUrl;
			}

			var root = storeLocation;

			if (root.IsEmpty())
			{
				var cdnUrl = _storeContext.CurrentStore.ContentDeliveryNetwork;
				if (cdnUrl.HasValue() && !_httpContext.IsDebuggingEnabled && !_httpContext.Request.IsLocal)
				{
					root = cdnUrl;
				}
			}

			if (root.IsEmpty())
			{
				// relative url must start with a slash
				return publicUrl.EnsureStartsWith("/");
			}

			if (HostingEnvironment.IsHosted)
			{
				// strip out app path from public url if needed but do not strip away leading slash from publicUrl
				var appPath = HostingEnvironment.ApplicationVirtualPath.EmptyNull();
				if (appPath.Length > 0 && appPath != "/")
				{
					publicUrl = publicUrl.Substring(appPath.Length + 1);
				}
			}

			return root.TrimEnd('/', '\\') + publicUrl.EnsureStartsWith("/");
		}

		public virtual void DeleteCachedImages(Picture picture)
        {
            var filter = string.Format("{0}*.*", picture.Id.ToString("0000000"));

			var files = _fileSystem.SearchFiles(_thumbsRootDir, filter);
			foreach (var file in files)
			{
				_fileSystem.DeleteFile(file);
			}
		}

        public virtual void DeleteCachedImages()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foreach (var file in _fileSystem.ListFiles(_thumbsRootDir))
                    {
						if (!file.Name.IsCaseInsensitiveEqual("placeholder") && !file.Name.IsCaseInsensitiveEqual("placeholder.txt"))
						{
							_fileSystem.DeleteFile(file.Path);
						}
                    }
                    foreach (var dir in _fileSystem.ListFolders(_thumbsRootDir))
                    {
						_fileSystem.DeleteFolder(dir.Path);
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
			fileCount = 0;
			totalSize = 0;

			if (!_fileSystem.FolderExists(_thumbsRootDir))
			{
				return;
			}

			fileCount = _fileSystem.SearchFiles(_thumbsRootDir, "*.*").Count();
			totalSize = _fileSystem.ListFolders(_thumbsRootDir).Sum(x => x.Size);
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
            Guard.NotEmpty(extension, nameof(extension));

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
                // get the first four letters of the file name
                var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(imageFileName);
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

		private string BuildPath(string imagePath)
		{
			if (imagePath.IsEmpty())
				return null;

			return _thumbsRootDir + imagePath;
		}

		private static string GetCleanFileExtension(string url)
        {
            var extension = System.IO.Path.GetExtension(url);
            if (extension != null)
            {
                return extension.Replace(".", "").ToLower();
            }

            return string.Empty;
        }

        #endregion

    }

}
