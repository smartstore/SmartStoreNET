using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial interface ICatalogSearchService
	{
		IEnumerable<Product> Search(CatalogSearchQuery searchQuery);
	}
}
