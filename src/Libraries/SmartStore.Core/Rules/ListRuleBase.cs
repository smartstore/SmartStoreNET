using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class ListRuleBase<T> : RuleBase
    {
        protected abstract T GetValue(RuleContext context);

        public override bool Match(RuleContext context)
        {
            var list = Expression.Value.Convert<List<T>>();
            if (list == null || list.Count == 0)
            {
                return true;
            }

            var value = GetValue(context);

            if (object.Equals(value, default(T)))
            {
                return false;
            }
            if (Expression.Operator == RuleOperator.In)
            {
                return list.Contains(value);
            }
            if (Expression.Operator == RuleOperator.NotIn)
            {
                return !list.Contains(value);
            }

            throw new InvalidRuleOperatorException(this);
        }
    }
}
