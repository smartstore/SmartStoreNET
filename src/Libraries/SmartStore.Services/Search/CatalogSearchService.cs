using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchService : ICatalogSearchService
	{
		private readonly IIndexManager _indexManager;
		private readonly IProductService _productService;
		private readonly IRepository<Product> _productRepository;

		public CatalogSearchService(
			IIndexManager indexManager,
			IProductService productService,
			IRepository<Product> productRepository)
		{
			_indexManager = indexManager;
			_productService = productService;
			_productRepository = productRepository;
		}

		protected virtual IQueryable<Product> GetProductQuery(CatalogSearchQuery searchQuery)
		{
			// TODO: (mg) put LINQ searcher in separate class
			// TODO: (mg) don't use DynamicLinq: most field names in ISearchQuery has no matching db counterparts 

			var whereClause = new StringBuilder();
			var whereValues = new List<object>();
			var query = _productRepository.Table.Where(x => !x.Deleted && x.Published);

			// where clause: search term
			if (searchQuery.Term.HasValue())
			{
				if (searchQuery.Fields != null && searchQuery.Fields.Length != 0 && searchQuery.Fields.Any(x => x.HasValue()))
				{
					foreach (var field in searchQuery.Fields)
					{
						if (whereClause.Length > 0)
							whereClause.Append(" Or ");

						whereClause.AppendFormat("{0}.Contains(@0)", field);
					}

					whereValues.Add(searchQuery.Term);
				}
			}

			// TODO... where clause: filters

			if (whereClause.Length > 0)
			{
				query = query.Where(whereClause.ToString(), whereValues.ToArray());
			}

			// ordering
			var ordering = string.Join(", ", searchQuery.Sorting.Select(x => string.Concat(x.FieldName, x.Descending ? " Desc" : " Asc")));
			if (ordering.HasValue())
			{
				query = query.OrderBy(ordering);
			}
			else
			{
				query = query.OrderBy(x => x.Id);
			}

			// paging
			if (searchQuery.Skip > 0)
			{
				query = query.Skip(searchQuery.Skip);
			}

			if (searchQuery.Take != int.MaxValue)
			{
				query = query.Take(searchQuery.Take);
			}

			return query;
		}

		public IEnumerable<Product> Search(CatalogSearchQuery query)
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

			var products = GetProductQuery(query).ToList();
			return products;
		}
	}
}
