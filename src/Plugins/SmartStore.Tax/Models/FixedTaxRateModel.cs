using SmartStore.Web.Framework;

namespace SmartStore.Tax.Models
{
    public class FixedTaxRateModel
    {
        public int TaxCategoryId { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.FixedRate.Fields.TaxCategoryName")]
        public string TaxCategoryName { get; set; }

        [SmartResourceDisplayName("Plugins.Tax.FixedRate.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}