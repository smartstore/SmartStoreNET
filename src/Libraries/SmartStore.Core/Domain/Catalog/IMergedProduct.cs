using System.Collections.Generic;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Interface for shared product data between products and variant combinations
    /// </summary>
    public interface IMergedProduct
    {
        int Id { get; }
        string Sku { get; set; }
        string Gtin { get; set; }
        string ManufacturerPartNumber { get; set; }
        int StockQuantity { get; set; }
        int? DeliveryTimeId { get; set; }
        decimal Length { get; set; }
        decimal Width { get; set; }
        decimal Height { get; set; }
		int ManageInventoryMethodId { get; }

		bool BasePrice_Enabled { get; set; }
		string BasePrice_MeasureUnit { get; set; }
		decimal? BasePrice_Amount { get; set; }
		int? BasePrice_BaseAmount { get; set; }
		bool BasePrice_HasValue { get; }

        ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; }
    }
}
