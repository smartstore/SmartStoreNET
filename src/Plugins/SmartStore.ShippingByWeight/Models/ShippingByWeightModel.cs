using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.ShippingByWeight.Models
{
    public class ShippingByWeightModel : EntityModelBase
    {
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Store")]
        public int StoreId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Store")]
        public string StoreName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public int CountryId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public string CountryName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.From")]
        public decimal From { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.To")]
        public decimal To { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Zip")]
        public string Zip { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage")]
        public decimal ShippingChargePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount")]
        public decimal ShippingChargeAmount { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.SmallQuantitySurcharge")]
        public decimal SmallQuantitySurcharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.SmallQuantityThreshold")]
        public decimal SmallQuantityThreshold { get; set; }
    }
}