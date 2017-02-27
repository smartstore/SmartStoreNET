using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute option
    /// </summary>
	[DataContract]
	public partial class SpecificationAttributeOption : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductSpecificationAttribute> _productSpecificationAttributes;

        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
		[DataMember]
		public int SpecificationAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the specification attribute option alias
		/// </summary>
		[DataMember]
		public string Alias { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the identifier for range filtering
		/// </summary>
		[DataMember]
		public int RangeFilterId { get; set; }

		/// <summary>
		/// Gets or sets the specification attribute
		/// </summary>
		[DataMember]
		public virtual SpecificationAttribute SpecificationAttribute { get; set; }

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes
        {
			get { return _productSpecificationAttributes ?? (_productSpecificationAttributes = new HashSet<ProductSpecificationAttribute>()); }
            protected set { _productSpecificationAttributes = value; }
        }
    }
}
