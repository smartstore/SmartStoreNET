using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Models
{
    public class PublicInfoModel : ModelBase
    {
        public string TrustedShopsId { get; set; }
        public bool IsTestMode { get; set; }
        public bool IsExcellenceMode { get; set; }
        public string BuyerEmail { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentType { get; set; }
        public string CustomerId { get; set; }
        public string OrderId { get; set; }
        //public string ProtectionLink { get; set; } 
        public string TrustedShopsProductId { get; set; }

        public string ExcellenceName { get; set; }
        public string ExcellenceDescription { get; set; }
    }
}