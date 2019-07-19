using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public static Expression ToLowerCall(this Expression stringExpression, bool liftToNull)
        {
            if (liftToNull)
            {
                stringExpression = LiftStringExpressionToEmpty(stringExpression);
            }

            return Expression.Call(stringExpression, StringToLowerMethodInfo);
        }

        public static Expression ToCaseInsensitiveStringMethodCall(this MethodInfo methodInfo, Expression left, Expression right, bool liftToNull)
        {
            var leftCall = ToLowerCall(left, liftToNull);
            var rightCall = ToLowerCall(right, liftToNull);

            if (methodInfo.IsStatic)
            {
                return Expression.Call(methodInfo, new Expression[] { leftCall, rightCall });
            }

            return Expression.Call(leftCall, methodInfo, new Expression[] { rightCall });
        }

        public static Expression LiftStringExpressionToEmpty(Expression stringExpression)
        {
            if (stringExpression.Type != typeof(string))
            {
                throw new ArgumentException("Provided expression should be string type", nameof(stringExpression));
            }

            if (IsNotNullConstantExpression(stringExpression))
            {
                return stringExpression;
            }

            return Expression.Coalesce(stringExpression, EmptyStringLiteral);
        }

        public static bool IsNotNullConstantExpression(Expression expression)
        {
            if (expression is ConstantExpression constantExpr)
            {
                return constantExpr.Value != null;
            }

            return false;
        }

        public static MethodInfo GetCollectionContainsMethod(Type itemType)
        {
            return typeof(ICollection<>).MakeGenericType(itemType).GetMethod("Contains", new Type[] { itemType });
        }

        public static Expression CreateValueExpression(Type targetType, object value, CultureInfo culture = null)
        {
            var targetIsNullable = targetType.IsNullable(out var nonNullableType);

            if (((targetType != typeof(string)) && (!targetType.IsValueType || targetIsNullable)) && (string.Compare(value as string, "null", StringComparison.OrdinalIgnoreCase) == 0))
            {
                value = null;
            }

            if (value != null)
            {
                if (value.GetType() != nonNullableType)
                {
                    if (nonNullableType.IsEnum)
                    {
                        value = Enum.Parse(nonNullableType, value.ToString(), true);
                    }
                    else if (value is IConvertible)
                    {
                        value = Convert.ChangeType(value, nonNullableType, culture ?? CultureInfo.InvariantCulture);
                    }
                }
            }

            return CreateConstantExpression(value);
        }

        public static Expression CreateConstantExpression(object value)
        {
            return value == null ? NullLiteral : Expression.Constant(value);
        }

        public static LambdaExpression CreateLambdaExpression(ParameterExpression p, Expression body)
        {
            return Expression.Lambda(
                new FilterExpressionVisitor(p).Visit(body), 
                new[] { p });
        }
    }
}
