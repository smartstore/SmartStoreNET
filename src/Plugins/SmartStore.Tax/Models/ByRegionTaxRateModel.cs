using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Tax.Models
{
    public class ByRegionTaxRateModel : EntityModelBase
    {
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public int TaxCategoryId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public string TaxCategoryName { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public int CountryId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public string CountryName { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Zip")]
        public string Zip { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Percentage")]
        public decimal Percentage { get; set; }
    }
}