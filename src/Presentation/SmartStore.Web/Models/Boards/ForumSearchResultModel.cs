using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Search;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Boards
{
    public class ForumSearchResultModel : SearchResultModelBase, IForumSearchResultModel, IListActions
    {
        public ForumSearchResultModel(ForumSearchQuery query)
        {
            Query = query;
        }

        public ForumSearchQuery Query { get; private set; }
        public ForumSearchResult SearchResult { get; set; }

        /// <summary>
        /// Contains the original/misspelled search term, when the search did not match any results 
        /// and the spell checker suggested at least one term.
        /// </summary>
        public string AttemptedTerm { get; set; }
        public string Term { get; set; }

        public int CumulativeHitCount { get; set; }
        public int TotalCount { get; set; }
        public int PostsPageSize { get; set; }
        public string Error { get; set; }

        public override IList<HitGroup> HitGroups { get; protected set; }

        #region IListActions

        public bool AllowViewModeChanging => throw new NotImplementedException();
        public ProductSummaryViewMode ViewMode => throw new NotImplementedException();
        public GridColumnSpan GridColumnSpan => throw new NotImplementedException();

        public bool AllowFiltering => true;

        public bool AllowSorting { get; set; }
        public int? CurrentSortOrder { get; set; }
        public string CurrentSortOrderName { get; set; }
        public string RelevanceSortOrderName { get; set; }
        public IDictionary<int, string> AvailableSortOptions { get; set; }

        public IPageable PagedList { get; set; }
        public IEnumerable<int> AvailablePageSizes => throw new NotImplementedException();

        #endregion
    }
}