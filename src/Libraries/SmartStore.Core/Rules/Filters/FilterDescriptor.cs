using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            //var d = new FilterDescriptor<Customer>(x => x.Orders.Average(y => y.OrderTotal));
            //var e = new FilterDescriptor<Customer>(x => x.Orders.SelectMany(y => y.OrderItems).Where(z => z.Any(a => a.ProductId == 1)));
        }

        public LambdaExpression MemberExpression { get; private set; }

        public virtual Expression GetExpression(RuleOperator op, Expression valueExpression)
        {
            return op.GenerateExpression(MemberExpression.Body, valueExpression);
        }

        //public static FilterDescriptor<T, TValue> Create<T, TValue>(Expression<Func<T, TValue>> expression) where T : class
        //{
        //    return new FilterDescriptor<T, TValue>(expression);
        //}
    }

    public class FilterDescriptor<T, TValue> : FilterDescriptor where T : class 
    {
        public FilterDescriptor(Expression<Func<T, TValue>> expression) : base(expression) // TODO
        {
            Guard.NotNull(expression, nameof(expression));

            MemberExpression = expression;
        }

        public new Expression<Func<T, TValue>> MemberExpression { get; private set; }

        //protected virtual Expression<Func<T, bool>> GenerateMemberExpression(RuleOperator op, Expression valueExpression)
        //{
        //    var expression = op.GenerateExpression(MemberExpression, valueExpression);
        //    return Expression.Lambda<Func<T, bool>>(expression);
        //}
    }

    //public class FilterDescriptor<T, TPredicate>
    //{
    //    private readonly Expression<Func<TPredicate, object>> _member;
    //    private readonly Func<Expression<Func<TPredicate, object>>, Expression<Func<T, bool>>> _expression;

    //    public FilterDescriptor(
    //        Expression<Func<TPredicate, object>> member, 
    //        Func<Expression<Func<TPredicate, object>>, Expression<Func<T, bool>>> expression)
    //    {
    //        //MemberExpression = expression;
    //        _member = member;
    //        _expression = expression;

    //        Expression<Func<Customer, bool>> expr = x => x.

    //        var d = new FilterDescriptor<Customer, OrderItem>(
    //            oi => oi.ProductId,
    //            a => a);
    //    }

    //    public Expression<Func<T, bool>> Expression { get; private set; }

    //    //protected virtual Expression<Func<T, bool>> GenerateMemberExpression(RuleOperator op, Expression valueExpression)
    //    //{
    //    //    var expression = op.GenerateExpression(MemberExpression, valueExpression);
    //    //    return Expression.Lambda<Func<T, bool>>(expression);
    //    //}
    //}

    //internal class HasBoughtProductFilter : FilterDescriptor<Customer>
    //{
    //    public override Expression GetExpression(RuleOperator op, Expression valueExpression)
    //    {
    //        Expression<Func<Customer, bool>> path = c => c.Orders.SelectMany(o => o.OrderItems).Any(oi => (new int[] { 100, 102, 340, 489, 549, 698 }).Contains(oi.ProductId));

    //        return base.GetExpression(op, valueExpression);
    //    }
    //}
}
