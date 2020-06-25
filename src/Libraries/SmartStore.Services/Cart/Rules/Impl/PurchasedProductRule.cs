using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedProductRule : ListRuleBase<int>
    {
        protected readonly IOrderService _orderService;

        public PurchasedProductRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public override bool Match(CartRuleContext context, RuleExpression expression)
        {
            if (expression.Operator == RuleOperator.In || expression.Operator == RuleOperator.NotIn)
            {
                // Get result using LINQ to Entities.
                var productIds = expression.Value as List<int>;
                if (!(productIds?.Any() ?? false))
                {
                    return true;
                }

                var query = GetQuery(context);

                if (expression.Operator == RuleOperator.In)
                {
                    return query.Where(oi => productIds.Contains(oi.ProductId)).Any();
                }

                return query.Where(oi => !productIds.Contains(oi.ProductId)).Any();
            }

            // Get result using LINQ to Objects.
            return base.Match(context, expression);
        }

        protected virtual IQueryable<OrderItem> GetQuery(CartRuleContext context)
        {
            var query = _orderService.GetOrders(context.Store.Id, context.Customer.Id, null, null, null, null, null, null, null, null, null)
                .SelectMany(o => o.OrderItems);

            return query;
        }

        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            // Fast batch loading of product IDs.
            var productIds = new HashSet<int>();
            var take = 4000;
            var query = GetQuery(context);
            var maxId = query.Max(x => (int?)x.Id) ?? 0;

            for (var lastId = 0; lastId < maxId;)
            {
                var batchQuery = query
                    .OrderBy(x => x.Id)
                    .Select(x => new { x.Id, x.ProductId });

                if (lastId > 0)
                {
                    batchQuery = batchQuery.Where(x => x.Id > lastId);
                }

                batchQuery = batchQuery.Take(() => take);
                var batch = batchQuery.ToList();

                productIds.AddRange(batch.Select(x => x.ProductId));
                lastId = batch.Last().Id;
            }

            return productIds;
        }
    }
}
