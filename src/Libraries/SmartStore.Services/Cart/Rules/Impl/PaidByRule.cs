using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PaidByRule : ListRuleBase<string>
    {
        private readonly IOrderService _orderService;

        public PaidByRule(IOrderService orderService)
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
            _orderService = orderService;
        }

        public override bool Match(CartRuleContext context, RuleExpression expression)
        {
            if (expression.Operator == RuleOperator.In || expression.Operator == RuleOperator.NotIn)
            {
                // Get result using LINQ to Entities.
                var paymentMethods = expression.Value as List<string>;
                if (!(paymentMethods?.Any() ?? false))
                {
                    return true;
                }

                var query = GetQuery(context);

                if (expression.Operator == RuleOperator.In)
                {
                    return query.Where(o => paymentMethods.Contains(o.PaymentMethodSystemName)).Any();
                }

                return query.Where(o => !paymentMethods.Contains(o.PaymentMethodSystemName)).Any();
            }

            // Get result using LINQ to Objects.
            return base.Match(context, expression);
        }

        protected virtual IQueryable<Order> GetQuery(CartRuleContext context)
        {
            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null, null);
            return query;
        }

        protected override IEnumerable<string> GetValues(CartRuleContext context)
        {
            // Fast batch loading of payment methods.
            var paymentMethods = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var take = 4000;
            var query = GetQuery(context);
            var maxId = query.Max(x => (int?)x.Id) ?? 0;

            for (var lastId = 0; lastId < maxId;)
            {
                var batchQuery = query
                    .OrderBy(x => x.Id)
                    .Select(x => new { x.Id, x.PaymentMethodSystemName });

                if (lastId > 0)
                {
                    batchQuery = batchQuery.Where(x => x.Id > lastId);
                }

                batchQuery = batchQuery.Take(() => take);
                var batch = batchQuery.ToList();

                paymentMethods.AddRange(batch.Select(x => x.PaymentMethodSystemName));
                lastId = batch.Last().Id;
            }

            return paymentMethods;
        }
    }
}
