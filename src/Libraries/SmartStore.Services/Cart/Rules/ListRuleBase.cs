using System.Collections.Generic;
using System.Linq;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules
{
    public abstract class ListRuleBase<T> : IRule
    {
        protected readonly IEqualityComparer<T> _comparer;

        protected ListRuleBase(IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer;
        }

        protected abstract T GetValue(CartRuleContext context);

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var list = expression.Value as List<T>;
            if (!(list?.Any() ?? false))
            {
                return true;
            }

            var value = GetValue(context);

            if (object.Equals(value, default(T)))
            {
                return false;
            }
            if (expression.Operator == RuleOperator.In)
            {
                return list.Contains(value, _comparer);
            }
            if (expression.Operator == RuleOperator.NotIn)
            {
                return !list.Contains(value, _comparer);
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
