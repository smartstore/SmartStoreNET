using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Media.Imaging
{
    public class ImageCache : IImageCache
    {
        public const string IdFormatString = "0000000";
        internal const int MaxDirLength = 4;

        private readonly MediaSettings _mediaSettings;
        private readonly IMediaFileSystem _fileSystem;
        private readonly string _thumbsRootDir;

        public ImageCache(
            MediaSettings mediaSettings,
            IMediaFileSystem fileSystem,
            MediaHelper mediaHelper)
        {
            _mediaSettings = mediaSettings;
            _fileSystem = fileSystem;

            _thumbsRootDir = "Thumbs/";
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public void Put(CachedImage cachedImage, IImage image)
        {
            var path = BuildPath(cachedImage.Path);
            using var stream = _fileSystem.GetFile(path).OpenWrite();

            if (PreparePut(cachedImage, stream))
            {
                image.Save(stream);
                image.Dispose();
                PostPut(cachedImage, path);
            }
        }

        public void Put(CachedImage cachedImage, Stream stream)
        {
            if (PreparePut(cachedImage, stream))
            {
                var path = BuildPath(cachedImage.Path);
                _fileSystem.SaveStream(path, stream);
                PostPut(cachedImage, path);
            }
        }

        public async Task PutAsync(CachedImage cachedImage, Stream stream)
        {
            if (PreparePut(cachedImage, stream))
            {
                var path = BuildPath(cachedImage.Path);
                await _fileSystem.SaveStreamAsync(path, stream);
                PostPut(cachedImage, path);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PostPut(CachedImage cachedImage, string path)
        {
            cachedImage.Exists = true;
            cachedImage.File = _fileSystem.GetFile(path);
        }

        private bool PreparePut(CachedImage cachedImage, Stream stream)
        {
            Guard.NotNull(cachedImage, nameof(cachedImage));

            if (stream == null)
            {
                return false;
            }

            // create folder if needed
            string imageDir = System.IO.Path.GetDirectoryName(cachedImage.Path);
            if (imageDir.HasValue())
            {
                _fileSystem.CreateFolder(BuildPath(imageDir));
            }

            return true;
        }

        public virtual CachedImage Get(int? mediaFileId, MediaPathData data, ProcessImageQuery query = null)
        {
            Guard.NotNull(data, nameof(data));

            var resultExtension = query?.GetResultExtension();
            if (resultExtension != null)
            {
                data.Extension = resultExtension;
            }

            var imagePath = GetCachedImagePath(mediaFileId, data, query);
            var file = _fileSystem.GetFile(BuildPath(imagePath));

            var result = new CachedImage(file)
            {
                Path = imagePath,
                Extension = data.Extension,
                IsRemote = _fileSystem.IsCloudStorage
            };

            return result;
        }

        public virtual void RefreshInfo(CachedImage cachedImage)
        {
            Guard.NotNull(cachedImage, nameof(cachedImage));

            var file = _fileSystem.GetFile(cachedImage.File.Path);
            cachedImage.File = file;
            cachedImage.Exists = file.Exists;
        }

        public virtual void Delete(MediaFile picture)
        {
            var filter = string.Format("{0}*.*", picture.Id.ToString(IdFormatString));

            var files = _fileSystem.SearchFiles(_thumbsRootDir, filter);
            foreach (var file in files)
            {
                _fileSystem.DeleteFile(file);
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < 10; i++)
            {

                try
                {
                    _fileSystem.DeleteFolder(_thumbsRootDir);
                    _fileSystem.CreateFolder(_thumbsRootDir);
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

        protected string GetCachedImagePath(int? mediaFileId, MediaPathData data, ProcessImageQuery query = null)
        {
            string result = "";

            // xxxxxxx
            if (mediaFileId.GetValueOrDefault() > 0)
            {
                result = mediaFileId.Value.ToString(IdFormatString);
            }

            //// INFO: (mm) don't include folder id in pathes for now. It results in more complex image cache invalidation code.
            //// xxxxxxx-f
            //if (data.Folder != null)
            //{
            //	result = result.Grow(data.Folder.Id.ToString(CultureInfo.InvariantCulture), "-");
            //}

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

        private string BuildPath(string imagePath)
        {
            if (imagePath.IsEmpty())
                return null;

            return _thumbsRootDir + imagePath;
        }

        #endregion
    }

}
