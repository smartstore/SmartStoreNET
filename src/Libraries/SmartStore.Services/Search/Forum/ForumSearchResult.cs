using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search
{
    public partial class ForumSearchResult
    {
        private readonly Func<IList<ForumTopic>> _hitsFactory;
		private IPagedList<ForumTopic> _hits;

		public ForumSearchResult(
			ISearchEngine engine,
            ForumSearchQuery query,
			int totalHitsCount,
			Func<IList<ForumTopic>> hitsFactory,
			string[] spellCheckerSuggestions,
            IDictionary<string, FacetGroup> facets)
		{
			Guard.NotNull(query, nameof(query));

			Engine = engine;
			Query = query;
			SpellCheckerSuggestions = spellCheckerSuggestions ?? new string[0];
            Facets = facets ?? new Dictionary<string, FacetGroup>();

            _hitsFactory = hitsFactory ?? (() => new List<ForumTopic>());
			TotalHitsCount = totalHitsCount;
		}

        /// <summary>
        /// Constructor for an instance without any search hits
        /// </summary>
        /// <param name="query">Forum search query</param>
        public ForumSearchResult(ForumSearchQuery query)
			: this(null, query, 0, () => new List<ForumTopic>(), null, null)
		{
		}

        /// <summary>
        /// Forum topics found
        /// </summary>
        public IPagedList<ForumTopic> Hits
		{
			get
			{
				if (_hits == null)
				{
					var entities = TotalHitsCount == 0 
						? new List<ForumTopic>() 
						: _hitsFactory.Invoke();

					_hits = new PagedList<ForumTopic>(entities, Query.PageIndex, Query.Take, TotalHitsCount);
				}

				return _hits;
			}
		}

        public int TotalHitsCount { get; }

        /// <summary>
        /// The original forum search query
        /// </summary>
        public ForumSearchQuery Query { get; private set; }

		/// <summary>
		/// Gets spell checking suggestions/corrections
		/// </summary>
		public string[] SpellCheckerSuggestions { get; set;	}

        public IDictionary<string, FacetGroup> Facets { get; private set; }

        public ISearchEngine Engine { get; private set;	}
    }
}
