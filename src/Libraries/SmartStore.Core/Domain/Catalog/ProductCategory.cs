using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    [DataContract]
    public partial class ProductCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
		[DataMember]
        public int CategoryId { get; set; }

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
        /// Indicates whether the mapping is created by the user or by the system.
        /// </summary>
        [DataMember]
        [Index]
        public bool IsSystemMapping { get; set; }

        /// <summary>
        /// Gets the category
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        [DataMember]
        public virtual Product Product { get; set; }

    }

}
