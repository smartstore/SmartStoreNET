using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal	
{
    public class TrustedShopsSealSettings : ISettings
    {
        public string TrustedShopsId { get; set; }
        public bool IsTestMode { get; set; }
        public string WidgetZone { get; set; }
        public string ShopName { get; set; }
        public string ShopText { get; set; }

    }
}