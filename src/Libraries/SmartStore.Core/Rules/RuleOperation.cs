using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public sealed class RuleOperation : IEquatable<RuleOperation>
    {
        public readonly static RuleOperation EqualTo = new RuleOperation("=");
        public readonly static RuleOperation NotEqualTo = new RuleOperation("!=");
        public readonly static RuleOperation GreaterThanOrEqualTo = new RuleOperation(">=");
        public readonly static RuleOperation GreaterThan = new RuleOperation(">");
        public readonly static RuleOperation LessThanOrEqualTo = new RuleOperation("<=");
        public readonly static RuleOperation LessThan = new RuleOperation("<");
        public readonly static RuleOperation Between = new RuleOperation("Between");
        public readonly static RuleOperation StartsWith = new RuleOperation("StartsWith");
        public readonly static RuleOperation EndsWith = new RuleOperation("EndsWith");
        public readonly static RuleOperation Contains = new RuleOperation("Contains");
        public readonly static RuleOperation NotContains = new RuleOperation("NotContains");
        public readonly static RuleOperation IsNull = new RuleOperation("IsNull");
        public readonly static RuleOperation IsNotNull = new RuleOperation("IsNotNull");
        public readonly static RuleOperation In = new RuleOperation("In");
        public readonly static RuleOperation NotIn = new RuleOperation("NotIn");
        //public readonly static string All = new RuleOperator("All");

        private RuleOperation(string op)
        {
            Guard.NotEmpty(op, nameof(op));
            Operator = op;
        }

        public Expression GenerateExpression(Expression left, Expression right)
        {
            // TODO: to be removed later
            return null;
        }

        public string Operator { get; set; }

        public override string ToString() => Operator;

        public static implicit operator string(RuleOperation obj) => obj.Operator;

        public static implicit operator RuleOperation(string obj) => new RuleOperation(obj);

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((RuleOperation)obj);
        }

        public bool Equals(RuleOperation other)
        {
            return string.Equals(Operator, other.Operator);
        }

        public override int GetHashCode()
        {
            return Operator?.GetHashCode() ?? 0;
        }
    }
}
