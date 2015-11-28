
using SmartStore.Core.Configuration;

namespace SmartStore.ShippingByWeight
{
    public class ShippingByWeightSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }

        public bool CalculatePerWeightUnit { get; set; }
    }
}