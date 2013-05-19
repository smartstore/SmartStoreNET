using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Tax.CountryStateZip.Models
{
    public class TaxRateListModel : ModelBase
    {
        public TaxRateListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            TaxRates = new List<TaxRateModel>();
        }

        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Country")]
        public int AddCountryId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.StateProvince")]
        public int AddStateProvinceId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Zip")]
        public string AddZip { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.TaxCategory")]
        public int AddTaxCategoryId { get; set; }
        [SmartResourceDisplayName("Plugins.Tax.CountryStateZip.Fields.Percentage")]
        public decimal AddPercentage { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        public IList<TaxRateModel> TaxRates { get; set; }
        
    }
}