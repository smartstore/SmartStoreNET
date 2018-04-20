using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling
{
	public interface IStoreSelector
	{
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		bool LimitedToStores { get; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		IEnumerable<SelectListItem> AvailableStores { get; }

		int[] SelectedStoreIds { get; }
	}
}
