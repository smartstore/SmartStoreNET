namespace SmartStore.Services.Cms
{
    /// <summary>
    /// Provides methods to resolve link expressions.
    /// </summary>
    public partial interface ILinkResolver
    {
        /// <summary>
        /// Gets a display name for a link expression.
        /// </summary>
        /// <param name="linkExpression">Link expression.</param>
        /// <param name="languageId">Language identifier. 0 to use current working language.</param>
        /// <returns>Tokenize result. Never returns <c>null</c>.</returns>
        TokenizeResult GetDisplayName(string linkExpression, int languageId = 0);

        /// <summary>
        /// Gets the link for a link expression.
        /// </summary>
        /// <param name="linkExpression">Link expression.</param>
        /// <param name="languageId">Language identifier. 0 to use current working language.</param>/// 
        /// <returns>Tokenize result. Never returns <c>null</c>.</returns>
        TokenizeResult GetLink(string linkExpression, int languageId = 0);
    }
}
