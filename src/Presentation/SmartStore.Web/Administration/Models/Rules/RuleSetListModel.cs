using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Rules
{
	public class RuleSetListModel : ModelBase
	{
		public RuleSetListModel()
		{
			AvailableStores = new List<SelectListItem>();
		}

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int SearchStoreId { get; set; }

		[SmartResourceDisplayName("Admin.Rules.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Admin.Rules.Title")]
		public string Title { get; set; }

		public IList<SelectListItem> AvailableStores { get; set; }
	}
}