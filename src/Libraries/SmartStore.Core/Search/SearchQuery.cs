using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search.Facets;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Core.Search
{
    public enum SearchResultFlags
    {
        WithHits = 1 << 0,
        WithFacets = 1 << 1,
        WithSuggestions = 1 << 2,
        Full = WithHits | WithFacets | WithSuggestions
    }

    public class SearchQuery : SearchQuery<SearchQuery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchQuery"/> class without a search term being set
        /// </summary>
        public SearchQuery()
            : base((string[])null, null)
        {
        }

        public SearchQuery(string field, string term, SearchMode mode = SearchMode.Contains, bool escape = false, bool isFuzzySearch = false)
            : base(field.HasValue() ? new[] { field } : null, term, mode, escape, isFuzzySearch)
        {
        }

        public SearchQuery(string[] fields, string term, SearchMode mode = SearchMode.Contains, bool escape = false, bool isFuzzySearch = false)
            : base(fields, term, mode, escape, isFuzzySearch)
        {
        }
    }

    public class SearchQuery<TQuery> : ISearchQuery where TQuery : class, ISearchQuery
    {
        private readonly Dictionary<string, FacetDescriptor> _facetDescriptors;
        private Dictionary<string, object> _customData;

        protected SearchQuery(string[] fields, string term, SearchMode mode = SearchMode.Contains, bool escape = false, bool isFuzzySearch = false)
        {
            Fields = fields;
            Term = term;
            Mode = mode;
            EscapeTerm = escape;
            IsFuzzySearch = isFuzzySearch;

            Filters = new List<ISearchFilter>();
            Sorting = new List<SearchSort>();
            _facetDescriptors = new Dictionary<string, FacetDescriptor>(StringComparer.OrdinalIgnoreCase);

            Take = int.MaxValue;

            SpellCheckerMinQueryLength = 4;
            SpellCheckerMaxHitCount = 3;

            ResultFlags = SearchResultFlags.WithHits;
        }

        // Language, Currency & Store
        public int? LanguageId { get; protected set; }
        public string LanguageCulture { get; protected set; }
        public string CurrencyCode { get; protected set; }
        public int? StoreId { get; protected set; }

        // Search term
        public string[] Fields { get; set; }
        public string Term { get; set; }
        public bool EscapeTerm { get; protected set; }
        public SearchMode Mode { get; protected set; }
        public bool IsFuzzySearch { get; protected set; }

        // Filtering
        public ICollection<ISearchFilter> Filters { get; }

        // Facets
        public IReadOnlyDictionary<string, FacetDescriptor> FacetDescriptors => _facetDescriptors;

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
        public int SpellCheckerMaxHitCount { get; protected set; }

        // Result control
        public SearchResultFlags ResultFlags { get; protected set; }

        // Misc
        public string Origin { get; protected set; }

        public IDictionary<string, object> CustomData => _customData ?? (_customData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

        #region Fluent builder

        public virtual TQuery HasStoreId(int id)
        {
            Guard.NotNegative(id, nameof(id));

            StoreId = id;

            return (this as TQuery);
        }

        public TQuery WithLanguage(Language language)
        {
            Guard.NotNull(language, nameof(language));
            Guard.NotEmpty(language.LanguageCulture, nameof(language.LanguageCulture));

            LanguageId = language.Id;
            LanguageCulture = language.LanguageCulture;

            return (this as TQuery);
        }

        public TQuery WithCurrency(Currency currency)
        {
            Guard.NotNull(currency, nameof(currency));
            Guard.NotEmpty(currency.CurrencyCode, nameof(currency.CurrencyCode));

            CurrencyCode = currency.CurrencyCode;

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

        public TQuery CheckSpelling(int maxSuggestions, int minQueryLength = 4, int maxHitCount = 3)
        {
            Guard.IsPositive(minQueryLength, nameof(minQueryLength));
            Guard.IsPositive(maxHitCount, nameof(maxHitCount));

            if (maxSuggestions > 0)
            {
                ResultFlags = ResultFlags | SearchResultFlags.WithSuggestions;
            }
            else
            {
                ResultFlags &= ~SearchResultFlags.WithSuggestions;
            }

            SpellCheckerMaxSuggestions = Math.Max(maxSuggestions, 0);
            SpellCheckerMinQueryLength = minQueryLength;
            SpellCheckerMaxHitCount = maxHitCount;

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

        public TQuery BuildHits(bool build)
        {
            if (build)
            {
                ResultFlags = ResultFlags | SearchResultFlags.WithHits;
            }
            else
            {
                ResultFlags &= ~SearchResultFlags.WithHits;
            }


            return (this as TQuery);
        }

        public TQuery BuildFacetMap(bool build)
        {
            if (build)
            {
                ResultFlags = ResultFlags | SearchResultFlags.WithFacets;
            }
            else
            {
                ResultFlags &= ~SearchResultFlags.WithFacets;
            }


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
            var psb = PooledStringBuilder.Rent();
            var sb = (StringBuilder)psb;

            if (Term.HasValue())
            {
                var fields = (Fields != null && Fields.Length > 0 ? string.Join(", ", Fields) : "".NaIfEmpty());

                sb.AppendFormat("'{0}' in {1}", Term, fields);

                var parameters = string.Join(" ", EscapeTerm ? "escape" : "", IsFuzzySearch ? "fuzzy" : Mode.ToString()).TrimSafe();

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

            return psb.ToStringAndReturn();
        }

        #region Utilities

        protected TQuery CreateFilter(string fieldName, params int[] values)
        {
            var len = values?.Length ?? 0;
            if (len > 0)
            {
                if (len == 1)
                {
                    return WithFilter(SearchFilter.ByField(fieldName, values[0]).Mandatory().ExactMatch().NotAnalyzed());
                }

                return WithFilter(SearchFilter.Combined(values.Select(x => SearchFilter.ByField(fieldName, x).ExactMatch().NotAnalyzed()).ToArray()));
            }

            return this as TQuery;
        }

        #endregion
    }
}
