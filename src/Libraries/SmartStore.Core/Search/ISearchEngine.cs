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
        /// Performs spell checking ("Diy you mean?")
        /// </summary>
        /// <returns>Suggestions/corrections or an empty array</returns>
        string[] CheckSpelling();

        /// <summary>
        /// Highlights chosen terms in a text, extracting the most relevant sections
        /// </summary>
        /// <param name="input">Text to highlight terms in</param>
        /// <param name="fieldName">Field name</param>
        /// <param name="preMatch">Text/HTML to prepend to matched keyword</param>
        /// <param name="postMatch">Text/HTML to append to matched keyword</param>
        /// <returns>Highlighted text fragments</returns>
        string Highlight(string input, string fieldName, string preMatch, string postMatch);

        /// <summary>
        /// Gets highlighted text fragments for an entity identifier.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <param name="fieldName">Field name.</param>
        /// <param name="preMatch">Text/HTML to prepend to matched keyword.</param>
        /// <param name="postMatch">Text/HTML to append to matched keyword.</param>
        /// <param name="maxNumFragments">Maximum number of returned text fragments.</param>
        /// <returns>Highlighted text fragments.</returns>
        string Highlight(int id, string fieldName, string preMatch, string postMatch, int maxNumFragments);
    }
}
