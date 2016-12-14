using System;
using System.Collections.Generic;
using System.Text;
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

		public SearchQuery(string field, string term, bool escape = false, bool isExactMatch = false, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, escape, isExactMatch, isFuzzySearch)
		{
		}

		public SearchQuery(string[] fields, string term, bool escape = false, bool isExactMatch = false, bool isFuzzySearch = false)
			: base(fields, term, escape, isExactMatch, isFuzzySearch)
		{
		}
	}

	public class SearchQuery<TQuery> : ISearchQuery where TQuery : class, ISearchQuery
	{
		private readonly Dictionary<string, FacetDescriptor> _facetDescriptors;
		private Dictionary<string, object> _customData;

		protected SearchQuery(string[] fields, string term, bool escape = false, bool isExactMatch = false, bool isFuzzySearch = false)
		{
			Fields = fields;
			Term = term;
			EscapeTerm = escape;
			IsExactMatch = isExactMatch;
			IsFuzzySearch = isFuzzySearch;

			Filters = new List<ISearchFilter>();
			Sorting = new List<SearchSort>();
			_facetDescriptors = new Dictionary<string, FacetDescriptor>(StringComparer.OrdinalIgnoreCase);

			Take = int.MaxValue;

			SpellCheckerMinQueryLength = 4;
		}

		// Language & Store
		public int? LanguageId { get; protected set; }
		public string LanguageSeoCode { get; protected set; }
		public int? StoreId { get; protected set; }

		// Search term
		public string[] Fields { get; set; }
		public string Term { get; set; }
		public bool EscapeTerm { get; protected set; }
		public bool IsExactMatch { get; protected set; }
		public bool IsFuzzySearch { get; protected set; }

		// Filtering
		public ICollection<ISearchFilter> Filters { get; }

		// Facets
		public IReadOnlyDictionary<string, FacetDescriptor> FacetDescriptors
		{
			get { return _facetDescriptors; }
		}

		// Paging
		public int Skip { get; protected set; }
		public int Take { get; protected set; }
		public int PageIndex
		{
			get
			{
				if (Take == 0)
					return 0;

				return Math.Max(Skip / Take, 0);
			}
		}

		// Sorting
		public ICollection<SearchSort> Sorting { get; }

		// Spell checker
		public int SpellCheckerMaxSuggestions { get; protected set; }
		public int SpellCheckerMinQueryLength { get; protected set; }

		// Misc
		public string Origin { get; protected set; }

		public IDictionary<string, object> CustomData
		{
			get
			{
				return _customData ?? (_customData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
			}
		}

		#region Fluent builder

		public virtual TQuery HasStoreId(int id)
		{
			Guard.IsPositive(id, nameof(id));

			StoreId = id;

			return (this as TQuery);
		}

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
			Guard.NotNegative(take, nameof(take));

			Skip = skip;
			Take = take;

			return (this as TQuery);
		}

		public TQuery CheckSpelling(int maxSuggestions, int minQueryLength = 4)
		{
			Guard.IsPositive(maxSuggestions, nameof(maxSuggestions));

			SpellCheckerMaxSuggestions = maxSuggestions;
			SpellCheckerMinQueryLength = minQueryLength;

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

		public TQuery WithFacet(FacetDescriptor facetDescription)
		{
			Guard.NotNull(facetDescription, nameof(facetDescription));

			if (_facetDescriptors.ContainsKey(facetDescription.Key))
			{
				throw new InvalidOperationException("A facet description object with the same key has already been added. Key: {0}".FormatInvariant(facetDescription.Key));
			}

			_facetDescriptors.Add(facetDescription.Key, facetDescription);

			return (this as TQuery);
		}

		public TQuery OriginatesFrom(string origin)
		{
			Guard.NotEmpty(origin, nameof(origin));

			Origin = origin;

			return (this as TQuery);
		}

		#endregion

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (Term.HasValue())
			{
				var fields = (Fields != null && Fields.Length > 0 ? string.Join(", ", Fields) : "".NaIfEmpty());

				sb.AppendFormat("'{0}' in {1}", Term, fields);

				var parameters = string.Join(" ", EscapeTerm ? "escape" : "", IsFuzzySearch ? "fuzzy" : (IsExactMatch ? "exact" : "")).TrimSafe();

				if (parameters.HasValue())
				{
					sb.AppendFormat(" ({0})", parameters);
				}
			}

			foreach (var filter in Filters)
			{
				if (sb.Length > 0)
				{
					sb.Append(" ");
				}

				sb.Append(filter.ToString());
			}

			return sb.ToString();
		}
	}
}
