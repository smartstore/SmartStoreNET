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
            var reviews = _customerContentService.GetAllCustomerContent<ProductReview>(context.Customer.Id, true);

            return expression.Operator.Match(reviews.Count, expression.Value);
        }
    }
}
