﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class MediaFileInfo : IFile
    {
        private string _url;
        
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

        public static explicit operator MediaFile(MediaFileInfo fileInfo) => fileInfo.File;

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

        [JsonProperty("url")]
        public string Url
        {
            get { return _url ?? (_url = _urlGenerator.GenerateUrl(this, null, string.Empty)); }
        }

        [JsonIgnore]
        public bool Exists => File.Id > 0;

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
