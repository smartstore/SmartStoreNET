using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute mapping
    /// </summary>
    [DataContract]
    public partial class ProductVariantAttribute : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductVariantAttributeValue> _productVariantAttributeValues;

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        [Index("IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder", 1)]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute identifier
        /// </summary>
		[DataMember]
        public int ProductAttributeId { get; set; }

        /// <summary>
        /// Gets or sets a value a text prompt
        /// </summary>
		[DataMember]
        public string TextPrompt { get; set; }

        /// <summary>
        /// Gets or sets any custom data.
        /// It's not used by Smartstore but is being passed to the choice partial view.
        /// </summary>
        [DataMember]
        [MaxLength]
        public string CustomData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is required
        /// </summary>
		[DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
		[DataMember]
        [Index]
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        [Index("IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder", 2)]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the attribute control type
        /// </summary>
		[DataMember]
        public AttributeControlType AttributeControlType
        {
            get => (AttributeControlType)this.AttributeControlTypeId;
            set => this.AttributeControlTypeId = (int)value;
        }

        /// <summary>
        /// Returns a value whether the attribute has a list of values.
        /// </summary>
        public bool IsListTypeAttribute()
        {
            switch (AttributeControlType)
            {
                case AttributeControlType.Checkboxes:
                case AttributeControlType.Boxes:
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns a value whether the selection of multiple values is supported.
        /// </summary>
        [NotMapped]
        public bool IsMultipleChoice
        {
            get => AttributeControlType == AttributeControlType.Checkboxes;
        }

        /// <summary>
        /// Gets the product attribute
        /// </summary>
		[DataMember]
        public virtual ProductAttribute ProductAttribute { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
		public virtual Product Product { get; set; }

        /// <summary>
        /// Gets the product variant attribute values
        /// </summary>
        [DataMember]
        public virtual ICollection<ProductVariantAttributeValue> ProductVariantAttributeValues
        {
            get => _productVariantAttributeValues ?? (_productVariantAttributeValues = new HashSet<ProductVariantAttributeValue>());
            protected set => _productVariantAttributeValues = value;
        }

    }

}
