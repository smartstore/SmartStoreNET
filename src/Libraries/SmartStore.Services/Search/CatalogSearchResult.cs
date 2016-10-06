using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchResult
	{
		public CatalogSearchResult(
			IPagedList<Product> hits,
			CatalogSearchQuery query,
			string[] suggestions)
		{
			Guard.NotNull(hits, nameof(hits));
			Guard.NotNull(query, nameof(query));

			Hits = hits;
			Query = query;
			Suggestions = suggestions ?? new string[0];
		}

		/// <summary>
		/// Products found
		/// </summary>
		public IPagedList<Product> Hits { get; private set; }

		/// <summary>
		/// The original catalog search query
		/// </summary>
		public CatalogSearchQuery Query { get; private set; }

		/// <summary>
		/// Gets the word suggestions.
		/// </summary>
		public string[] Suggestions { get; private set; }
	}
}
