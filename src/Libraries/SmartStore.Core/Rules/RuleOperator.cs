using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class RuleOperator : IEquatable<RuleOperator>
    {
        protected RuleOperator(string op)
        {
            Guard.NotEmpty(op, nameof(op));
            Operator = op;
        }

        public abstract Expression GenerateExpression(Expression left, Expression right);

        public string Operator { get; set; }

        public override string ToString() => Operator;

        public static implicit operator string(RuleOperator obj) => obj.Operator;

        //public static implicit operator RuleOperator2(string obj) => new RuleOperator(obj);

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((RuleOperator)obj);
        }

        public bool Equals(RuleOperator other)
        {
            return string.Equals(Operator, other.Operator);
        }

        public override int GetHashCode()
        {
            return Operator?.GetHashCode() ?? 0;
        }
    }
}
