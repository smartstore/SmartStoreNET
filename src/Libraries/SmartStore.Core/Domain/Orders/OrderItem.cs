using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order item
    /// </summary>
	[DataContract]
    public partial class OrderItem : BaseEntity
    {
        private ICollection<GiftCard> _associatedGiftCards;

        /// <summary>
        /// Gets or sets the order product variant identifier
        /// </summary>
		[DataMember]
        public Guid OrderItemGuid { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
		[DataMember]
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
		[DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (incl tax)
        /// </summary>
		[DataMember]
        public decimal UnitPriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the unit price in primary store currency (excl tax)
        /// </summary>
		[DataMember]
        public decimal UnitPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (incl tax)
        /// </summary>
		[DataMember]
        public decimal PriceInclTax { get; set; }

        /// <summary>
        /// Gets or sets the price in primary store currency (excl tax)
        /// </summary>
		[DataMember]
        public decimal PriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rate
        /// </summary>
        [DataMember]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (incl tax)
        /// </summary>
		[DataMember]
        public decimal DiscountAmountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the discount amount (excl tax)
        /// </summary>
		[DataMember]
        public decimal DiscountAmountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the attribute description
        /// </summary>
		[DataMember]
        public string AttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the product variant attributes in XML format
        /// </summary>
		[DataMember]
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the download count
        /// </summary>
        [DataMember]
        public int DownloadCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether download is activated
        /// </summary>
		[DataMember]
        public bool IsDownloadActivated { get; set; }

        /// <summary>
        /// Gets or sets a license download identifier (in case this is a downloadable product)
        /// </summary>
		[DataMember]
        public int? LicenseDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the total weight of one item
        /// It's nullable for compatibility with the previous version where was no such property
        /// </summary>
        [DataMember]
        public decimal? ItemWeight { get; set; }

        /// <summary>
        /// Gets or sets extra bundle data
        /// </summary>
        [DataMember]
        public string BundleData { get; set; }

        /// <summary>
        /// Gets or sets the original product cost
        /// </summary>
        [DataMember]
        public decimal ProductCost { get; set; }

        /// <summary>
        /// Gets or sets the delivery time at the time of purchase.
        /// </summary>
        [DataMember]
        public int? DeliveryTimeId { get; set; }

        /// <summary>
        /// Indicates whether the delivery time was displayed at the time of purchase.
        /// </summary>
        [DataMember]
        public bool DisplayDeliveryTime { get; set; }

        /// <summary>
        /// Gets the order
        /// </summary>
		[DataMember]
        public virtual Order Order { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
		[DataMember]
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the associated gift card
        /// </summary>
        public virtual ICollection<GiftCard> AssociatedGiftCards
        {
            get => _associatedGiftCards ?? (_associatedGiftCards = new HashSet<GiftCard>());
            protected set => _associatedGiftCards = value;
        }
    }
}
