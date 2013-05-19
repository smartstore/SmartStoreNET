using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Messages
{
    public class NewsLetterSubscriptionListModel : ModelBase
    {
        public GridModel<NewsLetterSubscriptionModel> NewsLetterSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }
    }
}