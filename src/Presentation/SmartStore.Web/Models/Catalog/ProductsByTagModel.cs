using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductsByTagModel : EntityModelBase, ISearchResultModel
    {
        public LocalizedValue<string> TagName { get; set; }
        public ProductSummaryModel Products { get; set; }

        public CatalogSearchResult SearchResult
        {
            get;
            set;
        }
    }
}