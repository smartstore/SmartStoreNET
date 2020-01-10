using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedFromManufacturerRule : IRule
    {
        private readonly IOrderService _orderService;

        public PurchasedFromManufacturerRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var manuIds = expression.Value as List<int>;
            if (!(manuIds?.Any() ?? false))
            {
                return true;
            }

            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null, null)
                .SelectMany(o => o.OrderItems);

            if (expression.Operator == RuleOperator.In)
            {
                query = query.Where(oi => oi.Product.ProductManufacturers.Any(pm => manuIds.Contains(pm.ManufacturerId)));
            }
            else if (expression.Operator == RuleOperator.NotIn)
            {
                query = query.Where(oi => oi.Product.ProductManufacturers.Any(pm => !manuIds.Contains(pm.ManufacturerId)));
            }
            else
            {
                throw new InvalidRuleOperatorException(expression);
            }

            return query.Count() > 0;
        }
    }
}
