using System.Collections.Generic;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Search
{
	public interface ISearchQuery
	{
		// language
		int? LanguageId { get; }
		string LanguageSeoCode { get; }

		// Search term
		string[] Fields { get; }
		string Term { get; }
		bool EscapeTerm { get; }
		bool IsFuzzySearch { get; }

		// Filtering
		ICollection<ISearchFilter> Filters { get; }

		// Facets
		ICollection<FacetSelection> FacetSelections { get; }

		// Paging
		int Skip { get; }
		int Take { get; }

		// sorting
		ICollection<SearchSort> Sorting { get; }
	}
}
