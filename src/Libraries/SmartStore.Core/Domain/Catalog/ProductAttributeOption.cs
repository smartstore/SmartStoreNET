using System;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
	/// <summary>
	/// Represents a product attribute option
	/// </summary>
	[DataContract]
	public partial class ProductAttributeOption : BaseEntity, ILocalizedEntity, ICloneable<ProductVariantAttributeValue>
	{
		/// <summary>
		/// Gets or sets the product attribute identifier
		/// </summary>
		[DataMember]
		public int ProductAttributeId { get; set; }

		/// <summary>
		/// Gets or sets the product variant attribute alias
		/// </summary>
		[DataMember]
		public string Alias { get; set; }

		/// <summary>
		/// Gets or sets the product variant attribute name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Picture Id
		/// </summary>
		[DataMember]
		public int PictureId { get; set; }

		/// <summary>
		/// Gets or sets the color RGB value (used with "Color squares" attribute type)
		/// </summary>
		[DataMember]
		public string ColorSquaresRgb { get; set; }

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
		/// Gets or sets a value indicating whether the value is pre-selected
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
		/// Gets the product attribute
		/// </summary>
		[DataMember]
		public virtual ProductAttribute ProductAttribute { get; set; }

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
			value.ColorSquaresRgb = ColorSquaresRgb;
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
