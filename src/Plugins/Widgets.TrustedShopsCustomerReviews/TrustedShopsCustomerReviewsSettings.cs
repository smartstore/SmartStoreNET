using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews	
{
    public class TrustedShopsCustomerReviewsSettings : ISettings
    {
        public string TrustedShopsId { get; set; }
        public bool IsTestMode { get; set; }
        public bool ActivationState { get; set; }
        public string WidgetZone { get; set; }
        public string ShopName { get; set; }
        public bool DisplayWidget { get; set; }
        public bool DisplayReviewLinkOnOrderCompleted { get; set; }
        public bool DisplayReviewLinkInEmail { get; set; }
    }
}