using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartStore.Rules.Filters
{
    internal static class ExpressionHelper
    {
        public readonly static Expression TrueLiteral = Expression.Constant(true);
        public readonly static Expression FalseLiteral = Expression.Constant(false);
        public readonly static Expression NullLiteral = Expression.Constant(null);
        public readonly static Expression ZeroLiteral = Expression.Constant(0);
        public readonly static Expression EmptyStringLiteral = Expression.Constant(string.Empty);

        public readonly static MethodInfo StringToLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[0]);
        public readonly static MethodInfo StringStartsWithMethodInfo = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        public readonly static MethodInfo StringEndsWithMethodInfo = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        public readonly static MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) });
        public readonly static MethodInfo StringContainsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

        public static Expression ToLowerCall(this Expression stringExpression)
        {
            return Expression.Call(stringExpression, StringToLowerMethodInfo);
        }

        public static Expression ToCaseInsensitiveStringMethodCall(this MethodInfo methodInfo, Expression left, Expression right)
        {
            var leftCall = ToLowerCall(left);
            var rightCall = ToLowerCall(right);

            if (methodInfo.IsStatic)
            {
                return Expression.Call(methodInfo, new Expression[] { leftCall, rightCall });
            }

            return Expression.Call(leftCall, methodInfo, new Expression[] { rightCall });
        }

        public static MethodInfo GetCollectionContainsMethod(Type itemType)
        {
            return typeof(ICollection<>).MakeGenericType(itemType).GetMethod("Contains", new Type[] { itemType });
        }
    }
}
