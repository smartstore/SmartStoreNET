using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
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

		public CatalogSearchService(
			IComponentContext ctx,
			ILogger logger,
			IIndexManager indexManager,
			Lazy<IProductService> productService,
			IChronometer chronometer)
		{
			_ctx = ctx;
			_logger = logger;
			_indexManager = indexManager;
			_productService = productService;
			_chronometer = chronometer;
		}

		protected virtual CatalogSearchResult SearchFallback(CatalogSearchQuery searchQuery)
		{
			// fallback to linq search
			var linqCatalogSearchService = _ctx.ResolveNamed<ICatalogSearchService>("linq");
			return linqCatalogSearchService.Search(searchQuery);
		}

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery)
		{
			Guard.NotNull(searchQuery, nameof(searchQuery));
			Guard.NotNegative(searchQuery.Take, nameof(searchQuery.Take));

			if (_indexManager.HasAnyProvider())
			{
				var provider = _indexManager.GetIndexProvider();

				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);

					using (_chronometer.Step("Search (" + searchEngine.GetType().Name + ")"))
					{
						var totalCount = 0;
						string[] suggestions = null;
						IEnumerable<ISearchHit> searchHits;
						PagedList<Product> hits;

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
								var products = _productService.Value.GetProductsByIds(productIds);

								hits = new PagedList<Product>(products, searchQuery.PageIndex, searchQuery.Take, totalCount);
							}
						}
						else
						{
							hits = new PagedList<Product>(new List<Product>(), searchQuery.PageIndex, searchQuery.Take);
						}

						if (searchQuery.NumberOfSuggestions > 0)
						{
							try
							{
								using (_chronometer.Step("Get suggestions"))
								{
									suggestions = searchEngine.GetSuggestions(searchQuery.NumberOfSuggestions);
								}
							}
							catch (Exception exception)
							{
								// suggestions should not break the search
								_logger.Error(exception);
							}
						}

						return new CatalogSearchResult(hits, searchQuery, suggestions);
					}
				}
			}

			return SearchFallback(searchQuery);
		}
	}
}
