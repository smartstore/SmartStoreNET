using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		IPagedList<Product> Hits { get; }

		// TODO: Facets etc.
	}
}
