using SmartStore.Core.Domain.Localization;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
	[DataContract]
	public partial class ProductAttribute : BaseEntity, ILocalizedEntity
    {
		private ICollection<ProductAttributeOption> _productAttributeOptions;

        /// <summary>
        /// Gets or sets the product attribute alias 
        /// (an optional key for advanced customization)
        /// </summary>
		[DataMember]
        public string Alias { get; set; }
        
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
		[DataMember]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets whether the attribute can be filtered
		/// </summary>
		[DataMember]
		public bool AllowFiltering { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the prooduct attribute options
		/// </summary>
		[DataMember]
		public virtual ICollection<ProductAttributeOption> ProductAttributeOptions
		{
			get { return _productAttributeOptions ?? (_productAttributeOptions = new HashSet<ProductAttributeOption>()); }
			protected set { _productAttributeOptions = value; }
		}
	}
}
