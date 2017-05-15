using System;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search;

namespace SmartStore.Core.Domain.Catalog
{
	/// <summary>
	/// Represents a product attribute option
	/// </summary>
	[DataContract]
	public partial class ProductAttributeOption : BaseEntity, ILocalizedEntity, ISearchAlias, ICloneable<ProductVariantAttributeValue>
	{
		/// <summary>
		/// Gets or sets the product attribute options set identifier
		/// </summary>
		[DataMember]
		public int ProductAttributeOptionsSetId { get; set; }

		/// <summary>
		/// Gets or sets the option alias
		/// </summary>
		[DataMember]
		public string Alias { get; set; }

		/// <summary>
		/// Gets or sets the option name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Picture Id
		/// </summary>
		[DataMember]
		public int PictureId { get; set; }

		/// <summary>
		/// Gets or sets the color RGB value (used with "Boxes" attribute type)
		/// </summary>
		[DataMember]
		public string Color { get; set; }

		/// <summary>
		/// Gets or sets the price adjustment
		/// </summary>
		[DataMember]
		public decimal PriceAdjustment { get; set; }

		/// <summary>
		/// Gets or sets the weight adjustment
		/// </summary>
		[DataMember]
		public decimal WeightAdjustment { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the option is pre-selected
		/// </summary>
		[DataMember]
		public bool IsPreSelected { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the type Id
		/// </summary>
		[DataMember]
		public int ValueTypeId { get; set; }

		/// <summary>
		/// Gets or sets the linked product Id
		/// </summary>
		[DataMember]
		public int LinkedProductId { get; set; }

		/// <summary>
		/// Gets or sets the quantity for the linked product
		/// </summary>
		[DataMember]
		public int Quantity { get; set; }

		/// <summary>
		/// Gets the product attribute options set
		/// </summary>
		[DataMember]
		public virtual ProductAttributeOptionsSet ProductAttributeOptionsSet { get; set; }

		/// <summary>
		/// Gets or sets the product attribute value type
		/// </summary>
		[DataMember]
		public ProductVariantAttributeValueType ValueType
		{
			get
			{
				return (ProductVariantAttributeValueType)this.ValueTypeId;
			}
			set
			{
				this.ValueTypeId = (int)value;
			}
		}

		public ProductVariantAttributeValue Clone()
		{
			var value = new ProductVariantAttributeValue();
			value.Alias = Alias;
			value.Name = Name;
			value.PictureId = PictureId;
			value.Color = Color;
			value.PriceAdjustment = PriceAdjustment;
			value.WeightAdjustment = WeightAdjustment;
			value.IsPreSelected = IsPreSelected;
			value.DisplayOrder = DisplayOrder;
			value.ValueTypeId = ValueTypeId;
			value.LinkedProductId = LinkedProductId;
			value.Quantity = Quantity;

			return value;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}
}
