using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Shipping.Models
{
    public class ByTotalModel : EntityModelBase
    {
		[SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Store")]
		public int StoreId { get; set; }
		[SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Store")]
		public string StoreName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public int? CountryId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public string CountryName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Zip")]
        public string Zip { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.From")]
        public decimal From { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.To")]
        public decimal? To { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage")]
        public decimal ShippingChargePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount")]
        public decimal ShippingChargeAmount { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.BaseCharge")]
        public decimal BaseCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.MaxCharge")]
        public decimal? MaxCharge { get; set; }

    }
}
