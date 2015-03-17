using SmartStore.Web.Framework;

namespace SmartStore.Shipping.Models
{
    public class FixedRateModel
    {
        public int ShippingMethodId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.ShippingMethodName")]
        public string ShippingMethodName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}