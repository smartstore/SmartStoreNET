using System;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class IsEmptyOperator : RuleOperator
    {
        internal IsEmptyOperator() 
            : base("IsEmpty") { }

        public override Expression GenerateExpression(Expression left, Expression right)
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

        public override Expression GenerateExpression(Expression left, Expression right)
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

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            var methodInfo = ExpressionHelper.StringStartsWithMethodInfo;
            return Expression.Equal(methodInfo.ToCaseInsensitiveStringMethodCall(left, right), ExpressionHelper.TrueLiteral);
        }
    }

    internal sealed class EndsWithOperator : RuleOperator
    {
        internal EndsWithOperator() 
            : base("EndsWith") { }

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            var methodInfo = ExpressionHelper.StringEndsWithMethodInfo;
            return Expression.Equal(methodInfo.ToCaseInsensitiveStringMethodCall(left, right), ExpressionHelper.TrueLiteral);
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

        public override Expression GenerateExpression(Expression left, Expression right)
        {
            return Expression.Equal(
                ExpressionHelper.StringContainsMethodInfo.ToCaseInsensitiveStringMethodCall(left, right), 
                Negate ? ExpressionHelper.FalseLiteral : ExpressionHelper.TrueLiteral);
        }
    }
}
