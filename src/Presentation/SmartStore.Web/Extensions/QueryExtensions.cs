using System;
using SmartStore.Services.Search;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web
{
	public static class QueryExtensions
	{
		public static ProductSummaryViewMode GetViewMode(this CatalogSearchQuery query)
		{
			Guard.NotNull(query, nameof(query));

			var viewMode = query.CustomData.Get("ViewMode") as string;

			if (viewMode != null && viewMode.IsCaseInsensitiveEqual("list"))
			{
				return ProductSummaryViewMode.List;
			}

			return ProductSummaryViewMode.Grid;
		}
	}
}