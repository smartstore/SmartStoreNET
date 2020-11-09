using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Models.Tax
{
    public class TaxProviderModel : ProviderModel
    {
        [SmartResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.IsPrimaryTaxProvider")]
        public bool IsPrimaryTaxProvider { get; set; }
    }
}