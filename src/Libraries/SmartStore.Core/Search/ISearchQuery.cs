using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search
{
	public interface ISearchQuery
	{
		// language
		int? LanguageId { get; }

		// Search term
		string[] Fields { get; }
		string Term { get; }
		bool EscapeTerm { get; }
		bool IsFuzzySearch { get; }

		// Filtering
		ICollection<SearchFilter> Filters { get; }

		// Paging
		int Skip { get; }
		int Take { get; }

		// sorting
		ICollection<SearchSort> Sorting { get; }
	}
}
