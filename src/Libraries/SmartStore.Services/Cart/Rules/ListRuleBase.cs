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

        protected virtual T GetValue(CartRuleContext context) => default;

        protected virtual IEnumerable<T> GetValues(CartRuleContext context) => default;

        public virtual bool Match(CartRuleContext context, RuleExpression expression)
        {
            var list = expression.Value as List<T>;
            if (!(list?.Any() ?? false))
            {
                return true;
            }

            var values = GetValues(context);

            if (values == null)
            {
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
            }
            else
            {
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

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
