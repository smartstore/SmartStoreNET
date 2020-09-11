using System;
using System.Linq;
using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    public static class QueryableExtensions
    {
        public static IQueryable Where(this IQueryable source, FilterExpression filter)
        {
            Expression predicate = filter.ToPredicate(null, IsLinqToObjectsProvider(source.Provider));
            return source.Where(predicate);
        }

        public static IQueryable Where(this IQueryable source, Expression predicate)
        {
            var typeArgs = new Type[] { source.ElementType };
            var exprArgs = new Expression[] { source.Expression, Expression.Quote(predicate) };

            return source.Provider.CreateQuery(
                Expression.Call(typeof(Queryable), "Where", typeArgs, exprArgs));
        }

        internal static bool IsLinqToObjectsProvider(IQueryProvider provider)
        {
            return provider.GetType().FullName.Contains("EnumerableQuery");
        }
    }
}
