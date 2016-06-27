using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
	public class CheckoutAttributeListModel : ModelBase
    {
		public int GridPageSize { get; set; }

		public IList<SelectListItem> AvailableStores { get; set; }
	}
}