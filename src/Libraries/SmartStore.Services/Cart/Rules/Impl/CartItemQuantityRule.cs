using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartItemQuantityRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;

        public CartItemQuantityRule(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            int productId = 0;
            int? minQuantity = null;
            int? maxQuantity = null;

            try
            {
                var rawValue = expression.Value as string;
                if (rawValue.HasValue())
                {
                    dynamic json = JObject.Parse(rawValue);
                    productId = ((string)json.ProductId).ToInt();

                    var str = (string)json.MinQuantity;
                    if (str.HasValue())
                    {
                        minQuantity = str.ToInt();
                    }

                    str = (string)json.MaxQuantity;
                    if (str.HasValue())
                    {
                        maxQuantity = str.ToInt();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (productId == 0)
            {
                return false;
            }

            var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);
            var items = cart.Where(x => x.Item.ProductId == productId);
            if (!items.Any())
            {
                return false;
            }

            var quantity = items.Sum(x => x.Item.Quantity);
            if (quantity == 0)
            {
                return false;
            }
            
            if (minQuantity.HasValue && maxQuantity.HasValue)
            {
                if (minQuantity == maxQuantity)
                {
                    return quantity == minQuantity;
                }
                else
                {
                    return quantity >= minQuantity && quantity <= maxQuantity;
                }
            }
            else if (minQuantity.HasValue)
            {
                return quantity >= minQuantity;
            }
            else if (maxQuantity.HasValue)
            {
                return quantity <= maxQuantity;
            }

            return false;
        }
    }
}
