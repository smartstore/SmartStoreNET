using System;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class EqualOperator : RuleOperator
    {
        internal EqualOperator() 
            : base("=") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            if (left.Type == typeof(string))
            {
                left = left.ToLowerCall(liftToNull);
                right = right.ToLowerCall(liftToNull);
            }

            return Expression.Equal(left, right);
        }
    }

    internal sealed class NotEqualOperator : RuleOperator
    {
        internal NotEqualOperator() 
            : base("!=") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            if (left.Type == typeof(string))
            {
                left = left.ToLowerCall(liftToNull);
                right = right.ToLowerCall(liftToNull);
            }

            return Expression.NotEqual(left, right);
        }
    }

    internal sealed class IsNullOperator : RuleOperator
    {
        internal IsNullOperator() 
            : base("IsNull") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.Equal(left, ExpressionHelper.NullLiteral);
        }
    }

    internal sealed class IsNotNullOperator : RuleOperator
    {
        internal IsNotNullOperator() 
            : base("IsNotNull") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.NotEqual(left, ExpressionHelper.NullLiteral);
        }
    }
}
