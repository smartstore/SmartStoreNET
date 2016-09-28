using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		/// <summary>
		/// Products found
		/// </summary>
		public IPagedList<Product> Hits { get; set; }

		/// <summary>
		/// The original catalog search query
		/// </summary>
		public CatalogSearchQuery Query { get; set; }

		/// <summary>
		/// Gets the word suggestions.
		/// </summary>
		public string[] Suggestions { get; set; }

		// TODO: Facets etc.
	}
}
