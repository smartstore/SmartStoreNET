using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    internal class MergedProduct : IProduct
    {
        public MergedProduct()
        {
            this.Id = -1;
            this.BasePrice = new BasePriceQuotation();
            this.ProductVariantAttributeCombinations = new List<ProductVariantAttributeCombination>();
        }
        public MergedProduct(IProduct source)
        {
            Guard.ArgumentNotNull(source, "source");

            this.Id = source.Id;
            this.Sku = source.Sku;
            this.Gtin = source.Gtin;
            this.ManufacturerPartNumber = source.ManufacturerPartNumber;
            this.StockQuantity = source.StockQuantity;
            this.DeliveryTimeId = source.DeliveryTimeId;
            this.Length = source.Length;
            this.Width = source.Width;
            this.Height = source.Height;
            this.BasePrice = source.BasePrice;
			this.ManageInventoryMethodId = source.ManageInventoryMethodId;
            this.ProductVariantAttributeCombinations = source.ProductVariantAttributeCombinations;
        }

        public int Id { get; private set; }
        public string Sku { get; set; }
        public string Gtin { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public int StockQuantity { get; set; }
        public int? DeliveryTimeId { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public BasePriceQuotation BasePrice { get; set; }
		public int ManageInventoryMethodId { get; private set; }

        public ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; private set; }
    }
}
