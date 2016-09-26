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
		private readonly IProductService _productService;

		public CatalogSearchService(
			IIndexManager indexManager,
			IProductService productService)
		{
			_indexManager = indexManager;
			_productService = productService;
		}

		public IEnumerable<Product> Search(SearchQuery query)
		{
			if (_indexManager.HasAnyProvider())
			{
				var provider = _indexManager.GetIndexProvider();

				var indexStore = provider.GetIndexStore("Catalog");
				if (indexStore.Exists)
				{
					var searchEngine = provider.GetSearchEngine(indexStore, query);
					var searchHits = searchEngine.Search();

					var productIds = searchHits.Select(x => x.EntityId).ToArray();

					return _productService.GetProductsByIds(productIds);
				}
			}

			// TODO: fallback to linq based search

			return Enumerable.Empty<Product>();
		}
	}
}
