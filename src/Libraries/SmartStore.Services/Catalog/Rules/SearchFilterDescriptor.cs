using System;
using SmartStore.Rules;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog.Rules
{
    public class SearchFilterContext
    {
        public CatalogSearchQuery Query { get; set; }
        public SearchFilterExpression Expression { get; set; }
    }


    public abstract class SearchFilterDescriptor : RuleDescriptor
    {
        public SearchFilterDescriptor()
            : base(RuleScope.Product)
        {
        }

        public abstract CatalogSearchQuery ApplyFilter(SearchFilterContext ctx);
    }

    public class SearchFilterDescriptor<TValue> : SearchFilterDescriptor
    {
        public SearchFilterDescriptor(Func<SearchFilterContext, TValue, CatalogSearchQuery> filter)
        {
            Guard.NotNull(filter, nameof(filter));

            Filter = filter;
        }

        public Func<SearchFilterContext, TValue, CatalogSearchQuery> Filter { get; protected set; }

        public override CatalogSearchQuery ApplyFilter(SearchFilterContext ctx)
        {
            return Filter(ctx, ctx.Expression.Value.Convert<TValue>());
        }
    }
}
