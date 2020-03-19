using System;
using SmartStore.Rules;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog.Rules
{
    public abstract class SearchFilterDescriptor : RuleDescriptor
    {
        public SearchFilterDescriptor()
            : base(RuleScope.Product)
        {
        }

        public abstract CatalogSearchQuery ApplyFilter(CatalogSearchQuery query, object value);
    }

    public class SearchFilterDescriptor<TValue> : SearchFilterDescriptor
    {
        public SearchFilterDescriptor(Func<CatalogSearchQuery, TValue, CatalogSearchQuery> filter)
        {
            Guard.NotNull(filter, nameof(filter));

            Filter = filter;
        }

        public Func<CatalogSearchQuery, TValue, CatalogSearchQuery> Filter { get; protected set; }

        public override CatalogSearchQuery ApplyFilter(CatalogSearchQuery query, object value)
        {
            return Filter(query, value.Convert<TValue>());
        }
    }
}
