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

        protected virtual T GetValue(CartRuleContext context)
        {
            return default;
        }

        protected virtual IEnumerable<T> GetValues(CartRuleContext context)
        {
            return default;
        }

        public virtual bool Match(CartRuleContext context, RuleExpression expression)
        {
            var right = expression.Value as List<T>;
            if (!(right?.Any() ?? false))
            {
                return true;
            }

            var left = GetValues(context);

            if (left == null)
            {
                var value = GetValue(context);

                if (object.Equals(value, default(T)))
                {
                    return false;
                }

                if (expression.Operator == RuleOperator.In)
                {
                    return right.Contains(value, _comparer);
                }
                if (expression.Operator == RuleOperator.NotIn)
                {
                    return !right.Contains(value, _comparer);
                }
            }
            else
            {
                if (expression.Operator == RuleOperator.IsEqualTo)
                {
                    return !right.Except(left, _comparer).Any();
                }
                else if (expression.Operator == RuleOperator.IsNotEqualTo)
                {
                    return right.Except(left, _comparer).Any();
                }
                else if (expression.Operator == RuleOperator.Contains)
                {
                    // FALSE for left { 3,2,1 } and right { 0,1,2,3 }.
                    return right.All(x => left.Contains(x, _comparer));
                }
                else if (expression.Operator == RuleOperator.NotContains)
                {
                    return right.All(x => !left.Contains(x, _comparer));
                }
                else if (expression.Operator == RuleOperator.In)
                {
                    return left.Any(x => right.Contains(x, _comparer));
                }
                else if (expression.Operator == RuleOperator.NotIn)
                {
                    return left.Any(x => !right.Contains(x, _comparer));
                }
                else if (expression.Operator == RuleOperator.AllIn)
                {
                    // TRUE for left { 3,2,1 } and right { 0,1,2,3 }.
                    return left.All(x => right.Contains(x, _comparer));
                }
                else if (expression.Operator == RuleOperator.NotAllIn)
                {
                    return left.All(x => !right.Contains(x, _comparer));
                }
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
