using System;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class IsNullOperator : RuleOperator
    {
        internal IsNullOperator() : base("IsNull") { }
        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.Equal(left, ExpressionHelper.NullLiteral);
        }
    }

    internal sealed class IsNotNullOperator : RuleOperator
    {
        internal IsNotNullOperator() : base("IsNotNull") { }
        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.NotEqual(left, ExpressionHelper.NullLiteral);
        }
    }
}
