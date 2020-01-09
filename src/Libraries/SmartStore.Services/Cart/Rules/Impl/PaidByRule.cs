using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PaidByRule :  IRule
    {
        private readonly IRepository<Order> _orderRepository;

        public PaidByRule(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var paymentMethods = expression.Value as List<string>;
            if (!(paymentMethods?.Any() ?? false))
            {
                return true;
            }

            if (expression.Operator == RuleOperator.In)
            {
                return _orderRepository.TableUntracked.Any(o => o.CustomerId == context.Customer.Id && !o.Deleted && paymentMethods.Contains(o.PaymentMethodSystemName));
            }

            if (expression.Operator == RuleOperator.NotIn)
            {
                return _orderRepository.TableUntracked.Any(o => o.CustomerId == context.Customer.Id && !o.Deleted && !paymentMethods.Contains(o.PaymentMethodSystemName));
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
