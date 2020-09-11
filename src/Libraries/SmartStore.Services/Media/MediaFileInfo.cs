using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Imaging;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class MediaFileInfo : IFile, ICloneable<MediaFileInfo>
    {
        private string _alt;
        private string _title;

        private readonly IMediaStorageProvider _storageProvider;
        private readonly IMediaUrlGenerator _urlGenerator;

        public MediaFileInfo(MediaFile file, IMediaStorageProvider storageProvider, IMediaUrlGenerator urlGenerator, string directory)
        {
            File = file;
            Directory = directory.EmptyNull();

            if (File.Width.HasValue && File.Height.HasValue)
            {
                Dimensions = new Size(File.Width.Value, File.Height.Value);
            }

            _storageProvider = storageProvider;
            _urlGenerator = urlGenerator;
        }

        #region Clone

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object ICloneable.Clone()
        {
            return Clone();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MediaFileInfo Clone()
        {
            var clone = new MediaFileInfo(File, _storageProvider, _urlGenerator, Directory)
            {
                ThumbSize = this.ThumbSize,
                _alt = this._alt,
                _title = this._title
            };

            return clone;
        }

        #endregion

        [JsonIgnore]
        public MediaFile File { get; }

        [JsonProperty("id")]
        public int Id => File.Id;

        [JsonProperty("folderId")]
        public int? FolderId => File.FolderId;

        [JsonProperty("mime")]
        public string MimeType => File.MimeType;

        [JsonProperty("type")]
        public string MediaType => File.MediaType;

        [JsonProperty("isTransient")]
        public bool IsTransient => File.IsTransient;

        [JsonProperty("deleted")]
        public bool Deleted => File.Deleted;

        [JsonProperty("hidden")]
        public bool Hidden => File.Hidden;

        [JsonProperty("createdOn")]
        public DateTime CreatedOn => File.CreatedOnUtc;

        [JsonProperty("alt")]
        public string Alt
        {
            get => _alt ?? File.Alt;
            set => _alt = value;
        }

        [JsonProperty("titleAttr")]
        public string TitleAttribute
        {
            get => _title ?? File.Title;
            set => _title = value;
        }

        public static explicit operator MediaFile(MediaFileInfo fileInfo)
        {
            return fileInfo.File;
        }

        #region Url

        private readonly IDictionary<(int size, string host), string> _cachedUrls = new Dictionary<(int, string), string>();

        [JsonProperty("url")]
        internal string Url => GetUrl(0, string.Empty);

        [JsonProperty("thumbUrl")]
        internal string ThumbUrl => GetUrl(ThumbSize, string.Empty);

        [JsonIgnore]
        internal int ThumbSize
        {
            // For serialization of "ThumbUrl" in MediaManager
            get; set;
        } = 256;

        public string GetUrl(int maxSize = 0, string host = null)
        {
            var cacheKey = (maxSize, host);
            if (!_cachedUrls.TryGetValue(cacheKey, out var url))
            {
                url = _urlGenerator.GenerateUrl(this, null, host, false);

                if (maxSize > 0)
                {
                    // (perf) Instead of calling GenerateUrl() with a processing query we simply
                    // append the query part to the string.
                    url += "?size=" + maxSize.ToString(CultureInfo.InvariantCulture);
                }

                _cachedUrls[cacheKey] = url;
            }

            return url;
        }

        #endregion

        #region IFile

        [JsonProperty("path")]
        public string Path => (Directory + "/" + File.Name).Trim('/');

        [JsonProperty("dir")]
        public string Directory { get; }

        [JsonProperty("name")]
        public string Name => File.Name;

        [JsonProperty("title")]
        public string Title => System.IO.Path.GetFileNameWithoutExtension(File.Name);

        [JsonProperty("size")]
        public long Size => File.Size;

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated => File.UpdatedOnUtc;

        [JsonProperty("ext")]
        public string Extension => "." + File.Extension;

        [JsonProperty("dimensions")]
        public Size Dimensions { get; }

        [JsonIgnore]
        public bool Exists => File?.Id > 0;

        public Stream OpenRead()
        {
            return _storageProvider?.OpenRead(File);
        }

        public Stream CreateFile()
        {
            throw new NotSupportedException();
        }

        public Task<Stream> CreateFileAsync()
        {
            throw new NotSupportedException();
        }

        public Stream OpenWrite()
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
