using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    internal class MergedProduct : IMergedProduct
    {
        public MergedProduct()
        {
            this.Id = -1;
            this.ProductVariantAttributeCombinations = new List<ProductVariantAttributeCombination>();
        }
        public MergedProduct(IMergedProduct source)
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
			this.ManageInventoryMethodId = source.ManageInventoryMethodId;
            this.ProductVariantAttributeCombinations = source.ProductVariantAttributeCombinations;

			this.BasePrice_Enabled = source.BasePrice_Enabled;
			this.BasePrice_MeasureUnit = source.BasePrice_MeasureUnit;
			this.BasePrice_Amount = source.BasePrice_Amount;
			this.BasePrice_BaseAmount = source.BasePrice_BaseAmount;
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
		public int ManageInventoryMethodId { get; private set; }

		public bool BasePrice_Enabled { get; set; }
		public string BasePrice_MeasureUnit { get; set; }
		public decimal? BasePrice_Amount { get; set; }
		public int? BasePrice_BaseAmount { get; set; }
		public bool BasePrice_HasValue
		{
			get
			{
				return BasePrice_Enabled && BasePrice_Amount.GetValueOrDefault() > 0 && BasePrice_BaseAmount.GetValueOrDefault() > 0 && BasePrice_MeasureUnit.HasValue();
			}
		}

        public ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations { get; private set; }
    }
}
