using SmartStore.Core.Domain.Customers;

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
        /// <param name="customer">Customer whose access is to be checked. <c>null</c> to use current customer.</param>
        /// <param name="languageId">Language identifier. 0 to use current working language.</param>
        /// <param name="storeId">Store identifier. 0 to use current store.</param>
        /// <returns>LinkResolverResult</returns>
        LinkResolverResult Resolve(string linkExpression, Customer customer = null, int languageId = 0, int storeId = 0);
    }
}
