using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerForumSubscriptionsModel : PagedListBase
    {
        public CustomerForumSubscriptionsModel(IPageable pageable) : base(pageable)
        {
            this.ForumSubscriptions = new List<ForumSubscriptionModel>();
        }

        public IList<ForumSubscriptionModel> ForumSubscriptions { get; set; }
    }
}