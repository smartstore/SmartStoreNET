using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaFile : BaseEntity, ITransient, IHasMedia, IAuditable, ISoftDeletable, ILocalizedEntity
    {
        #region static

        private static readonly HashSet<string> _outputAffectingProductProps = new HashSet<string>
        {
            nameof(MediaFile.FolderId),
            nameof(MediaFile.Name),
            nameof(MediaFile.Alt),
            nameof(MediaFile.Title),
            nameof(MediaFile.Hidden),
            nameof(MediaFile.Deleted)
        };

        public static IReadOnlyCollection<string> GetOutputAffectingPropertyNames()
        {
            return _outputAffectingProductProps;
        }

        #endregion

        private ICollection<ProductMediaFile> _productMediaFiles;
        private ICollection<MediaTag> _tags;
        private ICollection<MediaTrack> _tracks;

        /// <summary>
        /// Gets or sets the associated folder identifier.
        /// </summary>
        [DataMember]
        [Index("IX_Media_MediaType", 0)]
        [Index("IX_Media_Extension", 0)]
        [Index("IX_Media_PixelSize", 0)]
        [Index("IX_Media_Name", 0)]
        [Index("IX_Media_Size", 0)]
        [Index("IX_Media_UpdatedOnUtc", 0)]
        [Index("IX_Media_FolderId", 0)]
        public int? FolderId { get; set; }

        /// <summary>
        /// Gets or sets the associated folder.
        /// </summary>
        [DataMember]
        public virtual MediaFolder Folder { get; set; }

        /// <summary>
        /// Gets or sets the SEO friendly name of the media file including file extension
        /// </summary>
        [DataMember]
        [Index("IX_Media_Name", 1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the localizable image ALT text
        /// </summary>
        [DataMember]
        public string Alt { get; set; }

        /// <summary>
        /// Gets or sets the localizable media file title text
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the (dotless) file extension
        /// </summary>
        [DataMember]
        [Index("IX_Media_MediaType", 2)]
        [Index("IX_Media_Extension", 1)]
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the file MIME type
        /// </summary>
        [DataMember]
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the file media type (image, video, audio, document etc.)
        /// </summary>
        [DataMember]
        [Index("IX_Media_MediaType", 1)]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        [DataMember]
        [Index("IX_Media_Size", 1)]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the total pixel size of an image (width * height)
        /// </summary>
        [DataMember]
        [Index("IX_Media_MediaType", 3)]
        [Index("IX_Media_Extension", 2)]
        [Index("IX_Media_PixelSize", 1)]
        public int? PixelSize { get; set; }

        /// <summary>
        /// Gets or sets the file metadata as raw JSON dictionary (width, height, video length, EXIF etc.)
        /// </summary>
        [DataMember]
        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the image width (if file is an image)
        /// </summary>
        [DataMember]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the image height (if file is an image)
        /// </summary>
        [DataMember]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [DataMember]
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is transient/preliminary
        /// </summary>
        [DataMember]
        public bool IsTransient { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has been soft deleted
        /// </summary>
        [Index("IX_Media_MediaType", 4)]
        [Index("IX_Media_Extension", 3)]
        [Index("IX_Media_PixelSize", 2)]
        [Index("IX_Media_Name", 2)]
        [Index("IX_Media_Size", 2)]
        [Index("IX_Media_UpdatedOnUtc", 2)]
        [Index("IX_Media_FolderId", 1)]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Internally used for migration stuff only.
        /// 0 = needs migration 'cause existed in previous versions already, 1 = was migrated by migrator, 2 = relations has been detected
        /// </summary>
        public int Version { get; set; } = 2;

        /// <summary>
        /// Gets or sets the media storage identifier
        /// </summary>
        [DataMember]
        public int? MediaStorageId { get; set; }

        /// <summary>
        /// Gets or sets the media storage
        /// </summary>
        public virtual MediaStorage MediaStorage { get; set; }

        /// <summary>
        /// Gets the associated tags
        /// </summary>
        [DataMember]
        public virtual ICollection<MediaTag> Tags
        {
            get => _tags ?? (_tags = new HashSet<MediaTag>());
            protected set => _tags = value;
        }

        /// <summary>
        /// Gets the related entity tracks
        /// </summary>
        [DataMember]
        public virtual ICollection<MediaTrack> Tracks
        {
            get => _tracks ?? (_tracks = new HashSet<MediaTrack>());
            protected set => _tracks = value;
        }

        /// <summary>
        /// Gets or sets the product pictures
        /// </summary>
        [DataMember]
        public virtual ICollection<ProductMediaFile> ProductMediaFiles
        {
            get => _productMediaFiles ?? (_productMediaFiles = new HashSet<ProductMediaFile>());
            protected set => _productMediaFiles = value;
        }
    }
}
