using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Shipping.ByTotal
{
    public class ShippingByTotalSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }
    }
}
