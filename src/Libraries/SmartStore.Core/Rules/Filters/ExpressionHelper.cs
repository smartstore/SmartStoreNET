using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartStore.Rules.Filters
{
    internal static class ExpressionHelper
    {
        public readonly static Expression TrueLiteral = Expression.Constant(true);
        public readonly static Expression FalseLiteral = Expression.Constant(false);
        public readonly static Expression NullLiteral = Expression.Constant(null);
        public readonly static Expression ZeroLiteral = Expression.Constant(0);
        public readonly static Expression EmptyStringLiteral = Expression.Constant(string.Empty);

        public readonly static MethodInfo StringToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), new Type[0]);
        public readonly static MethodInfo StringTrimMethod = typeof(string).GetMethod(nameof(string.Trim), new Type[0]);
        public readonly static MethodInfo StringIsNullOrEmptyMethod = typeof(string).GetMethod("IsNullOrEmpty", new Type[] { typeof(string) });
        public readonly static MethodInfo StringStartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
        public readonly static MethodInfo StringEndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });
        //public readonly static MethodInfo StringCompareMethod = typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) });
        public readonly static MethodInfo StringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });

        public static Expression CallToLower(this Expression stringExpression, bool liftToNull)
        {
            if (liftToNull)
            {
                stringExpression = LiftStringExpressionToEmpty(stringExpression);
            }

            return Expression.Call(stringExpression, StringToLowerMethod);
        }

        public static Expression CallIsNullOrEmpty(this Expression stringExpression)
        {
            return Expression.Call(StringIsNullOrEmptyMethod, stringExpression);
        }

        public static Expression CallTrim(this Expression stringExpression, bool liftToNull)
        {
            if (liftToNull)
            {
                stringExpression = LiftStringExpressionToEmpty(stringExpression);
            }

            return Expression.Call(stringExpression, StringTrimMethod);
        }

        public static Expression ToCaseInsensitiveStringMethodCall(this MethodInfo methodInfo, Expression left, Expression right, bool liftToNull)
        {
            var leftCall = CallToLower(left, liftToNull);
            var rightCall = CallToLower(right, liftToNull);

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

            if (IsNotNullConstantExpression(stringExpression, out _))
            {
                return stringExpression;
            }

            return Expression.Coalesce(stringExpression, EmptyStringLiteral);
        }

        public static bool IsNotNullConstantExpression(Expression expression, out object value)
        {
            value = null;

            if (expression is ConstantExpression c)
            {
                value = c.Value;
                return value != null;
            }

            return false;
        }

        public static bool IsNullObjectConstantExpression(Expression expression)
        {
            if (expression is ConstantExpression c)
            {
                return c.Value == null && c.Type == typeof(object);
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
                        if (typeof(IConvertible).IsAssignableFrom(nonNullableType))
                        {
                            value = Convert.ChangeType(value, nonNullableType, culture ?? CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return CreateConstantExpression(value);
        }

        public static Expression CreateConstantExpression(object value, Type type = null)
        {
            if (type != null && type != typeof(object))
            {
                return Expression.Constant(value, type);
            }

            if (value != null)
            {
                return Expression.Constant(value);
            }

            return NullLiteral;
        }

        public static LambdaExpression CreateLambdaExpression(ParameterExpression p, Expression body)
        {
            return Expression.Lambda(
                new FilterExpressionVisitor(p).Visit(body),
                new[] { p });
        }
    }
}
