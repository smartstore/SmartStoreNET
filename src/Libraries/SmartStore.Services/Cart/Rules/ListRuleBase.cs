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

        protected abstract object GetValue(CartRuleContext context);

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var list = expression.Value as List<T>;
            if (!(list?.Any() ?? false))
            {
                return true;
            }

            var value = GetValue(context);

            if (value is IEnumerable<T> values)
            {
                if (values == null)
                {
                    return false;
                }

                if (expression.Operator == RuleOperator.IsEqualTo)
                {
                    return !list.Except(values).Any();
                }
                else if (expression.Operator == RuleOperator.IsNotEqualTo)
                {
                    return list.Except(values).Any();
                }
                else if (expression.Operator == RuleOperator.Contains)
                {
                    // FALSE for list { 0,1,2,3 } and values { 3,2,1 }
                    return list.All(x => values.Contains(x));
                }
                else if (expression.Operator == RuleOperator.NotContains)
                {
                    return list.All(x => !values.Contains(x));
                }
                else if (expression.Operator == RuleOperator.In)
                {
                    return values.Any(x => list.Contains(x));
                }
                else if (expression.Operator == RuleOperator.NotIn)
                {
                    return values.Any(x => !list.Contains(x));
                }
                else if (expression.Operator == RuleOperator.AllIn)
                {
                    // TRUE for list { 0,1,2,3 } and values { 3,2,1 }
                    return values.All(x => list.Contains(x));
                }
                else if (expression.Operator == RuleOperator.NotAllIn)
                {
                    return values.All(x => !list.Contains(x));
                }
            }
            else
            {
                if (object.Equals(value, default(T)))
                {
                    return false;
                }

                if (expression.Operator == RuleOperator.In)
                {
                    return list.Contains((T)value, _comparer);
                }
                if (expression.Operator == RuleOperator.NotIn)
                {
                    return !list.Contains((T)value, _comparer);
                }
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
