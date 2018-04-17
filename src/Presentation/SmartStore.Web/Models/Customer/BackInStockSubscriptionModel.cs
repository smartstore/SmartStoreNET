using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class BackInStockSubscriptionModel : EntityModelBase
    {
        public int ProductId { get; set; }
        public LocalizedValue<string> ProductName { get; set; }
        public string SeName { get; set; }
    }
}
