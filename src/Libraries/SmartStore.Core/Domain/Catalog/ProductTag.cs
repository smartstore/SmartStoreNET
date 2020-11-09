using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product tag.
    /// </summary>
	[DataContract]
    public partial class ProductTag : BaseEntity, ILocalizedEntity
    {
        private ICollection<Product> _products;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
		[DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published.
        /// </summary>
        [DataMember]
        [Index("IX_ProductTag_Published")]
        public bool Published { get; set; } = true;

        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        public virtual ICollection<Product> Products
        {
            get => _products ?? (_products = new HashSet<Product>());
            protected set => _products = value;
        }
    }
}
