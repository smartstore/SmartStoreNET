using System;
using System.Linq;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Shipping
{
    public static class ShippingExtentions
    {
        public static bool IsShippingRateComputationMethodActive(this Provider<IShippingRateComputationMethod> srcm, ShippingSettings shippingSettings)
        {
            if (srcm == null)
                throw new ArgumentNullException("srcm");

            if (shippingSettings == null)
                throw new ArgumentNullException("shippingSettings");

            if (shippingSettings.ActiveShippingRateComputationMethodSystemNames == null)
                return false;

            if (!srcm.Value.IsActive)
                return false;

            return shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(srcm.Metadata.SystemName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
