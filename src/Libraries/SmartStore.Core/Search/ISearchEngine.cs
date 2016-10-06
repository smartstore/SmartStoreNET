using System.Collections.Generic;

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
		/// Get suggestions of similar words
		/// </summary>
		/// <param name="numberOfSuggestions">Maximum number of similar words to be returned</param>
		/// <returns>Suggestions of similar words</returns>
		string[] GetSuggestions(int numberOfSuggestions);
	}
}
