using System.Collections.Generic;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Models.Boards
{
    public class ForumSearchResultModel : ModelBase, IForumSearchResultModel
    {
        public ForumSearchResultModel(ForumSearchQuery query)
        {
            Query = query;
            ForumTopics = new List<ForumTopicRowModel>();
        }

        public ForumSearchQuery Query { get; private set; }

        public ForumSearchResult SearchResult { get; set; }

        /// <summary>
        /// Contains the original/misspelled search term, when the search did not match any results 
        /// and the spell checker suggested at least one term.
        /// </summary>
        public string AttemptedTerm { get; set; }
        public string Term { get; set; }

        public int TotalCount { get; set; }
        public int PostsPageSize { get; set; }
        public string Error { get; set; }

        public bool AllowSorting { get; set; }
        public int? CurrentSortOrder { get; set; }
        public string CurrentSortOrderName { get; set; }
        public IDictionary<int, string> AvailableSortOptions { get; set; }

        public IList<ForumTopicRowModel> ForumTopics { get; set; }
    }
}