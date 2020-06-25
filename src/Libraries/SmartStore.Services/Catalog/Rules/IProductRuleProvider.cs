using SmartStore.Rules;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog.Rules
{
    public interface IProductRuleProvider : IRuleProvider
    {
        SearchFilterExpressionGroup CreateExpressionGroup(int ruleSetId);

        CatalogSearchResult Search(SearchFilterExpression[] filters, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
