using SmartStore.Core.Configuration;

namespace SmartStore.Shipping
{
    public class ShippingByTotalSettings : ISettings
    {
        public ShippingByTotalSettings()
        {
            CalculateTotalIncludingTax = true;
        }

        public bool LimitMethodsToCreated { get; set; }
        public bool CalculateTotalIncludingTax { get; set; }

        public decimal SmallQuantityThreshold { get; set; }
        public decimal SmallQuantitySurcharge { get; set; }
    }
}
