using System;
using System.Linq.Expressions;

namespace SmartStore.Rules.Operators
{
    internal sealed class LessThanOperator : RuleOperator
    {
        internal LessThanOperator() 
            : base("<") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.LessThan(left, right);
        }
    }

    internal sealed class LessThanOrEqualOperator : RuleOperator
    {
        internal LessThanOrEqualOperator() 
            : base("<=") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.LessThanOrEqual(left, right);
        }
    }

    internal sealed class GreaterThanOperator : RuleOperator
    {
        internal GreaterThanOperator() 
            : base(">") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.GreaterThan(left, right);
        }
    }

    internal sealed class GreaterThanOrEqualOperator : RuleOperator
    {
        internal GreaterThanOrEqualOperator() 
            : base(">=") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.GreaterThanOrEqual(left, right);
        }
    }
}
