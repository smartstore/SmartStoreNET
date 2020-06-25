//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using SmartStore.Core.Utilities;

//namespace SmartStore.Rules.Filters
//{
//    internal sealed class FilterExpressionBuilder
//    {
//        private readonly ParameterExpression _parameterExpression;
//        private readonly FilterExpression _filter;

//        public FilterExpressionBuilder(ParameterExpression parameterExpression, FilterExpression filter)
//        {
//            _parameterExpression = parameterExpression;
//            _filter = filter;
//        }

//        public LambdaExpression CreateFilterExpression()
//        {
//            return Expression.Lambda(CreateBodyExpression(), new[] { _parameterExpression });
//        }

//        private Expression CreateBodyExpression()
//        {
//            var descriptor = _filter.Descriptor;
//            var memberExpression = descriptor.MemberExpression;
//            var targetType = memberExpression.Body.Type;

//            var valueExpression = CreateValueExpression(targetType, _filter.Value, CultureInfo.InvariantCulture);
//            bool flag = true;

//            if (TypesAreDifferent(memberExpression, valueExpression))
//            {
//                if (!TryConvertExpressionTypes(ref memberExpression, ref valueExpression))
//                {
//                    flag = false;
//                }
//            }
//            else if (targetType.IsEnumType() || valueExpression.Type.IsEnumType())
//            {
//                if (!TryPromoteNullableEnums(ref memberExpression, ref valueExpression))
//                {
//                    flag = false;
//                }
//            }
//            else if ((targetType.IsNullable(out _) && (targetType != valueExpression.Type)) && !TryConvertNullableValue(memberExpression, ref valueExpression))
//            {
//                flag = false;
//            }

//            if (!flag)
//            {
//                throw new ArgumentException("Operator '{0}' is incompatible with operand types '{1}' and '{2}'.".FormatInvariant(
//                    _filter.Operator.Operator,
//                    targetType.GetTypeName(), 
//                    valueExpression.Type.GetTypeName()));
//            }

//            return descriptor.GetExpression(_filter.Operator, valueExpression);
//        }

//        //public Expression CreateMemberExpression()
//        //{
//        //    Type memberType = _filter.Descriptor.Type.ClrType;
//        //    var memberName = _filter.Descriptor.Member;

//        //    var expression = CreateMemberAccessExpression(_parameterExpression, memberType, memberName);
//        //    if ((memberType != null) && (expression.Type.GetNonNullableType() != memberType.GetNonNullableType()))
//        //    {
//        //        expression = Expression.Convert(expression, memberType);
//        //    }

//        //    return expression;
//        //}

//        private Expression CreateValueExpression(Type targetType, object value, CultureInfo culture)
//        {
//            if (((targetType != typeof(string)) && (!targetType.IsValueType || targetType.IsNullable(out _))) && (string.Compare(value as string, "null", StringComparison.OrdinalIgnoreCase) == 0))
//            {
//                value = null;
//            }

//            if (value != null)
//            {
//                var nonNullableType = targetType.GetNonNullableType();
//                if (value.GetType() != nonNullableType)
//                {
//                    if (nonNullableType.IsEnum)
//                    {
//                        value = Enum.Parse(nonNullableType, value.ToString(), true);
//                    }
//                    else if (value is IConvertible)
//                    {
//                        value = Convert.ChangeType(value, nonNullableType, culture);
//                    }
//                }
//            }

//            return CreateConstantExpression(value);
//        }

//        //private Expression CreateMemberAccessExpression(Expression instance, Type memberType, string memberName)
//        //{
//        //    foreach (var token in MemberAccessTokenizer.GetTokens(memberName))
//        //    {
//        //        instance = token.CreateMemberAccessExpression(_parameterExpression);
//        //    }

//        //    return instance;
//        //}

//        private Expression CreateConstantExpression(object value)
//        {
//            return value == null
//                ? ExpressionHelper.NullLiteral
//                : Expression.Constant(value);
//        }

//        private Expression PromoteExpression(Expression expr, Type type, bool exact)
//        {
//            if (expr.Type == type)
//            {
//                return expr;
//            }

//            var expression = expr as ConstantExpression;

//            if (((expression != null) && (expression == ExpressionHelper.NullLiteral)) && (!type.IsValueType || type.IsNullable(out _)))
//            {
//                return Expression.Constant(null, type);
//            }
//            if (!expr.Type.IsCompatibleWith(type))
//            {
//                return null;
//            }
//            if (!type.IsValueType && !exact)
//            {
//                return expr;
//            }

//            return Expression.Convert(expr, type);
//        }

//        private static bool TryConvertExpressionTypes(ref LambdaExpression memberExpression, ref Expression valueExpression)
//        {
//            var targetType = memberExpression.Body.Type;

//            if (targetType != valueExpression.Type)
//            {
//                if (!targetType.IsAssignableFrom(valueExpression.Type))
//                {
//                    if (!valueExpression.Type.IsAssignableFrom(targetType))
//                    {
//                        return false;
//                    }
//                    //memberExpression = Expression.Convert(memberExpression, valueExpression.Type); // TODO > UnaryExpression <> LambdaExpression ??
//                }
//                else
//                {
//                    valueExpression = Expression.Convert(valueExpression, memberExpression.Type);
//                }
//            }

//            return true;
//        }

//        private bool TryConvertNullableValue(LambdaExpression memberExpression, ref Expression valueExpression)
//        {
//            var expression = valueExpression as ConstantExpression;
//            if (expression != null)
//            {
//                try
//                {
//                    valueExpression = Expression.Constant(expression.Value, memberExpression.Body.Type);
//                }
//                catch (ArgumentException)
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        private bool TryPromoteNullableEnums(ref LambdaExpression memberExpression, ref Expression valueExpression)
//        {
//            var targetType = memberExpression.Body.Type;

//            if (targetType != valueExpression.Type)
//            {
//                var expression = PromoteExpression(valueExpression, targetType, true);
//                if (expression == null)
//                {
//                    expression = PromoteExpression(memberExpression, valueExpression.Type, true);
//                    if (expression == null)
//                    {
//                        return false;
//                    }
//                    //memberExpression = expression; // TODO > Expression <> LambdaExpression ??
//                }
//                else
//                {
//                    valueExpression = expression;
//                }
//            }

//            return true;
//        }

//        private bool TypesAreDifferent(LambdaExpression memberExpression, Expression valueExpression)
//        {
//            //return ((((descriptor.Operator == FilterOperator.IsEqualTo) || (descriptor.Operator == FilterOperator.IsNotEqualTo)) && !memberExpression.Type.IsValueType) && !valueExpression.Type.IsValueType);

//            //// TODO: Implement
//            return false;
//        }
//    }
//}
