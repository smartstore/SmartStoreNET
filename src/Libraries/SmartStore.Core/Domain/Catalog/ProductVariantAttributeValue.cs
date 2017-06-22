using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute value
    /// </summary>
    [DataContract]
	public partial class ProductVariantAttributeValue : BaseEntity, ILocalizedEntity, ISearchAlias
	{
        /// <summary>
        /// Gets or sets the product variant attribute mapping identifier
        /// </summary>
		[DataMember]
		[Index("IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder", 1)]
		public int ProductVariantAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the product variant attribute alias 
        /// </summary>
		[DataMember]
		public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the product variant attribute name
        /// </summary>
		[DataMember]
		[Index]
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
        /// Gets or sets a value indicating whether the value is pre-selected
        /// </summary>
		[DataMember]
		public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		[Index("IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder", 2)]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets the type Id
		/// </summary>
		[DataMember]
		[Index]
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
        /// Gets the product variant attribute
        /// </summary>
		[DataMember]
		public virtual ProductVariantAttribute ProductVariantAttribute { get; set; }

		/// <summary>
		/// Gets or sets the product attribute value type
		/// </summary>
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
    }
}
