using SmartStore.Services.Search;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web
{
    public static class QueryExtensions
    {
        public static ProductSummaryViewMode GetViewMode(this CatalogSearchQuery query)
        {
            Guard.NotNull(query, nameof(query));

            if (query.CustomData.Get("ViewMode") is string viewMode && viewMode.IsCaseInsensitiveEqual("list"))
            {
                return ProductSummaryViewMode.List;
            }

            return ProductSummaryViewMode.Grid;
        }
    }
}