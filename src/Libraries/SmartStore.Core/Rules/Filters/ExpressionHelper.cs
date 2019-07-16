using System;
using System.Linq.Expressions;
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
    }
}
