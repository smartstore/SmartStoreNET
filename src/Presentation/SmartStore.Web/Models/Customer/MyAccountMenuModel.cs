using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class MyAccountMenuModel : ModelBase
    {
        public bool HideInfo { get; set; }
        public bool HideAddresses { get; set; }
        public bool HideOrders { get; set; }
        public bool HideBackInStockSubscriptions { get; set; }
        public bool HideReturnRequests { get; set; }
        public bool HideDownloadableProducts { get; set; }
        public bool HideRewardPoints { get; set; }
        public bool HideChangePassword { get; set; }
        public bool HideAvatar { get; set; }
        public bool HideForumSubscriptions { get; set; }
        public bool HidePrivateMessages { get; set; }
        public int UnreadMessageCount { get; set; }
        
        public string SelectedItemToken { get; set; }
    }
}