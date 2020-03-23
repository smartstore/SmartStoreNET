using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog.Rules
{
    public class SearchFilterExpression : RuleExpression
    {
        public new SearchFilterDescriptor Descriptor { get; set; }
    }


    public class SearchFilterExpressionGroup : SearchFilterExpression, IRuleExpressionGroup
    {
        private readonly List<IRuleExpression> _expressions = new List<IRuleExpression>();

        public SearchFilterExpressionGroup()
        {
            LogicalOperator = LogicalRuleOperator.And;
            Operator = RuleOperator.IsEqualTo;
        }

        public int RefRuleId { get; set; }
        public LogicalRuleOperator LogicalOperator { get; set; }
        public bool IsSubGroup { get; set; }
        public IRuleProvider Provider { get; set; }

        public IEnumerable<IRuleExpression> Expressions => _expressions;

        public void AddExpressions(params IRuleExpression[] expressions)
        {
            Guard.NotNull(expressions, nameof(expressions));

            _expressions.AddRange(expressions.OfType<SearchFilterExpression>());
        }

        public CatalogSearchQuery ApplyFilters(CatalogSearchQuery query)
        {
            // HOWTO: LogicalRuleOperator.Or? LinqCatalogSearchService doesn't support it. Really ICombinedSearchFilter of all filters for MegaSearch (weird)?
            var ctx = new SearchFilterContext { Query = query };

            foreach (var expression in Expressions.Cast<SearchFilterExpression>())
            {
                ctx.Expression = expression;
                ctx.Query = expression.Descriptor.ApplyFilter(ctx);
            }

            return ctx.Query;
        }
    }
}
