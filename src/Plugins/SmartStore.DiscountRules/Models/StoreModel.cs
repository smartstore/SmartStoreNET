using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.DiscountRules.Models
{
	public class StoreModel : DiscountRuleModelBase
    {
		public StoreModel()
        {
			AvailableStores = new List<SelectListItem>();
        }
		[SmartResourceDisplayName("Plugins.DiscountRules.Store.Fields.Store")]
		public int StoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
    }
}