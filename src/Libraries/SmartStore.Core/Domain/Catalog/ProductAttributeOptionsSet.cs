using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
	[DataContract]
	public partial class ProductAttributeOptionsSet : BaseEntity, ILocalizedEntityRelation
	{
		private ICollection<ProductAttributeOption> _productAttributeOptions;

		/// <summary>
		/// Gets or sets the name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the product attribute identifier
		/// </summary>
		[DataMember]
		public int ProductAttributeId { get; set; }

		/// <summary>
		/// Gets the product attribute
		/// </summary>
		[DataMember]
		public virtual ProductAttribute ProductAttribute { get; set; }

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
