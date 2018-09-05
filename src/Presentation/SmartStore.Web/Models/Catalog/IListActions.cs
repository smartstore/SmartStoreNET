using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Catalog
{
    public interface IListActions
	{
		ProductSummaryViewMode ViewMode { get; }
		GridColumnSpan GridColumnSpan { get; }
		bool AllowViewModeChanging { get; }

		bool AllowFiltering { get; }

		bool AllowSorting { get; }
		int? CurrentSortOrder { get; }
		string CurrentSortOrderName { get; }
		IDictionary<int, string> AvailableSortOptions { get; }

        PagedListModel PagedList { get; }
	}
}