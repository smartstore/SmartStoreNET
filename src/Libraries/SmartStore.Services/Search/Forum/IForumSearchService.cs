using System.Linq;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Services.Search
{
    public partial interface IForumSearchService
    {
        /// <summary>
        /// Searches for forum posts.
        /// </summary>
        /// <param name="searchQuery">Search term, filters and other parameters used for searching.</param>
        /// <param name="direct">Bypasses the index provider (if available) and directly searches in the database.</param>
        /// <returns>Forum search result.</returns>
        ForumSearchResult Search(ForumSearchQuery searchQuery, bool direct = false);

        /// <summary>
        /// Builds a forum post query using linq search.
        /// </summary>
        /// <param name="searchQuery">Search term, filters and other parameters used for searching.</param>
        /// <param name="baseQuery">Optional query used to build the forum post query.</param>
        /// <returns>Forum post queryable.</returns>
        IQueryable<ForumPost> PrepareQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery = null);
    }
}
