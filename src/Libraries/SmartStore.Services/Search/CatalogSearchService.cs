using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchService : ICatalogSearchService
	{
		private readonly IComponentContext _ctx;
		private readonly ILogger _logger;
		private readonly IIndexManager _indexManager;
		private readonly Lazy<IProductService> _productService;
		private readonly IChronometer _chronometer;
		private readonly IEventPublisher _eventPublisher;
		private readonly IPriceFormatter _priceFormatter;

		public CatalogSearchService(
			IComponentContext ctx,
			ILogger logger,
			IIndexManager indexManager,
			Lazy<IProductService> productService,
			IChronometer chronometer,
			IEventPublisher eventPublisher,
			IPriceFormatter priceFormatter)
		{
			_ctx = ctx;
			_logger = logger;
			_indexManager = indexManager;
			_productService = productService;
			_chronometer = chronometer;
			_eventPublisher = eventPublisher;
			_priceFormatter = priceFormatter;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		/// <summary>
		/// Bypasses the index provider and directly searches in the database
		/// </summary>
		/// <param name="searchQuery"></param>
		/// <param name="loadFlags"></param>
		/// <returns></returns>
		protected virtual CatalogSearchResult SearchDirect(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None)
		{
			// fallback to linq search
			var linqCatalogSearchService = _ctx.ResolveNamed<ICatalogSearchService>("linq");

			var result = linqCatalogSearchService.Search(searchQuery, loadFlags, true);
			ApplyFacetLabels(result.Facets);

			return result;
		}

		public CatalogSearchResult Search(
			CatalogSearchQuery searchQuery, 
			ProductLoadFlags loadFlags = ProductLoadFlags.None, 
			bool direct = false)
		{
			Guard.NotNull(searchQuery, nameof(searchQuery));
			Guard.NotNegative(searchQuery.Take, nameof(searchQuery.Take));

			var provider = _indexManager.GetIndexProvider();

			if (!direct && provider != null)
			{
				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);

					using (_chronometer.Step("Search (" + searchEngine.GetType().Name + ")"))
					{
						int totalCount = 0;
						string[] spellCheckerSuggestions = null;
						IEnumerable<ISearchHit> searchHits;
						Func<IList<Product>> hitsFactory = null;
						IDictionary<string, FacetGroup> facets = null;

						_eventPublisher.Publish(new CatalogSearchingEvent(searchQuery));

						if (searchQuery.Take > 0)
						{
							totalCount = searchEngine.Count();

							using (_chronometer.Step("Get hits"))
							{
								searchHits = searchEngine.Search();
							}

							if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithHits))
							{
								using (_chronometer.Step("Collect from DB"))
								{
									var productIds = searchHits.Select(x => x.EntityId).ToArray();
									hitsFactory = () => _productService.Value.GetProductsByIds(productIds, loadFlags);
								}
							}

							if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithFacets))
							{
								try
								{
									using (_chronometer.Step("Get facets"))
									{
										facets = searchEngine.GetFacetMap();
										ApplyFacetLabels(facets);
									}
								}
								catch (Exception exception)
								{
									_logger.Error(exception);
								}
							}
						}

						if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithSuggestions))
						{
							try
							{
								using (_chronometer.Step("Spell checking"))
								{
									spellCheckerSuggestions = searchEngine.CheckSpelling();
								}
							}
							catch (Exception exception)
							{
								// spell checking should not break the search
								_logger.Error(exception);
							}
						}

						var result = new CatalogSearchResult(
							searchEngine,
							searchQuery,
							totalCount,
							hitsFactory,
							spellCheckerSuggestions,
							facets);

						_eventPublisher.Publish(new CatalogSearchedEvent(searchQuery, result));

						return result;
					}
				}
			}

			return SearchDirect(searchQuery);
		}

		public IQueryable<Product> PrepareQuery(CatalogSearchQuery searchQuery, IQueryable<Product> baseQuery = null)
		{
			var linqCatalogSearchService = _ctx.ResolveNamed<ICatalogSearchService>("linq");
			return linqCatalogSearchService.PrepareQuery(searchQuery, baseQuery);
		}

		protected virtual void ApplyFacetLabels(IDictionary<string, FacetGroup> facets)
		{
			if (facets == null | facets.Count == 0)
			{
				return;
			}

			// TODO: (mc) > (mg) Apply labels to all other range attributes

			// Apply "price" labels
			FacetGroup group;
			string labelTemplate;

			if (facets.TryGetValue("price", out group))
			{
				// Format prices for price facet labels
				// TODO: formatting without decimals would be nice
				labelTemplate = T("Search.Facet.PriceMax").Text;

				foreach (var facet in group.Facets)
				{
					var val = facet.Value;

					if (val.Value == null && val.UpperValue != null)
					{
						facet.Value.Label = T("Search.Facet.PriceMax", FormatPrice(val.UpperValue.Convert<decimal>()));
					}
					else if (val.Value != null && val.UpperValue == null)
					{
						facet.Value.Label = T("Search.Facet.PriceMin", FormatPrice(val.Value.Convert<decimal>()));
					}
					else if (val.Value != null && val.UpperValue != null)
					{
						facet.Value.Label = T("Search.Facet.PriceBetween", 
							FormatPrice(val.Value.Convert<decimal>()),
							FormatPrice(val.UpperValue.Convert<decimal>()));
					}
				}
			}
			
			if (facets.TryGetValue("rate", out group))
			{
				foreach (var facet in group.Facets)
				{
					facet.Value.Label = T(facet.Key == "1" ? "Search.Facet.1StarAndMore" : "Search.Facet.XStarsAndMore", facet.Value.Value).Text;
				}
			}
		}

		private string FormatPrice(decimal price)
		{
			return _priceFormatter.FormatPrice(price, true, false);
		}
	}
}
