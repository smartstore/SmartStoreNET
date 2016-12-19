using System.Collections.Generic;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Search
{
	public interface ISearchQuery
	{
		// Language & STore
		int? LanguageId { get; }
		string LanguageSeoCode { get; }
		int? StoreId { get; }

		// Search term
		string[] Fields { get; }
		string Term { get; }
		SearchMode Mode { get; }
		bool EscapeTerm { get; }
		bool IsFuzzySearch { get; }

		// Filtering
		ICollection<ISearchFilter> Filters { get; }

		// Facets
		IReadOnlyDictionary<string, FacetDescriptor> FacetDescriptors { get; }

		// Paging
		int Skip { get; }
		int Take { get; }

		// Sorting
		ICollection<SearchSort> Sorting { get; }

		/// <summary>
		/// Maximum number of suggestions returned from spell checker
		/// </summary>
		int SpellCheckerMaxSuggestions { get; }

		/// <summary>
		/// Defines how many characters must be in the query before suggestions are provided
		/// </summary>
		int SpellCheckerMinQueryLength { get; }

		// Misc
		string Origin { get; }
		IDictionary<string, object> CustomData { get; }
	}
}
