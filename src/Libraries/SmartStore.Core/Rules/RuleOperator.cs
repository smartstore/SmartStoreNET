using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Rules.Operators;

namespace SmartStore.Rules
{
    public abstract class RuleOperator : IEquatable<RuleOperator>
    {
        private readonly static IDictionary<string, RuleOperator> _map = new Dictionary<string, RuleOperator>(StringComparer.OrdinalIgnoreCase);

        public readonly static RuleOperator EqualTo = new EqualOperator();
        public readonly static RuleOperator NotEqualTo = new NotEqualOperator();
        public readonly static RuleOperator IsNull = new IsNullOperator();
        public readonly static RuleOperator IsNotNull = new IsNotNullOperator();
        public readonly static RuleOperator GreaterThanOrEqualTo = new GreaterThanOrEqualOperator();
        public readonly static RuleOperator GreaterThan = new GreaterThanOperator();
        public readonly static RuleOperator LessThanOrEqualTo = new LessThanOrEqualOperator();
        public readonly static RuleOperator LessThan = new LessThanOperator();
        public readonly static RuleOperator StartsWith = new StartsWithOperator();
        public readonly static RuleOperator EndsWith = new EndsWithOperator();
        public readonly static RuleOperator Contains = new ContainsOperator();
        public readonly static RuleOperator NotContains = new NotContainsOperator();
        public readonly static RuleOperator IsEmpty = new IsEmptyOperator();
        public readonly static RuleOperator IsNotEmpty = new IsNotEmptyOperator();
        public readonly static RuleOperator In = new InOperator();
        public readonly static RuleOperator NotIn = new NotInOperator();
        //public readonly static string All = new RuleOperator(); // TODO

        protected RuleOperator(string op)
        {
            Guard.NotEmpty(op, nameof(op));

            Operator = op;
            _map[op] = this;
        }

        public abstract Expression GenerateExpression(Expression left, Expression right);

        public string Operator { get; set; }

        public override string ToString() => Operator;

        public static implicit operator string(RuleOperator obj) => obj.Operator;

        public static implicit operator RuleOperator(string obj) => GetOperator(obj);

        public static RuleOperator GetOperator(string op)
        {
            if (op.IsEmpty())
            {
                return null;
            }

            if (_map.TryGetValue(op, out var instance))
            {
                return instance;
            } 

            throw new InvalidCastException("No rule operator has been registered for '{0}'.".FormatInvariant(op));
        }

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
