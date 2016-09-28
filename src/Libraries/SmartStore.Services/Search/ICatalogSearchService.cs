namespace SmartStore.Services.Search
{
	public partial interface ICatalogSearchService
	{
		/// <summary>
		/// Searches for products
		/// </summary>
		/// <param name="searchQuery">Search term, filters and other parameters used for searching</param>
		/// <returns>Catalog search result</returns>
		CatalogSearchResult Search(CatalogSearchQuery searchQuery);
	}
}
