using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Rules.Filters;
using SmartStore.Rules.Operators;

namespace SmartStore.Rules
{
    public abstract class RuleOperator : IEquatable<RuleOperator>
    {
        private readonly static IDictionary<string, RuleOperator> _map = new Dictionary<string, RuleOperator>(StringComparer.OrdinalIgnoreCase);

        public readonly static RuleOperator IsEqualTo = new EqualOperator();
        public readonly static RuleOperator IsNotEqualTo = new NotEqualOperator();
        public readonly static RuleOperator IsNull = new IsNullOperator();
        public readonly static RuleOperator IsNotNull = new IsNotNullOperator();
        public readonly static RuleOperator GreaterThanOrEqualTo = new GreaterThanOrEqualOperator();
        public readonly static RuleOperator GreaterThan = new GreaterThanOperator();
        public readonly static RuleOperator LessThanOrEqualTo = new LessThanOrEqualOperator();
        public readonly static RuleOperator LessThan = new LessThanOperator();
        public readonly static RuleOperator StartsWith = new StartsWithOperator();
        public readonly static RuleOperator EndsWith = new EndsWithOperator();
        public readonly static RuleOperator Contains = new ContainsOperator();
        public readonly static RuleOperator NotContains = new NotContainsOperator();
        public readonly static RuleOperator IsEmpty = new IsEmptyOperator();
        public readonly static RuleOperator IsNotEmpty = new IsNotEmptyOperator();
        public readonly static RuleOperator In = new InOperator();
        public readonly static RuleOperator NotIn = new NotInOperator();
        public readonly static RuleOperator AllIn = new AllInOperator();
        public readonly static RuleOperator NotAllIn = new NotAllInOperator();
        //public readonly static string All = new RuleOperator(); // TODO

        protected RuleOperator(string op)
        {
            Guard.NotEmpty(op, nameof(op));

            Operator = op;
            _map[op] = this;
        }

        public string Operator { get; set; }

        public override string ToString()
        {
            return Operator;
        }

        public static implicit operator string(RuleOperator obj)
        {
            return obj.Operator;
        }

        public static implicit operator RuleOperator(string obj)
        {
            return GetOperator(obj);
        }

        public static RuleOperator GetOperator(string op)
        {
            if (op.IsEmpty())
            {
                return null;
            }

            if (_map.TryGetValue(op, out var instance))
            {
                return instance;
            }

            throw new InvalidCastException("No rule operator has been registered for '{0}'.".FormatInvariant(op));
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((RuleOperator)obj);
        }

        public bool Equals(RuleOperator other)
        {
            return string.Equals(Operator, other.Operator);
        }

        public override int GetHashCode()
        {
            return Operator?.GetHashCode() ?? 0;
        }

        public bool Match(object left, object right)
        {
            return Match<object>(left, right);
        }

        public virtual bool Match<TLeft>(TLeft left, object right)
        {
            var body = GetExpression(
                ExpressionHelper.CreateConstantExpression(left, typeof(TLeft)),
                ExpressionHelper.CreateConstantExpression(right),
                true);

            var lambda = Expression.Lambda<Func<bool>>(body);

            return lambda.Compile().Invoke();
        }

        public Expression GetExpression(Expression left, Expression right, bool liftToNull)
        {
            var targetType = GetBodyType(left);
            bool valid = true;

            if (TypesAreDifferent(left, right))
            {
                if (!TryConvertExpressionTypes(ref left, ref right))
                {
                    valid = false;
                }
            }
            else if (targetType.IsEnumType() || right.Type.IsEnumType())
            {
                if (!TryPromoteNullableEnums(ref left, ref right))
                {
                    valid = false;
                }
            }
            else if (targetType.IsValueType && !TryConvertNullableValue(left, ref right))
            {
                valid = false;
            }

            if (!valid)
            {
                throw new ArgumentException("Operator '{0}' is incompatible with operand types '{1}' and '{2}'.".FormatInvariant(
                    this.Operator,
                    targetType.GetTypeName(),
                    right.Type.GetTypeName()));
            }

            return GenerateExpression(left, right, liftToNull);
        }

        protected abstract Expression GenerateExpression(Expression left, Expression right, bool liftToNull);

