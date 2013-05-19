
using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Shipping.ByWeight
{
    public class ShippingByWeightSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }

        public bool CalculatePerWeightUnit { get; set; }
    }
}