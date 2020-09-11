using System.Collections.Generic;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Core.Search
{
    public class ForumSearchSettings : ISettings
    {
        public ForumSearchSettings()
        {
            SearchMode = SearchMode.Contains;
            SearchFields = new List<string> { "username", "text" };
            DefaultSortOrder = ForumTopicSorting.Relevance;
            InstantSearchEnabled = true;
            InstantSearchNumberOfHits = 10;
            InstantSearchTermMinLength = 2;
            FilterMinHitCount = 1;
            FilterMaxChoicesCount = 20;

            ForumDisplayOrder = 1;
            CustomerDisplayOrder = 2;
            DateDisplayOrder = 3;
        }

        /// <summary>
        /// Gets or sets the search mode.
        /// </summary>
        public SearchMode SearchMode { get; set; }

        /// <summary>
        /// Gets or sets name of fields to be searched. The name field is always searched.
        /// </summary>
        public List<string> SearchFields { get; set; }

        /// <summary>
        /// Gets or sets the default sort order in search results.
        /// </summary>
        public ForumTopicSorting DefaultSortOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether instant-search is enabled.
        /// </summary>
        public bool InstantSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of hits to return when using "instant-search" feature.
        /// </summary>
        public int InstantSearchNumberOfHits { get; set; }

        /// <summary>
        /// Gets or sets a minimum instant-search term length.
        /// </summary>
        public int InstantSearchTermMinLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum hit count for a filter value. Values with a lower hit count are not displayed.
        /// </summary>
        public int FilterMinHitCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of filter values to be displayed.
        /// </summary>
        public int FilterMaxChoicesCount { get; set; }

        #region Common facet settings

        public bool ForumDisabled { get; set; }
        public bool CustomerDisabled { get; set; }
        public bool DateDisabled { get; set; }

        public int ForumDisplayOrder { get; set; }
        public int CustomerDisplayOrder { get; set; }
        public int DateDisplayOrder { get; set; }

        #endregion
    }
}
