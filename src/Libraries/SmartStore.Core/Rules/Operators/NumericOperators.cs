using System.Linq.Expressions;

namespace SmartStore.Rules.Operators
{
    internal sealed class LessThanOperator : RuleOperator
    {
        internal LessThanOperator()
            : base("<") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.LessThan(left, right);
        }
    }

    internal sealed class LessThanOrEqualOperator : RuleOperator
    {
        internal LessThanOrEqualOperator()
            : base("<=") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.LessThanOrEqual(left, right);
        }
    }

    internal sealed class GreaterThanOperator : RuleOperator
    {
        internal GreaterThanOperator()
            : base(">") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.GreaterThan(left, right);
        }
    }

    internal sealed class GreaterThanOrEqualOperator : RuleOperator
    {
        internal GreaterThanOrEqualOperator()
            : base(">=") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.GreaterThanOrEqual(left, right);
        }
    }
}
