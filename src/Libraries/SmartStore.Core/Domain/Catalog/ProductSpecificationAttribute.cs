using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product specification attribute
    /// </summary>
	[DataContract]
    public partial class ProductSpecificationAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
		[DataMember]
        public int SpecificationAttributeOptionId { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute can be filtered. Only effective in accordance with MegaSearchPlus plugin.
        /// </summary>
        [DataMember]
        public bool? AllowFiltering { get; set; }

        /// <summary>
        /// Gets or sets whether the attribute will be shown on the product page.
        /// </summary>
		[DataMember]
        public bool? ShowOnProductPage { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option
        /// </summary>
        [DataMember]
        public virtual SpecificationAttributeOption SpecificationAttributeOption { get; set; }
    }
}
