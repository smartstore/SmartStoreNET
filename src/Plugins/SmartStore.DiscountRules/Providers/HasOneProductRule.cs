using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Customers;

namespace SmartStore.DiscountRules
{
	[SystemName("DiscountRequirement.HasOneProduct")]
	[FriendlyName("Customer has one of these products in the cart")]
	[DisplayOrder(15)]
	public partial class HasOneProductRule : DiscountRequirementRuleBase
    {
        public override bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
			if (request == null)
				throw new ArgumentNullException("request");

			if (request.DiscountRequirement == null)
				throw new SmartException("Discount requirement is not set");

			if (String.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedProductIds))
				return true;

			if (request.Customer == null)
				return false;

			//we support three ways of specifying products:
			//1. The comma-separated list of product identifiers (e.g. 77, 123, 156).
			//2. The comma-separated list of product identifiers with quantities.
			//      {Product ID}:{Quantity}. For example, 77:1, 123:2, 156:3
			//3. The comma-separated list of product identifiers with quantity range.
			//      {Product ID}:{Min quantity}-{Max quantity}. For example, 77:1-3, 123:2-5, 156:3-8
			var restrictedProducts = request.DiscountRequirement.RestrictedProductIds
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToList();
			if (restrictedProducts.Count == 0)
				return false;

			//cart
			var cart = request.Customer.GetCartItems(ShoppingCartType.ShoppingCart, request.Store.Id);

			bool found = false;
			foreach (var restrictedProduct in restrictedProducts)
			{
				if (String.IsNullOrWhiteSpace(restrictedProduct))
					continue;

				foreach (var sci in cart)
				{
					if (restrictedProduct.Contains(":"))
					{
						if (restrictedProduct.Contains("-"))
						{
							//the third way (the quantity rage specified)
							//{Product ID}:{Min quantity}-{Max quantity}. For example, 77:1-3, 123:2-5, 156:3-8
							int restrictedProductId = 0;
							if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[0], out restrictedProductId))
								//parsing error; exit;
								return false;
							int quantityMin = 0;
							if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[0], out quantityMin))
								//parsing error; exit;
								return false;
							int quantityMax = 0;
							if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[1], out quantityMax))
								//parsing error; exit;
								return false;

							if (sci.Item.ProductId == restrictedProductId && quantityMin <= sci.Item.Quantity && sci.Item.Quantity <= quantityMax)
							{
								found = true;
								break;
							}
						}
						else
						{
							//the second way (the quantity specified)
							//{Product ID}:{Quantity}. For example, 77:1, 123:2, 156:3
							int restrictedProductId = 0;
							if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[0], out restrictedProductId))
								//parsing error; exit;
								return false;
							int quantity = 0;
							if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1], out quantity))
								//parsing error; exit;
								return false;

							if (sci.Item.ProductId == restrictedProductId && sci.Item.Quantity == quantity)
							{
								found = true;
								break;
							}
						}
					}
					else
					{
						//the first way (the quantity is not specified)
						int restrictedProductId = int.Parse(restrictedProduct);
						if (sci.Item.ProductId == restrictedProductId)
						{
							found = true;
							break;
						}
					}
				}

				if (found)
				{
					break;
				}
			}

			if (found)
				return true;

			return false;
        }

		protected override string GetActionName()
		{
			return "HasOneProduct";
		}
        
    }
}