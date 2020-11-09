using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaAlbum : MediaFolder
    {
        /// <summary>
        /// Gets or sets the display name resource key.
        /// </summary>
        public string ResKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the folder paths in file URL generation.
        /// </summary>
        [DataMember]
        public bool IncludePath { get; set; }

        /// <summary>
        /// Gets or sets the media album display order.
        /// </summary>
        [DataMember]
        public int? Order { get; set; }
    }
}
