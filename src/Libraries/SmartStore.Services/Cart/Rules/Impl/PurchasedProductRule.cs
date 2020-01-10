using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedProductRule : IRule
    {
        private readonly IOrderService _orderService;

        public PurchasedProductRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var productIds = expression.Value as List<int>;
            if (!(productIds?.Any() ?? false))
            {
                return true;
            }

            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null, null)
                .SelectMany(o => o.OrderItems);

            if (expression.Operator == RuleOperator.In)
            {
                query = query.Where(oi => productIds.Contains(oi.ProductId));
            }
            else if (expression.Operator == RuleOperator.NotIn)
            {
                query = query.Where(oi => !productIds.Contains(oi.ProductId));
            }
            else
            {
                throw new InvalidRuleOperatorException(expression);
            }

            return query.Count() > 0;
        }
    }
}
