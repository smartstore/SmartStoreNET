using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Messages
{
    public class NewsLetterSubscriptionListModel : ModelBase
    {
		public NewsLetterSubscriptionListModel()
		{
			AvailableStores = new List<SelectListItem>();
		}

		public int GridPageSize { get; set; }

        public GridModel<NewsLetterSubscriptionModel> NewsLetterSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int StoreId { get; set; }

		public IList<SelectListItem> AvailableStores { get; set; }
    }
}