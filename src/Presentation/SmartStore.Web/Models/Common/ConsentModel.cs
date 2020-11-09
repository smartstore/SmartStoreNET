using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class CookieConsentModel : ModelBase
    {
        public string BadgeText { get; set; }
    }

    public partial class GdprConsentModel : ModelBase
    {
        public bool GdprConsent { get; set; }
        public bool SmallDisplay { get; set; }
    }
}