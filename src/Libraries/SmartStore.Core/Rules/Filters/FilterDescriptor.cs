using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Rules.Filters
{
    public class FilterDescriptor : RuleDescriptor
    {
        public FilterDescriptor(LambdaExpression memberExpression)
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

        public new Expression<Func<T, TValue>> MemberExpression { get; private set; }
    }

    public class AnyFilterDescriptor<T, TAny, TAnyValue> : FilterDescriptor<T, IEnumerable<TAny>> 
        where T : class 
        where TAny : class
    {
        public AnyFilterDescriptor(
            Expression<Func<T, IEnumerable<TAny>>> path, 
            Expression<Func<TAny, TAnyValue>> anyPredicate)
            : base(path)
        {
            AnyExpression = anyPredicate;
        }

        public Expression<Func<TAny, TAnyValue>> AnyExpression { get; private set; }

        public override Expression GetExpression(RuleOperator op, Expression valueExpression, bool liftToNull)
        {
            // Create the .Any() lambda predicate (the part within parentheses)
            var anyPredicate = ExpressionHelper.CreateLambdaExpression(
                Expression.Parameter(typeof(TAny), "it2"),
                op.GetExpression(AnyExpression.Body, valueExpression, liftToNull));

            var body = Expression.Call(
                typeof(Enumerable),
                "Any",
                // .Any<TAny>()
                new[] { typeof(TAny) },
                // 0 = left collection path: x.Orders.selectMany(o => o.OrderItems)
                // 1 = right Any predicate: y => y.ProductId = 1
                new Expression[]
                {
                    MemberExpression.Body,
                    anyPredicate
                });

            return body;
        }
    }
}
