using System.Collections.Generic;
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
        /// <param name="linkExpression">
        /// Link expression, e.g. product:123, category:234, topic:2, topic:aboutus etc.
        /// Supported entities are product, category, manufacturer and topic. Value of topic can
        /// either be the topic id or system name.
        /// </param>
        /// <param name="roles">Customer roles to check access for. <c>null</c> to use current customer's roles.</param>
        /// <param name="languageId">Language identifier. 0 to use current working language.</param>
        /// <param name="storeId">Store identifier. 0 to use current store.</param>
        /// <returns>LinkResolverResult</returns>
        LinkResolverResult Resolve(string linkExpression, IEnumerable<CustomerRole> roles = null, int languageId = 0, int storeId = 0);
    }
}
