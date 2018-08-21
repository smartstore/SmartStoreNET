using System;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
    public partial class ForumSearchQuery : SearchQuery<ForumSearchQuery>, ICloneable<ForumSearchQuery>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ForumSearchQuery"/> class without a search term being set
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
	}
}
