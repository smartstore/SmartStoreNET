
namespace SmartStore.Services.Catalog
{
    public class AdjustInventoryResult
    {
        private int? _stockQuantityOld;
        private int? _stockQuantityNew;

        /// <summary>
        /// The stock quantity before adjustment
        /// </summary>
        public int StockQuantityOld
        {
            get => _stockQuantityOld ?? 0;
            set => _stockQuantityOld = value;
        }

        /// <summary>
        /// The stock quantity after adjustment
        /// </summary>
        public int StockQuantityNew
        {
            get => _stockQuantityNew ?? 0;
            set => _stockQuantityNew = value;
        }

        /// <summary>
        /// Determines whether the adjustment resulted in a clear, unique stock quantity update. For instance false for bundle products.
        /// </summary>
        public bool HasClearStockQuantityResult => _stockQuantityOld.HasValue && _stockQuantityNew.HasValue;
    }
}
