using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class SpentAmountRule : IRule
    {
        private readonly IOrderService _orderService;

        public SpentAmountRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var orderTotals = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, new int[] { (int)OrderStatus.Complete }, null, null, null, null, null)
                .Select(x => x.OrderTotal)
                .ToList();

            var spentAmount = orderTotals.Any() ? orderTotals.Sum() : decimal.Zero;

            var money = new Money(spentAmount, context.WorkContext.WorkingCurrency);
            spentAmount = money.RoundedAmount;

            return expression.Operator.Match(spentAmount, expression.Value);
        }
    }
}
