using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedProductsRule : IRule
    {
        private readonly IRepository<OrderItem> _orderItemRepository;

        public PurchasedProductsRule(IRepository<OrderItem> orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var productIds = expression.Value as List<int>;
            if (!(productIds?.Any() ?? false))
            {
                return true;
            }

            if (expression.Operator == RuleOperator.In)
            {
                return _orderItemRepository.TableUntracked
                    .Any(oi => oi.Order.CustomerId == context.Customer.Id && !oi.Order.Deleted && productIds.Contains(oi.ProductId));
            }

            if (expression.Operator == RuleOperator.NotIn)
            {
                return _orderItemRepository.TableUntracked
                    .Any(oi => oi.Order.CustomerId == context.Customer.Id && !oi.Order.Deleted && !productIds.Contains(oi.ProductId));
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
