using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute combination
    /// </summary>
    [DataContract]
    public partial class ProductVariantAttributeCombination : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the attributes
        /// </summary>
		[DataMember]
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
		[DataMember]
        [Index("IX_StockQuantity_AllowOutOfStockOrders", 1)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow orders when out of stock
        /// </summary>
		[DataMember]
        [Index("IX_StockQuantity_AllowOutOfStockOrders", 2)]
        public bool AllowOutOfStockOrders { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }
    }
}
