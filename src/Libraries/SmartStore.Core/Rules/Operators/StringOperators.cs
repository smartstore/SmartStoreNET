using System.Linq.Expressions;
using SmartStore.Rules.Filters;

namespace SmartStore.Rules.Operators
{
    internal sealed class IsNotEmptyOperator : IsEmptyOperator
    {
        internal IsNotEmptyOperator()
            : base("IsNotEmpty", true) { }

        #region Old
        //protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        //{
        //    if (GetBodyType(left) == typeof(string) && (!liftToNull || ExpressionHelper.IsNotNullConstantExpression(left)))
        //    {
        //        left = left.CallTrim(false);
        //        return Expression.NotEqual(left, ExpressionHelper.EmptyStringLiteral);
        //    }

        //    return Expression.NotEqual(left, ExpressionHelper.NullLiteral);

        //    //return Expression.AndAlso(
        //    //    Expression.NotEqual(left, ExpressionHelper.NullLiteral),
        //    //    Expression.NotEqual(left.TrimCall(false), ExpressionHelper.EmptyStringLiteral));
        //}
        #endregion
    }

    internal class IsEmptyOperator : RuleOperator
    {
        internal IsEmptyOperator()
            : this("IsEmpty", false) { }

        protected IsEmptyOperator(string op, bool negate)
            : base(op)
        {
            Negate = negate;
        }

        private bool Negate { get; set; }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            return Expression.Equal(
                left.CallIsNullOrEmpty(),
                Negate ? ExpressionHelper.FalseLiteral : ExpressionHelper.TrueLiteral);

            #region Old
            //var leftIsString = GetBodyType(left) == typeof(string) && (!liftToNull || ExpressionHelper.IsNotNullConstantExpression(left));

            ////if (ss)
            ////{
            ////    left = left.CallTrim(false);
            ////    //return Expression.Equal(left, ExpressionHelper.EmptyStringLiteral);
            ////}

            ////return Expression.Equal(left, ExpressionHelper.NullLiteral);

            //return Expression.OrElse(
            //    Expression.Equal(left, ExpressionHelper.NullLiteral),
            //    Expression.Equal(leftIsString ? left.CallTrim(false) : left, ExpressionHelper.EmptyStringLiteral));
            #endregion
        }
    }

    internal sealed class StartsWithOperator : RuleOperator
    {
        internal StartsWithOperator()
            : base("StartsWith") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            var methodInfo = ExpressionHelper.StringStartsWithMethod;
            return Expression.Equal(
                methodInfo.ToCaseInsensitiveStringMethodCall(left, right, liftToNull),
                ExpressionHelper.TrueLiteral);
        }
    }

    internal sealed class EndsWithOperator : RuleOperator
    {
        internal EndsWithOperator()
            : base("EndsWith") { }

        protected override Expression GenerateExpression(Expression left, Expression right, bool liftToNull)
        {
            var methodInfo = ExpressionHelper.StringEndsWithMethod;
            return Expression.Equal(
                methodInfo.ToCaseInsensitiveStringMethodCall(left, right, liftToNull),
                ExpressionHelper.TrueLiteral);
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
                ExpressionHelper.StringContainsMethod.ToCaseInsensitiveStringMethodCall(left, right, liftToNull),
                Negate ? ExpressionHelper.FalseLiteral : ExpressionHelper.TrueLiteral);
        }
    }
}
