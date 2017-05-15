using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SmartStore.Linq
{

    public static class LinqContainsPredicateBuilder
    {

        /// <summary>
        /// Builds a LINQ contains predicate for each item in the passed collection.
        /// </summary>
        /// <param name="collection">The collection, which holds the values.</param>
        /// <param name="targetProperty">The target property within the entity.</param>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <returns>The LINQ expression.</returns>
        public static Expression<Func<T, bool>> Build<T, TValue>(ICollection<TValue> collection, string targetProperty)
        {
            Guard.NotEmpty(collection, nameof(collection));
            Guard.NotEmpty(targetProperty, nameof(targetProperty));

            Expression completeExpression = null;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "t");

            foreach (TValue item in collection)
            {
                Expression nextExpression = Expression.Equal
                  (
                  Expression.Property(parameterExpression, targetProperty),
                  Expression.Constant(item)
                  );

                completeExpression = (completeExpression != null)
                  ? Expression.OrElse(completeExpression, nextExpression)
                  : nextExpression;
            }

            return (Expression<Func<T, bool>>)Expression.Lambda(completeExpression, parameterExpression);
        }

    }

}
