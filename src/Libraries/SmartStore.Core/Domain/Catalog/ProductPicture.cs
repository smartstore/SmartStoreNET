using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
	[DataContract]
	public partial class ProductPicture : BaseEntity
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
		public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the picture
        /// </summary>
        [DataMember]
        public virtual Picture Picture { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        [DataMember]
        public virtual Product Product { get; set; }
    }
}
