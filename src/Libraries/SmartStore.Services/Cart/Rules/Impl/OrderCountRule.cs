using System.Linq;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class OrderCountRule : IRule
    {
        private readonly IOrderService _orderService;

        public OrderCountRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null);
            var orderCount = query.Count();

            return expression.Operator.Match(orderCount, expression.Value);
        }
    }
}
