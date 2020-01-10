using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PaidByRule :  IRule
    {
        private readonly IOrderService _orderService;

        public PaidByRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var paymentMethods = expression.Value as List<string>;
            if (!(paymentMethods?.Any() ?? false))
            {
                return true;
            }

            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null, null);

            if (expression.Operator == RuleOperator.In)
            {
                query = query.Where(o => paymentMethods.Contains(o.PaymentMethodSystemName));
            }
            else if (expression.Operator == RuleOperator.NotIn)
            {
                query = query.Where(o => !paymentMethods.Contains(o.PaymentMethodSystemName));
            }
            else
            {
                throw new InvalidRuleOperatorException(expression);
            }

            return query.Count() > 0;
        }
    }
}
