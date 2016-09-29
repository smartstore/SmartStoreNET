using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchService : ICatalogSearchService
	{
		private readonly IIndexManager _indexManager;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<ILinqCatalogSearchService> _linqCatalogSearchService;

		public CatalogSearchService(
			IIndexManager indexManager,
			Lazy<IProductService> productService,
			Lazy<ILinqCatalogSearchService> linqCatalogSearchService)
		{
			_indexManager = indexManager;
			_productService = productService;
			_linqCatalogSearchService = linqCatalogSearchService;
		}

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery)
		{
			Guard.IsPositive(searchQuery.Take, nameof(searchQuery.Take));

			var result = new CatalogSearchResult();
			result.Query = searchQuery;

			var pageIndex = Math.Max((searchQuery.Skip - 1) / searchQuery.Take, 0);

			if (_indexManager.HasAnyProvider())
			{
				var provider = _indexManager.GetIndexProvider();

				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);
					var totalCount = searchEngine.Count();
					var searchHits = searchEngine.Search();

					var productIds = searchHits.Select(x => x.EntityId).ToArray();
					var products = _productService.Value.GetProductsByIds(productIds);

					result.Hits = new PagedList<Product>(products, pageIndex, searchQuery.Take, totalCount);
				}
			}

			if (result.Hits == null)
			{
				// fallback to linq search
				var productQuery = _linqCatalogSearchService.Value.GetProducts(searchQuery);

				result.Hits = new PagedList<Product>(productQuery, pageIndex, searchQuery.Take);
			}

			return result;
		}
	}
}
