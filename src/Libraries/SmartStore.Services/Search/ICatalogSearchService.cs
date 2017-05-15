using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial interface ICatalogSearchService
	{
		/// <summary>
		/// Searches for products
		/// </summary>
		/// <param name="searchQuery">Search term, filters and other parameters used for searching</param>
		/// <param name="loadFlags">Which product navigation properties to eager load</param>
		/// <param name="direct">Bypasses the index provider (if available) and directly searches in the database</param>
		/// <returns>Catalog search result</returns>
		CatalogSearchResult Search(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None, bool direct = false);

		/// <summary>
		/// Builds a product query using linq search
		/// </summary>
		/// <param name="searchQuery">Search term, filters and other parameters used for searching</param>
		/// <param name="baseQuery">Optional query used to build the product query.</param>
		/// <returns>Product queryable</returns>
		IQueryable<Product> PrepareQuery(CatalogSearchQuery searchQuery, IQueryable<Product> baseQuery = null);
	}
}
