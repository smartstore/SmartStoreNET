using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Rules;

namespace SmartStore.Services.Catalog.Rules
{
    public interface IProductRuleProvider : IRuleProvider
    {
        SearchFilterExpressionGroup CreateExpressionGroup(int ruleSetId);

        IPagedList<Product> Search(SearchFilterExpression[] filters, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
