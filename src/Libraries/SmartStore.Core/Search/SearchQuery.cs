using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Search
{
	public class SearchQuery : SearchQuery<SearchQuery>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SearchQuery"/> class without a search term being set
		/// </summary>
		public SearchQuery()
			: base((string[])null, null)
		{
		}

		public SearchQuery(string field, string term, bool escape = false, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, escape, isFuzzySearch)
		{
		}

		public SearchQuery(string[] fields, string term, bool escape = false, bool isFuzzySearch = false)
			: base(fields, term, escape, isFuzzySearch)
		{
		}
	}

	public class SearchQuery<TQuery> : ISearchQuery where TQuery : class, ISearchQuery
	{
		private readonly Dictionary<string, FacetDescriptor> _facetDescriptors;

		protected SearchQuery(string[] fields, string term, bool escape = false, bool isFuzzySearch = false)
		{
			Fields = fields;
			Term = term;
			EscapeTerm = escape;
			IsFuzzySearch = isFuzzySearch;

			Filters = new List<ISearchFilter>();
			Sorting = new List<SearchSort>();
			_facetDescriptors = new Dictionary<string, FacetDescriptor>(StringComparer.OrdinalIgnoreCase);

			Take = int.MaxValue;
		}

		// language
		public int? LanguageId { get; protected set; }
		public string LanguageSeoCode { get; protected set; }

		// Search term
		public string[] Fields { get; protected set; }
		public string Term { get; protected set; }
		public bool EscapeTerm { get; protected set; }
		public bool IsFuzzySearch { get; protected set; }

		// Filtering
		public ICollection<ISearchFilter> Filters { get; }

		// Facets
		public IReadOnlyDictionary<string, FacetDescriptor> FacetDescriptors
		{
			get
			{
				return _facetDescriptors;
			}
		}

		// Paging
		public int Skip { get; protected set; }
		public int Take { get; protected set; }
		public int PageIndex
		{
			get
			{
				return Math.Max((Skip - 1) / Take, 0);
			}
		}

		// sorting
		public ICollection<SearchSort> Sorting { get; }

		#region Fluent builder

		public TQuery WithLanguage(Language language)
		{
			Guard.NotNull(language, nameof(language));

			LanguageId = language.Id;
			LanguageSeoCode = language.UniqueSeoCode.EmptyNull().ToLower();

			return (this as TQuery);
		}

		public TQuery Slice(int skip, int take)
		{
			Guard.NotNegative(skip, nameof(skip));
			Guard.IsPositive(take, nameof(take));

			Skip = skip;
			Take = take;

			return (this as TQuery);
		}

		public TQuery WithFilter(ISearchFilter filter)
		{
			Guard.NotNull(filter, nameof(filter));

			Filters.Add(filter);

			return (this as TQuery);
		}

		public TQuery SortBy(SearchSort sort)
		{
			Guard.NotNull(sort, nameof(sort));

			Sorting.Add(sort);

			return (this as TQuery);
		}

		public TQuery AddFacetDescriptor(FacetDescriptor facetDescription)
		{
			Guard.NotNull(facetDescription, nameof(facetDescription));

			if (_facetDescriptors.ContainsKey(facetDescription.Key))
			{
				throw new InvalidOperationException("A facet description object with the same key has already been added. Key: {0}".FormatInvariant(facetDescription.Key));
			}

			_facetDescriptors.Add(facetDescription.Key, facetDescription);

			return (this as TQuery);
		}

		#endregion
	}
}
