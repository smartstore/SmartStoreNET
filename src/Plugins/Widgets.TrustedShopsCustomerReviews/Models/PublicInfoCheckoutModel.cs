using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Models
{
    public class PublicInfoCheckoutModel : ModelBase
    {
        public string TrustedShopsId { get; set; }
        public string BuyerEmail { get; set; }
        public string ShopOrderId { get; set; }
        public bool DisplayReviewLinkOnOrderCompleted { get; set; }
    }
}