using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Rules;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ProductReviewCountRule : IRule
    {
        private readonly ICustomerContentService _customerContentService;

        public ProductReviewCountRule(ICustomerContentService customerContentService)
        {
            _customerContentService = customerContentService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var query = _customerContentService.GetAllCustomerContent<ProductReview>(context.Customer.Id, true).SourceQuery;
            var reviewsCount = query.Count();

            return expression.Operator.Match(reviewsCount, expression.Value);
        }
    }
}
