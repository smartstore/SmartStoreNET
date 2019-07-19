using System;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class IsEmptyOperator : RuleOperator
    {
        internal IsEmptyOperator() 
            : base("IsEmpty") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.OrElse(
                Expression.Equal(left, ExpressionHelper.NullLiteral),
                Expression.AndAlso(
                    Expression.NotEqual(left, ExpressionHelper.NullLiteral),
                    Expression.Equal(left, ExpressionHelper.EmptyStringLiteral)));
        }
    }

    internal sealed class IsNotEmptyOperator : RuleOperator
    {
        internal IsNotEmptyOperator() 
            : base("IsNotEmpty") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.AndAlso(
                Expression.NotEqual(left, ExpressionHelper.NullLiteral),
                Expression.NotEqual(left, ExpressionHelper.EmptyStringLiteral));
        }
    }

    internal sealed class StartsWithOperator : RuleOperator
    {
        internal StartsWithOperator() 
            : base("StartsWith") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            var methodInfo = ExpressionHelper.StringStartsWithMethodInfo;
            return Expression.Equal(methodInfo.ToCaseInsensitiveStringMethodCall(left, right, liftToNull), ExpressionHelper.TrueLiteral);
        }
    }

    internal sealed class EndsWithOperator : RuleOperator
    {
        internal EndsWithOperator() 
            : base("EndsWith") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            var methodInfo = ExpressionHelper.StringEndsWithMethodInfo;
            return Expression.Equal(methodInfo.ToCaseInsensitiveStringMethodCall(left, right, liftToNull), ExpressionHelper.TrueLiteral);
        }
    }

    internal sealed class NotContainsOperator : ContainsOperator
    {
        internal NotContainsOperator()
            : base("NotContains", true) { }
    }

    internal class ContainsOperator : RuleOperator
    {
        internal ContainsOperator() 
            : this("Contains", false) { }

        protected ContainsOperator(string op, bool negate)
            : base(op)
        {
            Negate = negate;
        }

        private bool Negate { get; set; }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.Equal(
                ExpressionHelper.StringContainsMethodInfo.ToCaseInsensitiveStringMethodCall(left, right, liftToNull), 
                Negate ? ExpressionHelper.FalseLiteral : ExpressionHelper.TrueLiteral);
        }
    }
}
