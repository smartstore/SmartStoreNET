using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PurchasedFromManufacturerRule : ListRuleBase<int>
    {
        private readonly IOrderService _orderService;

        public PurchasedFromManufacturerRule(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public override bool Match(CartRuleContext context, RuleExpression expression)
        {
            if (expression.Operator == RuleOperator.In || expression.Operator == RuleOperator.NotIn)
            {
                // Get result using LINQ to Entities.
                var manuIds = expression.Value as List<int>;
                if (!(manuIds?.Any() ?? false))
                {
                    return true;
                }

                var query = GetQuery(context);

                if (expression.Operator == RuleOperator.In)
                {
                    return query.Where(oi => oi.Product.ProductManufacturers.Any(pm => manuIds.Contains(pm.ManufacturerId))).Any();
                }

                return query.Where(oi => oi.Product.ProductManufacturers.Any(pm => !manuIds.Contains(pm.ManufacturerId))).Any();
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
            // Fast batch loading of manufacturer IDs.
            var manuIds = new HashSet<int>();
            var take = 4000;
            var query = GetQuery(context);
            var maxId = query.Max(x => (int?)x.Id) ?? 0;

            for (var lastId = 0; lastId < maxId;)
            {
                var batchQuery = query
                    .OrderBy(x => x.Id)
                    .Select(x => new { x.Id, ManufacturerIds = x.Product.ProductManufacturers.Select(pm => pm.ManufacturerId) });

                if (lastId > 0)
                {
                    batchQuery = batchQuery.Where(x => x.Id > lastId);
                }

                batchQuery = batchQuery.Take(() => take);
                var batch = batchQuery.ToList();

                manuIds.AddRange(batch.SelectMany(x => x.ManufacturerIds));
                lastId = batch.Last().Id;
            }

            return manuIds;
        }
    }
}
