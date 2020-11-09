using System.Collections.Generic;
using SmartStore.Services.Search;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Models.Search
{
    public class SearchResultModel : SearchResultModelBase, ISearchResultModel
    {
        public SearchResultModel(CatalogSearchQuery query)
        {
            Query = query;
        }

        public CatalogSearchQuery Query
        {
            get;
            private set;
        }

        public CatalogSearchResult SearchResult
        {
            get;
            set;
        }

        /// <summary>
        /// Contains the original/misspelled search term, when
        /// the search did not match any results and the spell checker
        /// suggested at least one term.
        /// </summary>
        public string AttemptedTerm
        {
            get;
            set;
        }

        public string Term
        {
            get;
            set;
        }

        public ProductSummaryModel TopProducts
        {
            get;
            set;
        }

        public int TotalProductsCount
        {
            get;
            set;
        }

        public override IList<HitGroup> HitGroups { get; protected set; }

        public string Error
        {
            get;
            set;
        }
    }
}