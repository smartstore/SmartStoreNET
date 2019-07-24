using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules
{
    public abstract class ListRuleBase<T> : IRule
    {
        protected abstract T GetValue(CartRuleContext context);

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var list = expression.Value as List<T>;
            if (list == null || list.Count == 0)
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
                return list.Contains(value);
            }
            if (expression.Operator == RuleOperator.NotIn)
            {
                return !list.Contains(value);
            }

            throw new InvalidRuleOperatorException(expression);
        }
    }
}
