using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedFromManufacturerRule : IRule
    {
        private readonly IRepository<OrderItem> _orderItemRepository;

        public PurchasedFromManufacturerRule(IRepository<OrderItem> orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var manuIds = expression.Value as List<int>;
            if (!(manuIds?.Any() ?? false))
            {
                return true;
            }

            if (expression.Operator == RuleOperator.In)
            {
                return _orderItemRepository.TableUntracked
                    .Any(oi => oi.Order.CustomerId == context.Customer.Id && oi.Product.ProductManufacturers.Any(pm => manuIds.Contains(pm.ManufacturerId)));
            }

            if (expression.Operator == RuleOperator.NotIn)
            {
                return _orderItemRepository.TableUntracked
                    .Any(oi => oi.Order.CustomerId == context.Customer.Id && oi.Product.ProductManufacturers.Any(pm => !manuIds.Contains(pm.ManufacturerId)));
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
