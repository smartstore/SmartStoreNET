using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Shipping.Models
{
    public class ByTotalListModel : ModelBase
    {
        public ByTotalListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Store")]
        public int AddStoreId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public int? AddCountryId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public int? AddStateProvinceId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Zip")]
        public string AddZip { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public int AddShippingMethodId { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.From")]
        public decimal AddFrom { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.To")]
        public decimal? AddTo { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.UsePercentage")]
        public bool AddUsePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage")]
        public decimal AddShippingChargePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount")]
        public decimal AddShippingChargeAmount { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.BaseCharge")]
        public decimal AddBaseCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.MaxCharge")]
        public decimal AddMaxCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.MaxCharge")]
        public decimal? MaxCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.SmallQuantityThreshold")]
        public decimal SmallQuantityThreshold { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.SmallQuantitySurcharge")]
        public decimal SmallQuantitySurcharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.ByTotal.Fields.CalculateTotalIncludingTax")]
        public bool CalculateTotalIncludingTax { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public int GridPageSize { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}