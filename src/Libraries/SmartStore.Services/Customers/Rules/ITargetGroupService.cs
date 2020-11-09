using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Rules;
using SmartStore.Rules.Filters;

namespace SmartStore.Services.Customers
{
    public interface ITargetGroupService : IRuleProvider
    {
        FilterExpressionGroup CreateExpressionGroup(int ruleSetId);

        IPagedList<Customer> ProcessFilter(
            FilterExpression filter,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        IPagedList<Customer> ProcessFilter(
            int[] ruleSetIds,
            LogicalRuleOperator logicalOperator,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        IPagedList<Customer> ProcessFilter(
            FilterExpression[] filters,
            LogicalRuleOperator logicalOperator,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
    }
}
