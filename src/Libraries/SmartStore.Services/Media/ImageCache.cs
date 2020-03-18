using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using System.Globalization;

namespace SmartStore.Services.Media
{
	public class ImageCache : IImageCache
    {
		public const string IdFormatString = "0000000";
		internal const int MaxDirLength = 4;

		private readonly MediaSettings _mediaSettings;
		private readonly IMediaFileSystem _fileSystem;
		private readonly IImageProcessor _imageProcessor;
		private readonly IFolderService _folderService;
		private readonly MediaHelper _mediaHelper;
		private readonly string _thumbsRootDir;

		public ImageCache(
			MediaSettings mediaSettings, 
			IMediaFileSystem fileSystem, 
			IImageProcessor imageProcessor,
			IFolderService folderService,
			MediaHelper mediaHelper)
        {
            _mediaSettings = mediaSettings;
			_fileSystem = fileSystem;
			_imageProcessor = imageProcessor;
			_folderService = folderService;
			_mediaHelper = mediaHelper;

			_thumbsRootDir = "Thumbs/";
		}

		public ILogger Logger { get; set; } = NullLogger.Instance;

		public void Put4(CachedImage cachedImage, Stream stream)
		{
			if (PreparePut4(cachedImage, stream))
			{
				var path = BuildPath(cachedImage.Path);
				_fileSystem.SaveStream(path, stream);
				cachedImage.Exists = true;
				cachedImage.File = _fileSystem.GetFile(path);
			}
		}

		public async Task Put4Async(CachedImage cachedImage, Stream stream)
		{
			if (PreparePut4(cachedImage, stream))
			{
				var path = BuildPath(cachedImage.Path);
				await _fileSystem.SaveStreamAsync(path, stream);
				cachedImage.Exists = true;
				cachedImage.File = _fileSystem.GetFile(path);
			}
		}

		public void Put(CachedImage cachedImage, byte[] buffer)
		{
			if (PreparePut(cachedImage, buffer))
			{
				var path = BuildPath(cachedImage.Path);

                if (cachedImage.Extension == "svg")
                {
                    _fileSystem.WriteAllText(path, Encoding.UTF8.GetString(buffer));
                }
                else
                {
                    _fileSystem.WriteAllBytes(path, buffer);
                }

                cachedImage.Exists = true;
				cachedImage.File = _fileSystem.GetFile(path);
			}
		}

		public async Task PutAsync(CachedImage cachedImage, byte[] buffer)
		{
			if (PreparePut(cachedImage, buffer))
			{
				var path = BuildPath(cachedImage.Path);

				// save file
                if (cachedImage.Extension == "svg")
                {
                    await _fileSystem.WriteAllTextAsync(path, Encoding.UTF8.GetString(buffer));
                }
                else
                {
                    await _fileSystem.WriteAllBytesAsync(path, buffer);
                }		

				// Refresh info
				cachedImage.Exists = true;
				cachedImage.File = _fileSystem.GetFile(path);
			}
		}

