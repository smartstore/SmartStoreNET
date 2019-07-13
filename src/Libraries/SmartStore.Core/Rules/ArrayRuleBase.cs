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
            var arr = Expression.Comparand.Convert<IEnumerable<T>>();
            if (arr == null || !arr.Any())
            {
                return true;
            }

            var value = GetValue(context);

            if (object.Equals(value, default(T)))
            {
                return false;
            }
            if (Expression.Operator == RuleOperators.In)
            {
                return arr.Contains(value);
            }
            if (Expression.Operator == RuleOperators.NotIn)
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
