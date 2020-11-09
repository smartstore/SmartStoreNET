using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Core.Domain.Catalog
{
    public interface IMediaFile
    {
        /// <summary>
        /// Gets or sets the media identifier
        int MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the media file
        /// </summary>
        [JsonIgnore]
        MediaFile MediaFile { get; set; }
    }

    /// <summary>
    /// Represents a product media file mapping
    /// </summary>
    [DataContract]
    public partial class ProductMediaFile : BaseEntity, IMediaFile
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
		[DataMember]
        public int MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }


        /// <summary>
        /// Gets the media file
        /// </summary>
        [DataMember]
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        [DataMember]
        public virtual Product Product { get; set; }
    }
}
