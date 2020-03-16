using System;
using SmartStore.Core.Search;
using SmartStore.Rules;

namespace SmartStore.Services.Catalog.Rules
{
    public abstract class SearchFilterDescriptor : RuleDescriptor
    {
        public SearchFilterDescriptor()
            : base(RuleScope.Product)
        {
        }

        public abstract ISearchFilter GetFilter(object value);
    }

    public class SearchFilterDescriptor<TValue> : SearchFilterDescriptor
    {
        public SearchFilterDescriptor(Func<TValue, ISearchFilter> filter)
        {
            Guard.NotNull(filter, nameof(filter));

            Filter = filter;
        }

        public Func<TValue, ISearchFilter> Filter { get; protected set; }

        public override ISearchFilter GetFilter(object value)
        {
            return Filter(value.Convert<TValue>());
        }
    }
}
