using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product manufacturer mapping
    /// </summary>
    [DataContract]
    public partial class ProductManufacturer : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer identifier
        /// </summary>
		[DataMember]
        public int ManufacturerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
		[DataMember]
        [Index]
        public bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer
        /// </summary>
        [DataMember]
        public virtual Manufacturer Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        [DataMember]
        public virtual Product Product { get; set; }
    }

}
