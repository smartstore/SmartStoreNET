using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.ShippingByWeight.Models
{
    public class ShippingByWeightListModel : ModelBase
    {
        public ShippingByWeightListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
			AvailableStores = new List<SelectListItem>();
        }

		[SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Store")]
		public int AddStoreId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Country")]
        public int AddCountryId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.Zip")]
        public string AddZip { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingMethod")]
        public int AddShippingMethodId { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.From")]
        public decimal AddFrom { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.To")]
        public decimal AddTo { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.UsePercentage")]
        public bool AddUsePercentage { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargePercentage")]
        public decimal AddShippingChargePercentage { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.ShippingChargeAmount")]
        public decimal AddShippingChargeAmount { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.SmallQuantitySurcharge")]
        public decimal SmallQuantitySurcharge { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.SmallQuantityThreshold")]
        public decimal SmallQuantityThreshold { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.CalculatePerWeightUnit")]
        public bool CalculatePerWeightUnit { get; set; }

		[SmartResourceDisplayName("Plugins.Shipping.ByWeight.Fields.IncludeWeightOfFreeShippingProducts")]
		public bool IncludeWeightOfFreeShippingProducts { get; set; }

		public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseWeightIn { get; set; }

		public int GridPageSize { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
    }
}