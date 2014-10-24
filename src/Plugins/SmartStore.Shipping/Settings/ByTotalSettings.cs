using SmartStore.Core.Configuration;

namespace SmartStore.Shipping
{
    public class ByTotalSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }

        public decimal SmallQuantityThreshold { get; set; }
        public decimal SmallQuantitySurcharge { get; set; }
    }
}
