using System;
using System.Linq;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
    public partial class ForumSearchQuery : SearchQuery<ForumSearchQuery>, ICloneable<ForumSearchQuery>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ForumSearchQuery"/> class without a search term being set.
        /// </summary>
        public ForumSearchQuery()
			: base((string[])null, null)
		{
		}

		public ForumSearchQuery(string field, string term, SearchMode mode = SearchMode.Contains, bool escape = true, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, mode, escape, isFuzzySearch)
		{
		}

		public ForumSearchQuery(string[] fields, string term, SearchMode mode = SearchMode.Contains, bool escape = true, bool isFuzzySearch = false)
			: base(fields, term, mode, escape, isFuzzySearch)
		{
		}

		public ForumSearchQuery Clone()
		{
			return (ForumSearchQuery)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}

        #region Fluent builder

        public ForumSearchQuery SortBy(ForumTopicSorting sort)
        {
            switch (sort)
            {
                case ForumTopicSorting.SubjectAsc:
                case ForumTopicSorting.SubjectDesc:
                    return SortBy(SearchSort.ByStringField("subject", sort == ForumTopicSorting.SubjectDesc));

                case ForumTopicSorting.UserNameAsc:
                case ForumTopicSorting.UserNameDesc:
                    return SortBy(SearchSort.ByStringField("username", sort == ForumTopicSorting.UserNameDesc));

                case ForumTopicSorting.CreatedOnAsc:
                case ForumTopicSorting.CreatedOnDesc:
                    return SortBy(SearchSort.ByDateTimeField("createdon", sort == ForumTopicSorting.CreatedOnDesc));

                case ForumTopicSorting.PostsAsc:
                case ForumTopicSorting.PostsDesc:
                    return SortBy(SearchSort.ByIntField("numposts", sort == ForumTopicSorting.PostsDesc));

                case ForumTopicSorting.Relevance:
                    return SortBy(SearchSort.ByRelevance());

                default:
                    return this;
            }
        }

        public override ForumSearchQuery HasStoreId(int id)
        {
            base.HasStoreId(id);

            if (id == 0)
            {
                WithFilter(SearchFilter.ByField("storeid", 0).ExactMatch().NotAnalyzed());
            }
            else
            {
                WithFilter(SearchFilter.Combined(
                    SearchFilter.ByField("storeid", 0).ExactMatch().NotAnalyzed(),
                    SearchFilter.ByField("storeid", id).ExactMatch().NotAnalyzed())
                );
            }

            return this;
        }

        public ForumSearchQuery WithForumIds(params int[] ids)
        {
            if (ids.Length == 0)
            {
                return this;
            }

            return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField("forumid", x).ExactMatch().NotAnalyzed()).ToArray()));
        }

        public ForumSearchQuery WithCustomerIds(params int[] ids)
        {
            if (ids.Length == 0)
            {
                return this;
            }

            return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField("customerid", x).ExactMatch().NotAnalyzed()).ToArray()));
        }

        public ForumSearchQuery CreatedBetween(DateTime? fromUtc, DateTime? toUtc)
        {
            if (fromUtc == null && toUtc == null)
            {
                return this;
            }

            return WithFilter(SearchFilter.ByRange("createdon", fromUtc, toUtc, fromUtc.HasValue, toUtc.HasValue).Mandatory().ExactMatch().NotAnalyzed());
        }

        #endregion
    }
}
