using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		IPagedList<Product> Hits { get; }

		/// <summary>
		/// The original catalog search query
		/// </summary>
		CatalogSearchQuery Query { get; }

		/// <summary>
		/// Gets the word suggestions.
		/// </summary>
		string[] Suggestions { get; }

		// TODO: Facets etc.
	}
}
