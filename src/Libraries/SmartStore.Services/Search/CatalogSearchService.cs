using System;
using System.Linq;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchService : ICatalogSearchService
	{
		private const int NUMBER_OF_SUGGESTIONS = 2;

		private readonly IComponentContext _ctx;
		private readonly IIndexManager _indexManager;
		private readonly Lazy<IProductService> _productService;

		public CatalogSearchService(
			IComponentContext ctx,
			IIndexManager indexManager,
			Lazy<IProductService> productService)
		{
			_ctx = ctx;
			_indexManager = indexManager;
			_productService = productService;
		}

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery)
		{
			Guard.NotNull(searchQuery, nameof(searchQuery));
			Guard.IsPositive(searchQuery.Take, nameof(searchQuery.Take));

			if (_indexManager.HasAnyProvider())
			{
				var provider = _indexManager.GetIndexProvider();

				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);

					var totalCount = 0;
					var searchHits = searchEngine.Search(out totalCount);

					var productIds = searchHits.Select(x => x.EntityId).ToArray();
					var products = _productService.Value.GetProductsByIds(productIds);

					var hits = new PagedList<Product>(products, searchQuery.PageIndex, searchQuery.Take, totalCount);
					var suggestions = searchEngine.GetSuggestions(NUMBER_OF_SUGGESTIONS);

					return new CatalogSearchResult(hits, searchQuery, suggestions);
				}
			}

			// fallback to linq search
			var linqCatalogSearchService = _ctx.ResolveNamed<ICatalogSearchService>("linq");
			return linqCatalogSearchService.Search(searchQuery);
		}
	}
}
