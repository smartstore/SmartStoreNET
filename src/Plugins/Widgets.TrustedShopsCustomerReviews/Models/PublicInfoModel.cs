using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Models
{
    public class PublicInfoModel : ModelBase
    {
        public string TrustedShopsId { get; set; }
        public string ShopName { get; set; }
        public bool DisplayWidget { get; set; }
    }
}