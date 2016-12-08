using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		private readonly int _totalHitsCount;
		private readonly Func<IList<Product>> _hitsFactory;
		private IPagedList<Product> _hits;

		public CatalogSearchResult(
			ISearchEngine engine,
			int totalHitsCount,
			Func<IList<Product>> hitsFactory,
			CatalogSearchQuery query,
			string[] spellCheckerSuggestions)
		{
			Guard.NotNull(query, nameof(query));

			Engine = engine;
			Query = query;
			SpellCheckerSuggestions = spellCheckerSuggestions ?? new string[0];

			_hitsFactory = hitsFactory ?? (() => new List<Product>());
			_totalHitsCount = totalHitsCount;
		}

		/// <summary>
		/// Products found
		/// </summary>
		public IPagedList<Product> Hits
		{
			get
			{
				if (_hits == null)
				{
					var products = _totalHitsCount == 0 
						? new List<Product>() 
						: _hitsFactory.Invoke();

					_hits = new PagedList<Product>(products, Query.PageIndex, Query.Take, _totalHitsCount);
				}

				return _hits;
			}
		}

		public int TotalHitsCount
		{
			get { return _totalHitsCount; }
		}

		/// <summary>
		/// The original catalog search query
		/// </summary>
		public CatalogSearchQuery Query
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets spell checking suggestions/corrections
		/// </summary>
		public string[] SpellCheckerSuggestions
		{
			get;
			set;
		}

		public ISearchEngine Engine
		{
			get;
			private set;
		}

		/// <summary>
		/// Highlights chosen terms in a text, extracting the most relevant sections
		/// </summary>
		/// <param name="input">Text to highlight terms in</param>
		/// <returns>Highlighted text fragments </returns>
		public string Highlight(string input, string preMatch = "<strong>", string postMatch = "</strong>")
		{
			if (Query?.Term == null || input.IsEmpty())
				return input;

			string hilite = null;

			if (Engine != null)
			{
				try
				{
					hilite = Engine.Highlight(input, preMatch, postMatch);
				}
				catch { }
			}

			if (hilite.HasValue())
			{
				return hilite;
			}

			return input.HighlightKeywords(Query.Term, preMatch, postMatch);
		}
	}
}
