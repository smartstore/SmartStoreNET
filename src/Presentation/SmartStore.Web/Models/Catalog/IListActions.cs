using System;
using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Web.Models.Catalog
{
	public interface IListActions
	{
		ProductSummaryViewMode ViewMode { get; }
		bool AllowViewModeChanging { get; }

		bool AllowFiltering { get; }

		bool AllowSorting { get; }
		int? CurrentSortOrder { get; }
		string CurrentSortOrderName { get; }
		IDictionary<int, string> AvailableSortOptions { get; }

		IPageable PagedList { get; }
		IEnumerable<int> AvailablePageSizes { get; }
	}
}