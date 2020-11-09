using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SmartStore.Rules.Filters
{
    public class FilterDescriptor : RuleDescriptor
    {
        public FilterDescriptor(LambdaExpression memberExpression)
            : base(RuleScope.Customer)
        {
            Guard.NotNull(memberExpression, nameof(memberExpression));
            MemberExpression = memberExpression;
        }

        public LambdaExpression MemberExpression { get; private set; }

        public virtual Expression GetExpression(RuleOperator op, Expression valueExpression, bool liftToNull)
        {
            return op.GetExpression(MemberExpression.Body, valueExpression, liftToNull);
        }
    }

    public class FilterDescriptor<T, TValue> : FilterDescriptor where T : class
    {
        public FilterDescriptor(Expression<Func<T, TValue>> expression) : base(expression) // TODO
        {
            Guard.NotNull(expression, nameof(expression));

            MemberExpression = expression;
        }

        public new Expression<Func<T, TValue>> MemberExpression { get; protected set; }
    }

    public abstract class PredicateFilterDescriptor<T, TPredicate, TPredicateValue> : FilterDescriptor<T, IEnumerable<TPredicate>>
        where T : class
        where TPredicate : class
    {
        protected PredicateFilterDescriptor(
            string methodName,
            Expression<Func<T, IEnumerable<TPredicate>>> path,
            Expression<Func<TPredicate, TPredicateValue>> predicate)
            : base(path)
        {
            MethodName = methodName;
            PredicateExpression = predicate;
        }

        protected string MethodName { get; set; }
        public Expression<Func<TPredicate, TPredicateValue>> PredicateExpression { get; private set; }

        public override Expression GetExpression(RuleOperator op, Expression valueExpression, bool liftToNull)
        {
            // Create the Any()/All() lambda predicate (the part within parentheses)
            var predicate = ExpressionHelper.CreateLambdaExpression(
                Expression.Parameter(typeof(TPredicate), "it2"),
                op.GetExpression(PredicateExpression.Body, valueExpression, liftToNull));

            var body = Expression.Call(
                typeof(Enumerable),
                MethodName,
                // .Any/All<TPredicate>()
                new[] { typeof(TPredicate) },
                // 0 = left collection path: x.Orders.selectMany(o => o.OrderItems)
                // 1 = right Any/All predicate: y => y.ProductId = 1
                new Expression[]
                {
                    MemberExpression.Body,
                    predicate
                });

            return body;
        }
    }

    public class AnyFilterDescriptor<T, TAny, TAnyValue> : PredicateFilterDescriptor<T, TAny, TAnyValue>
        where T : class
        where TAny : class
    {
        public AnyFilterDescriptor(
            Expression<Func<T, IEnumerable<TAny>>> path,
            Expression<Func<TAny, TAnyValue>> anyPredicate)
            : base("Any", path, anyPredicate)
        {
        }
    }

    public class AllFilterDescriptor<T, TAll, TAllValue> : PredicateFilterDescriptor<T, TAll, TAllValue>
        where T : class
        where TAll : class
    {
        public AllFilterDescriptor(
            Expression<Func<T, IEnumerable<TAll>>> path,
            Expression<Func<TAll, TAllValue>> allPredicate)
            : base("All", path, allPredicate)
        {
        }
    }
}