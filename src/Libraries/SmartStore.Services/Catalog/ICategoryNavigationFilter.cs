using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{  
    /// <summary>
    /// Implementors can use this interface to narrow down the category result set
    /// in the public store. Useful in scenarios where merchant specific requirements demand
    /// categories to be managed in backend, but be hidden in the frontend.
    /// </summary>
    public interface ICategoryNavigationFilter
    {
        /// <summary>
        /// Applies a store specific filter to the actual category query.
        /// </summary>
        /// <param name="query">The original query</param>
        /// <returns>The modified/extended query</returns>
        IQueryable<Category> Apply(IQueryable<Category> query);
    }
}