        #region Expression/Lambda stuff

        protected virtual bool TypesAreDifferent(Expression left, Expression right)
        {
            bool isEqualityCheck = this == RuleOperator.IsEqualTo || this == RuleOperator.IsNotEqualTo;
            return isEqualityCheck && GetBodyType(left) != GetBodyType(right);
            //return (isEqualityCheck && !GetBodyType(left).IsValueType) && !GetBodyType(right).IsValueType;
        }

        private bool TryConvertNullableValue(Expression left, ref Expression right)
        {
            var c = right as ConstantExpression;
            if (c == null)
                return true;

            var targetType = GetBodyType(left);
            if (targetType == right.Type)
                return true;

            var leftIsNullable = targetType.IsNullable(out _);
            var rightIsNullObj = ExpressionHelper.IsNullObjectConstantExpression(right);
            var rightIsList = false;

            if (!rightIsNullObj && right.Type.IsGenericType)
            {
                rightIsList = right.Type.GetGenericTypeDefinition() == typeof(List<>);
            }

            if (leftIsNullable || rightIsNullObj)
            {
                try
                {
                    var value = c.Value;
                    var handled = false;

                    if (!leftIsNullable && rightIsNullObj)
                    {
                        // Right is null, but left is NOT nullable: (int)null does not work, we need to create the default value (e.g. default(int))
                        value = Activator.CreateInstance(targetType);
                    }
                    else if (leftIsNullable && rightIsList)
                    {
                        handled = TryConvertList(targetType, ref right);
                    }

                    if (!handled)
                    {
                        right = Expression.Constant(value, targetType);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryConvertList(Type targetType, ref Expression right)
        {
            // If left is int?, but right is List<int>: make right List<int?>
            try
            {
                var value = ((ConstantExpression)right).Value;
                var listElemType = right.Type.GetGenericArguments()[0];
                if (!listElemType.IsNullable(out _))
                {
                    // List<T> > List<T?>
                    var nullableListType = typeof(List<>).MakeGenericType(targetType);
                    var nullableList = Activator.CreateInstance(nullableListType);
                    var addMethod = nullableListType.GetMethod("Add");
                    foreach (var item in (IEnumerable)value)
                    {
                        addMethod.Invoke(nullableList, new object[] { item.Convert(targetType) });
                    }

                    right = Expression.Constant(nullableList, nullableListType);
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool TryPromoteNullableEnums(ref Expression left, ref Expression right)
        {
            var targetType = GetBodyType(left);

            if (targetType != right.Type)
            {
                var expression = PromoteExpression(right, targetType, true);
                if (expression == null)
                {
                    expression = PromoteExpression(left, right.Type, true);
                    if (expression == null)
                    {
                        return false;
                    }
                    left = expression; // TODO > Expression <> LambdaExpression ??
                }
                else
                {
                    right = expression;
                }
            }

            return true;
        }

        private Expression PromoteExpression(Expression expr, Type type, bool exact)
        {
            var targetType = GetBodyType(expr);

            if (targetType == type)
            {
                return expr;
            }

            var expression = expr as ConstantExpression;

            if (expression != null && expression.Value == null && (!type.IsValueType || type.IsNullable(out _)))
            {
                return Expression.Constant(null, type);
            }
            if (!targetType.IsCompatibleWith(type))
            {
                return null;
            }
            if (!type.IsValueType && !exact)
            {
                return expr;
            }

            return Expression.Convert(expr, type);
        }

        private bool TryConvertExpressionTypes(ref Expression left, ref Expression right)
        {
            var targetType = GetBodyType(left);

            if (targetType != right.Type)
            {
                if (!targetType.IsAssignableFrom(right.Type))
                {
                    if (!right.Type.IsAssignableFrom(targetType))
                    {
                        return false;
                    }

                    left = Expression.Convert(left, right.Type); // TODO > UnaryExpression <> LambdaExpression ??
                }
                else
                {
                    right = Expression.Convert(right, targetType);
                }
            }

            return true;
        }

        protected Type GetBodyType(Expression expr)
        {
            if (expr is LambdaExpression lambda)
                return lambda.Body.Type;
            else
                return expr.Type;
        }

        #endregion
    }
}
