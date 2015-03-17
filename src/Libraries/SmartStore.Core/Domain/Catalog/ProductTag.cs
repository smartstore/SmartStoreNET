using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product tag
    /// </summary>
	[DataContract]
	public partial class ProductTag : BaseEntity, ILocalizedEntity
    {
		private ICollection<Product> _products;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the products
		/// </summary>
		public virtual ICollection<Product> Products
		{
			get { return _products ?? (_products = new HashSet<Product>()); }
			protected set { _products = value; }
		}
    }
}
