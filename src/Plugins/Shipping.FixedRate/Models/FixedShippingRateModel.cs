using SmartStore.Web.Framework;

namespace SmartStore.Plugin.Shipping.FixedRate.Models
{
    public class FixedShippingRateModel
    {
        public int ShippingMethodId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.ShippingMethodName")]
        public string ShippingMethodName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}