using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Customer
{
    public partial class BackInStockSubscriptionModel : EntityModelBase
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SeName { get; set; }
    }
}
