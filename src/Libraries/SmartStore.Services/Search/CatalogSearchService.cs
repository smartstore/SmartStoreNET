using System;
using System.Collections.Generic;
using System.Linq;
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

		public IEnumerable<Product> Search(CatalogSearchQuery searchQuery)
		{
			if (_indexManager.HasAnyProvider())
			{
				var provider = _indexManager.GetIndexProvider();

				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, searchQuery);
					var searchHits = searchEngine.Search();

					var productIds = searchHits.Select(x => x.EntityId).ToArray();

					return _productService.Value.GetProductsByIds(productIds);
				}
			}

			var products = _linqCatalogSearchService.Value.GetProducts(searchQuery).ToList();
			return products;
		}
	}
}
