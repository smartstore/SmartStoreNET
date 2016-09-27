using System.Linq;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial interface ILinqCatalogSearchService
	{
		IQueryable<Product> GetProducts(CatalogSearchQuery searchQuery);
	}
}