		private bool PreparePut4(CachedImage cachedImage, Stream stream)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));

			if (stream == null || stream.Length == 0)
			{
				return false;
			}

			if (cachedImage.Exists)
			{
				_fileSystem.DeleteFile(BuildPath(cachedImage.Path));
			}

			// create folder if needed
			string imageDir = System.IO.Path.GetDirectoryName(cachedImage.Path);
			if (imageDir.HasValue())
			{
				_fileSystem.TryCreateFolder(BuildPath(imageDir));
			}

			return true;
		}

		private bool PreparePut(CachedImage cachedImage, byte[] buffer)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));

			if (buffer == null || buffer.Length == 0)
			{
				return false;
			}

			if (cachedImage.Exists)
			{
				_fileSystem.DeleteFile(BuildPath(cachedImage.Path));
			}

			// create folder if needed
			string imageDir = System.IO.Path.GetDirectoryName(cachedImage.Path);
			if (imageDir.HasValue())
			{
				_fileSystem.TryCreateFolder(BuildPath(imageDir));
			}

			return true;
		}

		public virtual CachedImage Get4(int? mediaFileId, MediaPathData data, ProcessImageQuery query = null)
		{
			Guard.NotNull(data, nameof(data));

			var resultExtension = query?.GetResultExtension();
			if (resultExtension != null)
			{
				data.Extension = resultExtension;
			}

			var imagePath = GetCachedImagePath4(mediaFileId, data, query);
			var file = _fileSystem.GetFile(BuildPath(imagePath));

			var result = new CachedImage(file)
			{
				Path = imagePath,
				Extension = data.Extension,
				IsRemote = _fileSystem.IsCloudStorage
			};

			if (file.Exists && file.Size <= 0)
			{
				result.Exists = false;
			}

			return result;
		}

		public virtual CachedImage Get(int? pictureId, string seoFileName, string extension, ProcessImageQuery query = null)
        {
			Guard.NotEmpty(extension, nameof(extension));

			extension = query?.GetResultExtension() ?? extension.TrimStart('.').ToLower();
			var imagePath = GetCachedImagePath(pictureId, seoFileName, extension, query);

			var file = _fileSystem.GetFile(BuildPath(imagePath));

			var result = new CachedImage(file)
			{
				Path = imagePath,
				Extension = extension,
				IsRemote = _fileSystem.IsCloudStorage
			};

			if (file.Exists && file.Size <= 0)
			{
				result.Exists = false;
			}

			return result;
        }

		public virtual CachedImage Get(IFile file, ProcessImageQuery query)
		{
			Guard.NotNull(file, nameof(file));
			Guard.NotNull(query, nameof(query));

			var imagePath = GetCachedImagePath(file, query);
			var thumbFile = _fileSystem.GetFile(BuildPath(imagePath));

			var result = new CachedImage(thumbFile)
			{
				Path = imagePath,
				Extension = file.Extension.TrimStart('.'),
				IsRemote = _fileSystem.IsCloudStorage
			};

			if (file.Exists && file.Size <= 0)
			{
				result.Exists = false;
			}

			return result;
		}

		public virtual Stream Open(CachedImage cachedImage)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));

			return _fileSystem.GetFile(BuildPath(cachedImage.Path)).OpenRead();
		}

        public virtual string GetPublicUrl(string imagePath)
        {
			if (imagePath.IsEmpty())
                return null;

			return _fileSystem.GetPublicUrl(BuildPath(imagePath), true).EmptyNull();
		}

		public virtual void RefreshInfo(CachedImage cachedImage)
		{
			Guard.NotNull(cachedImage, nameof(cachedImage));
			
			var file = _fileSystem.GetFile(cachedImage.File.Path);
			cachedImage.File = file;
			cachedImage.Exists = file.Exists && file.Size > 0;
		}

		//public virtual void Delete4(MediaFile mediaFile)
		//{
		//	var filter = string.Format("{0}{1}*.*", 
		//		mediaFile.Id.ToString(IdFormatString),
		//		mediaFile.FolderId.HasValue ? "-" + mediaFile.FolderId.Value + "-" : "");

		//	var folder = _folderService.GetNodeById(mediaFile.FolderId ?? 0)?.Value;
		//	var data = new TokenizedMediaPath(folder, mediaFile.Name) { Extension = string.Empty };
		//	var pathPattern = GetCachedImagePath4(mediaFile.Id, data);

		//	var files = _fileSystem.SearchFiles(_thumbsRootDir, filter);
		//	//foreach (var file in files)
		//	//{
		//	//	_fileSystem.DeleteFile(file);
		//	//}
		//}

		public virtual void Delete(MediaFile picture)
        {
            var filter = string.Format("{0}*.*", picture.Id.ToString(IdFormatString));

			var files = _fileSystem.SearchFiles(_thumbsRootDir, filter);
			foreach (var file in files)
			{
				_fileSystem.DeleteFile(file);
			}
		}

		public virtual void Delete(IFile file)
		{
			// TODO: (mc) this could lead to more thumbs getting deleted as desired. But who cares? :-)
			var filter = string.Format("{0}*.*", file.Title);

			var files = _fileSystem.SearchFiles(BuildPath(file.Directory), filter);
			foreach (var f in files)
			{
				_fileSystem.DeleteFile(f);
			}
		}

		public virtual void Clear()
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
                catch (Exception ex)
                {
					Logger.Error(ex);
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

		protected string GetCachedImagePath4(int? mediaFileId, MediaPathData data, ProcessImageQuery query = null)
		{
			string result = "";

			// xxxxxxx
			if (mediaFileId.GetValueOrDefault() > 0)
			{
				result = mediaFileId.Value.ToString(IdFormatString).Grow(data.FileTitle, "-");
			}

			// xxxxxxx-f
			if (data.Folder != null)
			{
				result = result.Grow(data.Folder.Id.ToString(CultureInfo.InvariantCulture), "-");
			}

			// xxxxxxx-f-abc
			result = result.Grow(data.FileTitle, "-");

			if (result.IsEmpty())
			{
				// files without name? No way!
				return null;
			}

			if (query != null && query.NeedsProcessing())
			{
				// xxxxxxx-f-abc-w100-h100
				result += query.CreateHash();
			}

			if (_mediaSettings.MultipleThumbDirectories && result.Length > MaxDirLength)
			{
				// Get the first four letters of the file name
				// 0001/xxxxxxx-f-abc-w100-h100
				var subDirectoryName = result.Substring(0, MaxDirLength);
				result = subDirectoryName + "/" + result;
			}

			// 0001/xxxxxxx-f-abc-w100-h100.png
			return result.Grow(data.Extension, ".");
		}

		/// <summary>
		/// Returns the file name with the subfolder (when multidirs are enabled)
		/// </summary>
		/// <param name="pictureId"></param>
		/// <param name="seoFileName">File name without extension</param>
		/// <param name="extension">Dot-less file extension</param>
		/// <param name="query"></param>
		/// <returns></returns>
		private string GetCachedImagePath(int? pictureId, string seoFileName, string extension, ProcessImageQuery query = null)
        {
            string firstPart = "";

            if (pictureId.GetValueOrDefault() > 0)
            {
                firstPart = pictureId.Value.ToString(IdFormatString) + (seoFileName.IsEmpty() ? "" : "-");
            }

            if (firstPart.IsEmpty() && seoFileName.IsEmpty())
            {
                // files without name? No way!
                return null;
            }

            seoFileName = seoFileName.EmptyNull();

            string imageFileName;
            if (query == null || !query.NeedsProcessing())
            {
                imageFileName = String.Concat(firstPart, seoFileName);
            }
            else
            {
                imageFileName = String.Concat(firstPart, seoFileName, query.CreateHash());
            }

            if (_mediaSettings.MultipleThumbDirectories && imageFileName != null && imageFileName.Length > MaxDirLength)
            {
                // Get the first four letters of the file name
                var subDirectoryName = imageFileName.Substring(0, MaxDirLength);
                imageFileName = subDirectoryName + "/" + imageFileName;
            }

            return imageFileName + "." + extension;
        }

		/// <summary>
		/// Returns the images thumb path as is plus query (required for uploaded images)
		/// </summary>
		/// <param name="file">Image file to get thumbnail for</param>
		/// <param name="query"></param>
		/// <returns></returns>
		private string GetCachedImagePath(IFile file, ProcessImageQuery query)
		{
			if (!_imageProcessor.IsSupportedImage(file.Extension))
			{
				throw new InvalidOperationException("Thumbnails for '{0}' files are not supported".FormatInvariant(file.Extension));
			}
			
			// TODO: (mc) prevent creating thumbs for thumbs AND check equality of source and target

			var imageFileName = file.Title + query.CreateHash();
			var extension = (query.GetResultExtension() ?? file.Extension).EnsureStartsWith(".").ToLower();
			var path = _fileSystem.Combine(file.Directory, imageFileName + extension);

			return path.TrimStart('/', '\\');
		}

		private string BuildPath(string imagePath)
		{
			if (imagePath.IsEmpty())
				return null;

			return _thumbsRootDir + imagePath;
		}

        #endregion

    }

}
