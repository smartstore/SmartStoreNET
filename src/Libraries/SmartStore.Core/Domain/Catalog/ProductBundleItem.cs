using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Catalog
{
    [DataContract]
    public partial class ProductBundleItem : BaseEntity, IAuditable, ILocalizedEntity, ICloneable<ProductBundleItem>
    {
        private ICollection<ProductBundleItemAttributeFilter> _attributeFilters;

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        [DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier of the bundle product
        /// </summary>
        [DataMember]
        public int BundleProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        [DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the discount value
        /// </summary>
        [DataMember]
        public decimal? Discount { get; set; }

        /// <summary>
        /// Gets or sets whether the discount is in percent
        /// </summary>
        [DataMember]
        public bool DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the name value
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name value
        /// </summary>
        [DataMember]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to filter attributes
        /// </summary>
        [DataMember]
        public bool FilterAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the thumbnail
        /// </summary>
        [DataMember]
        public bool HideThumbnail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is visible
        /// </summary>
        [DataMember]
        public bool Visible { get; set; }

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
        [DataMember]
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets the bundle product
        /// </summary>
        [DataMember]
        public virtual Product BundleProduct { get; set; }

        /// <summary>
        /// Gets or sets the collection of attribute filters
        /// </summary>
        public virtual ICollection<ProductBundleItemAttributeFilter> AttributeFilters
        {
            get => _attributeFilters ?? (_attributeFilters = new HashSet<ProductBundleItemAttributeFilter>());
            protected set => _attributeFilters = value;
        }

        public ProductBundleItem Clone()
        {
            var bundleItem = new ProductBundleItem
            {
                ProductId = this.ProductId,
                BundleProductId = this.BundleProductId,
                Quantity = this.Quantity,
                Discount = this.Discount,
                DiscountPercentage = this.DiscountPercentage,
                Name = this.Name,
                ShortDescription = this.ShortDescription,
                FilterAttributes = this.FilterAttributes,
                HideThumbnail = this.HideThumbnail,
                Visible = this.Visible,
                Published = this.Published,
                DisplayOrder = this.DisplayOrder,
                CreatedOnUtc = this.CreatedOnUtc,
                UpdatedOnUtc = this.UpdatedOnUtc
            };
            return bundleItem;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
