using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
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

		public CatalogSearchService(
			IComponentContext ctx,
			ILogger logger,
			IIndexManager indexManager,
			Lazy<IProductService> productService,
			IChronometer chronometer,
			IEventPublisher eventPublisher)
		{
			_ctx = ctx;
			_logger = logger;
			_indexManager = indexManager;
			_productService = productService;
			_chronometer = chronometer;
			_eventPublisher = eventPublisher;
		}

		protected virtual CatalogSearchResult SearchFallback(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None)
		{
			// fallback to linq search
			var linqCatalogSearchService = _ctx.ResolveNamed<ICatalogSearchService>("linq");
			return linqCatalogSearchService.Search(searchQuery, loadFlags);
		}

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None)
		{
			Guard.NotNull(searchQuery, nameof(searchQuery));
			Guard.NotNegative(searchQuery.Take, nameof(searchQuery.Take));

			var provider = _indexManager.GetIndexProvider();

			if (provider != null)
			{
				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);

					using (_chronometer.Step("Search (" + searchEngine.GetType().Name + ")"))
					{
						var totalCount = 0;
						string[] spellCheckerSuggestions = null;
						IEnumerable<ISearchHit> searchHits;
						PagedList<Product> hits;

						_eventPublisher.Publish(new CatalogSearchingEvent(searchQuery));

						if (searchQuery.Take > 0)
						{
							using (_chronometer.Step("Get total count"))
							{
								totalCount = searchEngine.Count();
							}

							using (_chronometer.Step("Get hits"))
							{
								searchHits = searchEngine.Search();
							}

							using (_chronometer.Step("Collect from DB"))
							{
								var productIds = searchHits.Select(x => x.EntityId).ToArray();
								var products = _productService.Value.GetProductsByIds(productIds, loadFlags);

								hits = new PagedList<Product>(products, searchQuery.PageIndex, searchQuery.Take, totalCount);
							}
						}
						else
						{
							hits = new PagedList<Product>(new List<Product>(), searchQuery.PageIndex, searchQuery.Take);
						}
						
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

						var result = new CatalogSearchResult(
							searchEngine, 
							hits, 
							searchQuery, 
							spellCheckerSuggestions);

						_eventPublisher.Publish(new CatalogSearchedEvent(searchQuery, result));

						return result;
					}
				}
			}

			return SearchFallback(searchQuery);
		}
	}
}
