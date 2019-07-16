using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public sealed class RuleType
    {
        public readonly static RuleType Boolean = new RuleType("bool", typeof(bool));
        public readonly static RuleType Int = new RuleType("int", typeof(int));
        public readonly static RuleType Float = new RuleType("float", typeof(float));
        public readonly static RuleType Guid = new RuleType("guid", typeof(Guid));
        public readonly static RuleType DateTime = new RuleType("guid", typeof(DateTime));
        public readonly static RuleType String = new RuleType("guid", typeof(string));
        public readonly static RuleType IntArray = new RuleType("guid", typeof(int[]));
        public readonly static RuleType FloatArray = new RuleType("guid", typeof(float[]));
        public readonly static RuleType StringArray = new RuleType("guid", typeof(string[]));

        private RuleType(string name, Type clrType)
        {
            Guard.NotEmpty(name, nameof(name));
            Guard.NotNull(clrType, nameof(clrType));

            Name = name;
            ClrType = clrType;

        }

        public string Name { get; set; }
        public Type ClrType { get; set; }

        public IEnumerable<RuleOperation> GetValidOperators()
        {
            bool isComparable = typeof(IComparable).IsAssignableFrom(ClrType);

            if (isComparable)
            {
                yield return RuleOperation.EqualTo;
                yield return RuleOperation.NotEqualTo;
            }

            if (ClrType == typeof(int) || ClrType == typeof(float) || ClrType == typeof(DateTime))
            {
                yield return RuleOperation.GreaterThanOrEqualTo;
                yield return RuleOperation.GreaterThan;
                yield return RuleOperation.LessThanOrEqualTo;
                yield return RuleOperation.LessThan;
                yield return RuleOperation.Between;
            }

            if (ClrType == typeof(string))
            {
                yield return RuleOperation.StartsWith;
                yield return RuleOperation.EndsWith;
                yield return RuleOperation.Contains;
                yield return RuleOperation.NotContains;
            }

            if (ClrType == typeof(int[]) || ClrType == typeof(float[]) || ClrType == typeof(string[]))
            {
                yield return RuleOperation.In;
                yield return RuleOperation.NotIn;
            }
        }
    }
}
