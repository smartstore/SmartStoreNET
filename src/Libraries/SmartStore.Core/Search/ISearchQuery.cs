using System.Collections.Generic;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Search
{
	public interface ISearchQuery
	{
		// Language
		int? LanguageId { get; }
		string LanguageSeoCode { get; }

		// Search term
		string[] Fields { get; }
		string Term { get; }
		bool EscapeTerm { get; }
		bool IsExactMatch { get; }
		bool IsFuzzySearch { get; }

		// Filtering
		ICollection<ISearchFilter> Filters { get; }

		// Facets
		IReadOnlyDictionary<string, FacetDescriptor> FacetDescriptors { get; }

		// Paging
		int Skip { get; }
		int Take { get; }

		int NumberOfSuggestions { get; }

		// Sorting
		ICollection<SearchSort> Sorting { get; }
	}
}
