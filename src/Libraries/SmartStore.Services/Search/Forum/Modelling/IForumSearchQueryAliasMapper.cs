using SmartStore.Core.Search.Facets;

namespace SmartStore.Services.Search.Modelling
{
    public interface IForumSearchQueryAliasMapper
    {
        /// <summary>
        /// Clears all cached common facet mappings
        /// </summary>
        void ClearCommonFacetCache();

        /// <summary>
        /// Get the common facet alias by facet group kind
        /// </summary>
        /// <param name="kind">Facet group kind</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Common facet alias</returns>
        string GetCommonFacetAliasByGroupKind(FacetGroupKind kind, int languageId);
    }
}
