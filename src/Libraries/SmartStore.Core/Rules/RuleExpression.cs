using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public static class RuleOperators
    {
        public const string GreaterThanOrEqual = ">=";
        public const string GreaterThan = ">";
        public const string LessThanOrEqual = "<=";
        public const string LessThan = "<";
        public const string Equal = "=";
        public const string NotEqual = "!=";
        public const string Between = "between"; // TBD: muss das sein? verkompliziert alles unnötig.
        public const string StartsWith = "startswith";
        public const string EndsWith = "endswith";
        public const string Contains = "contains";
        public const string NotContains = "notcontains";
        public const string Null = "null";
        public const string NotNull = "notnull";
        //public const string All = "all";
        public const string In = "in";
        public const string NotIn = "notin";

        public static string[] StringOperators { get; } = new[] { Equal, NotEqual, StartsWith, Contains, NotContains, EndsWith, Null, NotNull };
        public static string[] ArrayOperators { get; } = new[] { In/*, All*/, NotIn };
        public static string[] ComparableOperators { get; } = new[] { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Between, Null, NotNull };
    }

    public enum RuleTypeCode
    {
        Boolean,
        Int,
        Float,
        Guid,
        DateTime,
        String,
        IntArray,
        FloatArray,
        StringArray
    }

    public class RuleExpression
    {
        public RuleTypeCode TypeCode { get; set; }
        public string Operator { get; set; }
        public object Comparand { get; set; }
    }

    public class RangeRuleExpression : RuleExpression
    {
        public object UpperComparand { get; set; }
        //public bool IncludesLower { get; set; }
        //public bool IncludesUpper { get; set; }
    }
}
