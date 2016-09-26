using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial interface ICatalogSearchService
	{
		IEnumerable<Product> Search(SearchQuery query);
	}
}
