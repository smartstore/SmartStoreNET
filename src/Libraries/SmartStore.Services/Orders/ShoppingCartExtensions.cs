using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Represents a shopping cart
    /// </summary>
    public static class ShoppingCartExtensions
    {
        /// <summary>
        /// Indicates whether the shopping cart requires shipping
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>True if the shopping cart requires shipping; otherwise, false.</returns>
        public static bool RequiresShipping(this IList<OrganizedShoppingCartItem> shoppingCart)
        {
            foreach (var shoppingCartItem in shoppingCart)
                if (shoppingCartItem.Item.IsShipEnabled)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a number of product in the cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Result</returns>
		public static int GetTotalProducts(this IEnumerable<OrganizedShoppingCartItem> shoppingCart)
        {
            return shoppingCart.Sum(x => x.Item.Quantity);
        }

        /// <summary>
        /// Gets a value indicating whether shopping cart is recurring
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Result</returns>
		public static bool IsRecurring(this IList<OrganizedShoppingCartItem> shoppingCart)
        {
            foreach (var sci in shoppingCart)
            {
                var product = sci.Item.Product;
                if (product != null && product.IsRecurring)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get a recurring cycle information
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
		/// <param name="localizationService">Localization service</param>
        /// <param name="cycleLength">Cycle length</param>
        /// <param name="cyclePeriod">Cycle period</param>
        /// <param name="totalCycles">Total cycles</param>
        /// <returns>Error (if exists); otherwise, empty string</returns>
		public static string GetRecurringCycleInfo(this IList<OrganizedShoppingCartItem> shoppingCart, ILocalizationService localizationService,
            out int cycleLength, out RecurringProductCyclePeriod cyclePeriod, out int totalCycles)
        {
            string error = "";

            cycleLength = 0;
            cyclePeriod = 0;
            totalCycles = 0;

            int? _cycleLength = null;
            RecurringProductCyclePeriod? _cyclePeriod = null;
            int? _totalCycles = null;

            foreach (var sci in shoppingCart)
            {
                var product = sci.Item.Product;
                if (product == null)
                {
                    throw new SmartException(string.Format("Product (Id={0}) cannot be loaded", sci.Item.ProductId));
                }

				string conflictError = localizationService.GetResource("ShoppingCart.ConflictingShipmentSchedules");
                if (product.IsRecurring)
                {
                    //cycle length
                    if (_cycleLength.HasValue && _cycleLength.Value != product.RecurringCycleLength)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _cycleLength = product.RecurringCycleLength;
                    }

                    //cycle period
                    if (_cyclePeriod.HasValue && _cyclePeriod.Value != product.RecurringCyclePeriod)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _cyclePeriod = product.RecurringCyclePeriod;
                    }

                    //total cycles
                    if (_totalCycles.HasValue && _totalCycles.Value != product.RecurringTotalCycles)
                    {
                        error = conflictError;
                        return error;
                    }
                    else
                    {
                        _totalCycles = product.RecurringTotalCycles;
                    }
                }
            }

			if (_cycleLength.HasValue && _cyclePeriod.HasValue && _totalCycles.HasValue)
            {
                cycleLength = _cycleLength.Value;
                cyclePeriod = _cyclePeriod.Value;
                totalCycles = _totalCycles.Value;
            }

            return error;
        }

        /// <summary>
        /// Get customer of shopping cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Customer of shopping cart</returns>
        public static Customer GetCustomer(this IList<OrganizedShoppingCartItem> shoppingCart)
        {
            if (shoppingCart.Count == 0)
                return null;

            return shoppingCart[0].Item.Customer;
        }

		public static IEnumerable<ShoppingCartItem> Filter(this IEnumerable<ShoppingCartItem> shoppingCart, ShoppingCartType type, int? storeId = null)
		{
			var enumerable = shoppingCart.Where(x => x.ShoppingCartType == type);

			if (storeId.HasValue)
				enumerable = enumerable.Where(x => x.StoreId == storeId.Value);

			return enumerable;
		}
		public static IList<OrganizedShoppingCartItem> Organize(this IList<ShoppingCartItem> cart)
		{
			var result = new List<OrganizedShoppingCartItem>();
			var productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();

			if (cart == null || cart.Count <= 0)
				return result;

			foreach (var parent in cart.Where(x => x.ParentItemId == null))
			{
				var parentItem = new OrganizedShoppingCartItem(parent);

				var childs = cart.Where(x => x.ParentItemId != null && x.ParentItemId == parent.Id && x.Id != parent.Id && 
					x.ShoppingCartTypeId == parent.ShoppingCartTypeId && x.Product.CanBeBundleItem());

				foreach (var child in childs)
				{
					var childItem = new OrganizedShoppingCartItem(child);

					if (parent.Product != null && parent.Product.BundlePerItemPricing && child.AttributesXml != null && child.BundleItem != null)
					{
						child.Product.MergeWithCombination(child.AttributesXml);

						var attributeValues = productAttributeParser.ParseProductVariantAttributeValues(child.AttributesXml).ToList();
						if (attributeValues != null)
						{
							childItem.BundleItemData.AdditionalCharge = decimal.Zero;
							attributeValues.Each(x => childItem.BundleItemData.AdditionalCharge += x.PriceAdjustment);
						}
					}

					parentItem.ChildItems.Add(childItem);
				}

				result.Add(parentItem);
			}

			return result;
		}
    }
}
