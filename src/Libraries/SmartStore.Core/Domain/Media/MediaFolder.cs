using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaFolder : BaseEntity
    {
        private ICollection<MediaFile> _files;
        private ICollection<MediaFolder> _children;

        /// <summary>
        /// Gets or sets the media folder name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the folder URL part slug.
        /// </summary>
        [DataMember]
        public string Slug { get; set; } // TBD: localizable (?)

        /// <summary>
        /// Gets or sets the parent folder id.
        /// </summary>
        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the parent folder.
        /// </summary>
        public virtual MediaFolder Parent { get; set; }

        /// <summary>
        /// Gets the child folders.
        /// </summary>
		public virtual ICollection<MediaFolder> Children
        {
            get { return _children ?? (_children = new HashSet<MediaFolder>()); }
            protected set { _children = value; }
        }

        /// <summary>
        /// Gets or sets the media folder metadata as raw JSON string.
        /// </summary>
        [DataMember]
        public string Metadata { get; set; }

        /// <summary>
        /// (Perf) gets or sets the total number of files in this folder (excluding files from sub-folders))
        /// </summary>
        [DataMember]
        public int FilesCount { get; set; }

        /// <summary>
        /// Gets or sets the associated media files.
        /// </summary>
        public virtual ICollection<MediaFile> Files
        {
            get { return _files ?? (_files = new HashSet<MediaFile>()); }
            protected set { _files = value; }
        }
    }
}
