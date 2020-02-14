using System;
using System.Collections.Generic;

namespace SmartStore.Rules
{
    public sealed class RuleType
    {
        public readonly static RuleType Boolean = new RuleType("bool", typeof(bool));
        public readonly static RuleType Int = new RuleType("int", typeof(int));
        public readonly static RuleType Float = new RuleType("float", typeof(float));
        public readonly static RuleType Money = new RuleType("money", typeof(decimal));
        public readonly static RuleType Guid = new RuleType("Guid", typeof(Guid));
        public readonly static RuleType DateTime = new RuleType("Date", typeof(DateTime));
        public readonly static RuleType NullableBoolean = new RuleType("bool?", typeof(bool?));
        public readonly static RuleType NullableInt = new RuleType("int?", typeof(int?));
        public readonly static RuleType NullableFloat = new RuleType("float?", typeof(float?));
        public readonly static RuleType NullableGuid = new RuleType("Guid?", typeof(Guid?));
        public readonly static RuleType NullableDateTime = new RuleType("Date?", typeof(DateTime?));
        public readonly static RuleType String = new RuleType("String", typeof(string));
        public readonly static RuleType IntArray = new RuleType("IntArray", typeof(List<int>));
        public readonly static RuleType FloatArray = new RuleType("FloatArray", typeof(List<float>));
        public readonly static RuleType StringArray = new RuleType("StringArray", typeof(List<string>));

        private RuleType(string name, Type clrType)
        {
            Guard.NotEmpty(name, nameof(name));
            Guard.NotNull(clrType, nameof(clrType));

            Name = name;
            ClrType = clrType;
        }

        public string Name { get; set; }
        public Type ClrType { get; set; }

        public IEnumerable<RuleOperator> GetValidOperators(bool isComparingSequences = false)
        {
            var isComparable = typeof(IComparable).IsAssignableFrom(ClrType);
            var isNullable = ClrType.IsNullable(out var nonNullableType);

            if (isNullable)
            {
                yield return RuleOperator.IsNull;
                yield return RuleOperator.IsNotNull;
            }

            if (isComparable)
            {
                yield return RuleOperator.IsEqualTo;
                yield return RuleOperator.IsNotEqualTo;
            }

            if (nonNullableType == typeof(int) || nonNullableType == typeof(float) || nonNullableType == typeof(DateTime) || nonNullableType == typeof(decimal))
            {
                yield return RuleOperator.GreaterThanOrEqualTo;
                yield return RuleOperator.GreaterThan;
                yield return RuleOperator.LessThanOrEqualTo;
                yield return RuleOperator.LessThan;
            }

            if (nonNullableType == typeof(string))
            {
                yield return RuleOperator.IsEmpty;
                yield return RuleOperator.IsNotEmpty;
                yield return RuleOperator.StartsWith;
                yield return RuleOperator.EndsWith;
                yield return RuleOperator.Contains;
                yield return RuleOperator.NotContains;
            }

            if (nonNullableType == typeof(List<int>) || nonNullableType == typeof(List<float>) || nonNullableType == typeof(List<string>))
            {
                if (isComparingSequences)
                {
                    yield return RuleOperator.In;
                    yield return RuleOperator.NotIn;
                    yield return RuleOperator.Contains;
                    yield return RuleOperator.NotContains;
                    yield return RuleOperator.AllIn;
                    yield return RuleOperator.NotAllIn;
                    yield return RuleOperator.IsEqualTo;
                    yield return RuleOperator.IsNotEqualTo;
                }
                else
                {
                    yield return RuleOperator.In;
                    yield return RuleOperator.NotIn;
                }
            }
        }
    }
}