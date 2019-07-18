using System;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class EqualOperator : RuleOperator
    {
        internal EqualOperator() 
            : base("=") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                left = left.ToLowerCall();
                right = right.ToLowerCall();
            }

            return Expression.Equal(left, right);
        }
    }

    internal sealed class NotEqualOperator : RuleOperator
    {
        internal NotEqualOperator() 
            : base("!=") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            if (left.Type == typeof(string))
            {
                left = left.ToLowerCall();
                right = right.ToLowerCall();
            }

            return Expression.NotEqual(left, right);
        }
    }

    internal sealed class IsNullOperator : RuleOperator
    {
        internal IsNullOperator() 
            : base("IsNull") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.Equal(left, ExpressionHelper.NullLiteral);
        }
    }

    internal sealed class IsNotNullOperator : RuleOperator
    {
        internal IsNotNullOperator() 
            : base("IsNotNull") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.NotEqual(left, ExpressionHelper.NullLiteral);
        }
    }
}
