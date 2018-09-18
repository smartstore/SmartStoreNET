using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Search
{
    public abstract partial class SearchServiceBase
    {
        protected virtual void FlattenFilters(ICollection<ISearchFilter> filters, List<ISearchFilter> result)
        {
            foreach (var filter in filters)
            {
                var combinedFilter = filter as ICombinedSearchFilter;
                if (combinedFilter != null)
                {
                    FlattenFilters(combinedFilter.Filters, result);
                }
                else
                {
                    result.Add(filter);
                }
            }
        }

        protected virtual ISearchFilter FindFilter(ICollection<ISearchFilter> filters, string fieldName)
        {
            if (fieldName.HasValue())
            {
                foreach (var filter in filters)
                {
                    var attributeFilter = filter as IAttributeSearchFilter;
                    if (attributeFilter != null && attributeFilter.FieldName == fieldName)
                    {
                        return attributeFilter;
                    }

                    var combinedFilter = filter as ICombinedSearchFilter;
                    if (combinedFilter != null)
                    {
                        var filter2 = FindFilter(combinedFilter.Filters, fieldName);
                        if (filter2 != null)
                        {
                            return filter2;
                        }
                    }
                }
            }

            return null;
        }

        protected virtual List<int> GetIdList(List<ISearchFilter> filters, string fieldName)
        {
            var result = new List<int>();

            foreach (IAttributeSearchFilter filter in filters)
            {
                if (!(filter is IRangeSearchFilter) && filter.FieldName == fieldName)
                {
                    result.Add((int)filter.Term);
                }
            }

            return result;
        }

        protected virtual IOrderedQueryable<TEntity> OrderBy<TEntity, TKey>(
            ref bool ordered,
            IQueryable<TEntity> query,
            Expression<Func<TEntity, TKey>> keySelector,
            bool descending = false)
        {
            if (ordered)
            {
                if (descending)
                {
                    return ((IOrderedQueryable<TEntity>)query).ThenByDescending(keySelector);
                }

                return ((IOrderedQueryable<TEntity>)query).ThenBy(keySelector);
            }
            else
            {
                ordered = true;

                if (descending)
                {
                    return query.OrderByDescending(keySelector);
                }

                return query.OrderBy(keySelector);
            }
        }

        /// <summary>
        /// Notifies the admin that indexing is required to use the advanced search.
        /// </summary>
        protected virtual void IndexingRequiredNotification(ICommonServices services, UrlHelper urlHelper)
        {
            if (services.WorkContext.CurrentCustomer.IsAdmin())
            {
                var indexingUrl = urlHelper.Action("Indexing", "MegaSearch", new { area = "SmartStore.MegaSearch" });
                var configureUrl = urlHelper.Action("ConfigurePlugin", "Plugin", new { area = "admin", systemName = "SmartStore.MegaSearch" });
                var notification = services.Localization.GetResource("Search.IndexingRequiredNotification").FormatInvariant(indexingUrl, configureUrl);

                services.Notifier.Information(notification);
            }
        }
    }
}
