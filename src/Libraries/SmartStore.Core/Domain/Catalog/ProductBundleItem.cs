using System;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
	[DataContract]
	public partial class ProductBundleItem : BaseEntity, ILocalizedEntity
	{
		/// <summary>
		/// Gets or sets the product identifier
		/// </summary>
		[DataMember]
		public int ProductId { get; set; }

		/// <summary>
		/// Gets or sets the product identifier of the product bundle
		/// </summary>
		[DataMember]
		public int ParentBundledProductId { get; set; }

		/// <summary>
		/// Gets or sets the quantity
		/// </summary>
		[DataMember]
		public int Quantity { get; set; }

		/// <summary>
		/// Gets or sets the discount in percent
		/// </summary>
		[DataMember]
		public decimal? Discount { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the name should be overwritten
		/// </summary>
		[DataMember]
		public bool OverrideName { get; set; }

		/// <summary>
		/// Gets or sets the name value
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the short description should be overwritten
		/// </summary>
		[DataMember]
		public bool OverrideShortDescription { get; set; }

		/// <summary>
		/// Gets or sets the name value
		/// </summary>
		[DataMember]
		public string ShortDescription { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to hide the thumbnail
		/// </summary>
		[DataMember]
		public bool HideThumbnail { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is published
		/// </summary>
		[DataMember]
		public bool Published { get; set; }

		/// <summary>
		/// Gets or sets a display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the date and time of product bundle item creation
		/// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the date and time of product bundle item update
		/// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets the product
		/// </summary>
		public virtual Product Product { get; set; }

		/// <summary>
		/// Gets the parent bundled product
		/// </summary>
		public virtual Product ParentBundledProduct { get; set; }
	}
}
