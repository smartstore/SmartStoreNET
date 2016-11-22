using System.Collections.Generic;
using SmartStore.Core.Search.Facets;

namespace SmartStore.Core.Search
{
	public interface ISearchEngine
	{
		/// <summary>
		/// Search query
		/// </summary>
		ISearchQuery Query { get; }

		/// <summary>
		/// Get search hit by entity identifier
		/// </summary>
		/// <param name="id">Entity identifier</param>
		/// <returns>Search hit</returns>
		ISearchHit Get(int id);

		/// <summary>
		/// Get search bits
		/// </summary>
		/// <returns>Search bits</returns>
		ISearchBits GetBits();

		/// <summary>
		/// Get total number of search hits
		/// </summary>
		/// <returns>Total number of search hits</returns>
		int Count();

		/// <summary>
		/// Search
		/// </summary>
		/// <returns>Search hits</returns>
		IEnumerable<ISearchHit> Search();

		/// <summary>
		/// Gets the facet map for drilldown navigation
		/// </summary>
		/// <returns>The facet groups</returns>
		IDictionary<string, FacetGroup> GetFacetMap();

		/// <summary>
		/// Get suggestions of similar words
		/// </summary>
		/// <param name="numberOfSuggestions">Maximum number of similar words to be returned</param>
		/// <returns>Suggestions of similar words</returns>
		string[] GetSuggestions(int numberOfSuggestions);

		/// <summary>
		/// Highlights chosen terms in a text, extracting the most relevant sections
		/// </summary>
		/// <param name="input">Text to highlight terms in</param>
		/// <param name="preMatch">Text/HTML to prepend to matched keyword</param>
		/// <param name="postMatch">Text/HTML to append to matched keyword</param>
		/// <returns>Highlighted text fragments</returns>
		string Highlight(string input, string preMatch, string postMatch);
	}
}
