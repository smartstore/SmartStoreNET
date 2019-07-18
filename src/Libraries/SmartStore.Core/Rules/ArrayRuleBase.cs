using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class ArrayRuleBase<T> : RuleBase where T : struct
    {
        protected abstract T GetValue(RuleContext context);

        public override bool Match(RuleContext context)
        {
            var arr = Expression.Value.Convert<IEnumerable<T>>();
            if (arr == null || !arr.Any())
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
                return arr.Contains(value);
            }
            if (Expression.Operator == RuleOperator.NotIn)
            {
                return !arr.Contains(value);
            }

            throw new InvalidRuleOperatorException(this);
        }

        public override void ApplyToQuery(QueryRuleContext context)
        {
            throw new NotSupportedException();
        }
    }
}
