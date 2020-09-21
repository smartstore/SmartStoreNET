using SmartStore.Licensing;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Plugins
{
    public class LicenseLabelModel : ModelBase
    {
        public string LicenseUrl { get; set; }
        public bool IsLicensable { get; set; }
        public bool HideLabel { get; set; }
        public LicensingState LicenseState { get; set; }
        public string TruncatedLicenseKey { get; set; }
        public int? RemainingDemoUsageDays { get; set; }
    }
}