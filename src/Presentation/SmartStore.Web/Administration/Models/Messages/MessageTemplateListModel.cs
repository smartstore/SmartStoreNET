using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Messages
{
	public class MessageTemplateListModel : ModelBase
	{
		public MessageTemplateListModel()
		{
			AvailableStores = new List<SelectListItem>();
		}

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.SearchStore")]
		public int SearchStoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
	}
}