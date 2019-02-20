namespace SmartStore.Services.Cms
{
    /// <summary>
    /// Provides methods to resolve link expressions.
    /// </summary>
    public partial interface ILinkResolver
    {
        /// <summary>
        /// Resolves a link expression.
        /// </summary>
        /// <param name="linkExpression">Link expression.</param>
        /// <param name="languageId">Language identifier. 0 to use current working language.</param>
        /// <returns>LinkResolverResult</returns>
        LinkResolverResult Resolve(string linkExpression, int languageId = 0);
    }
}
