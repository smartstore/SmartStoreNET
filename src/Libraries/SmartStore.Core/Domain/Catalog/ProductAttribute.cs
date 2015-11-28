using SmartStore.Core.Domain.Localization;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
	[DataContract]
	public partial class ProductAttribute : BaseEntity, ILocalizedEntity
    {
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
    }
}
