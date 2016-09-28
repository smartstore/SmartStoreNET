using System.Linq;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial interface ILinqCatalogSearchService
	{
		/// <summary>
		/// Searches for products
		/// </summary>
		/// <param name="searchQuery">Search term, filters and other parameters used for searching</param>
		/// <returns>Queryable of products</returns>
		IQueryable<Product> GetProducts(CatalogSearchQuery searchQuery);
	}
}
