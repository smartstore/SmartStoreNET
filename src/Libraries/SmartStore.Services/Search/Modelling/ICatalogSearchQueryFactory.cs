using System.Web;

namespace SmartStore.Services.Search.Modelling
{
	public interface ICatalogSearchQueryFactory
	{
		/// <summary>
		/// Creates a <see cref="CatalogSearchQuery"/> instance from the current <see cref="HttpContextBase"/> 
		/// by looking up corresponding keys in posted form and/or query string
		/// </summary>
		/// <returns>The query object</returns>
		CatalogSearchQuery CreateFromQuery();

		/// <summary>
		/// Deserializes the passed query object to a query string
		/// </summary>
		/// <param name="query">The query</param>
		/// <returns>Query string (e.g. '?q=term&o=1&i=1&s=100...')</returns>
		string ToQueryString(CatalogSearchQuery query);

		/// <summary>
		/// The last created query instance. The MVC model binder uses this property to avoid repeated binding.
		/// </summary>
		CatalogSearchQuery Current { get; }
	}
}
