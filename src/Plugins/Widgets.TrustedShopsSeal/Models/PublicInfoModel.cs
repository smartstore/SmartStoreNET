using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal.Models
{
    public class PublicInfoModel : ModelBase
    {
        public string TrustedShopsId { get; set; }
        public string ShopName { get; set; }
        public string ShopText { get; set; }
        public bool IsTestMode { get; set; }
    }
}