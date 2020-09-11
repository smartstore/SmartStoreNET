using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaTag : BaseEntity
    {
        private ICollection<MediaFile> _mediaFiles;

        /// <summary>
        /// Gets or sets the media tag name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the associated media files.
        /// </summary>
        public virtual ICollection<MediaFile> MediaFiles
        {
            get => _mediaFiles ?? (_mediaFiles = new HashSet<MediaFile>());
            protected set => _mediaFiles = value;
        }
    }
}
