using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.DiscountRules.Settings;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Services.Orders;
using SmartStore.Services.Tax;

namespace SmartStore.DiscountRules
{
	[SystemName("DiscountRequirement.HadSpentAmount")]
	[FriendlyName("Customer has spent x amount")]
	[DisplayOrder(25)]
	public partial class HadSpentAmountRule : DiscountRequirementRuleBase
    {
		private readonly IOrderService _orderService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly ITaxService _taxService;
		private readonly ILogger _logger;

		public HadSpentAmountRule(
            IOrderService orderService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
			ILogger logger)
        {
			_orderService = orderService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
			_logger = logger;
        }

		public override bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
			if (request == null)
				throw new ArgumentNullException("request");

			if (request.DiscountRequirement == null)
				throw new SmartException("Discount requirement is not set");

			if (request.DiscountRequirement.SpentAmount == decimal.Zero)
				return true;

			if (request.Customer == null)
				return false;

			var settings = DeserializeSettings(request.DiscountRequirement.ExtraData);

			if (settings.LimitToCurrentBasketSubTotal)
			{
				return CheckCurrentSubTotalRequirement(request);
			}
			else
			{
				return CheckTotalHistoryRequirement(request);
			}
        }

		protected override string GetActionName()
		{
			return "HadSpentAmount";
		}

		private bool CheckTotalHistoryRequirement(CheckDiscountRequirementRequest request)
		{
			if (request.Customer.IsGuest())
				return false;

			var orders = _orderService.SearchOrders(
				request.Store.Id,
				request.Customer.Id,
				null,
				null,
				new int[] { (int)OrderStatus.Complete },
				null,
				null,
				null,
				null,
				null,
				0,
				int.MaxValue);

			decimal spentAmount = orders.Sum(o => o.OrderTotal);
			return spentAmount >= request.DiscountRequirement.SpentAmount;
		}

		private bool CheckCurrentSubTotalRequirement(CheckDiscountRequirementRequest request)
		{
			var spentAmount = decimal.Zero;

			try
			{
				var taxRate = decimal.Zero;
				var cartItems = request.Customer.GetCartItems(ShoppingCartType.ShoppingCart, request.Store.Id);

				foreach (var cartItem in cartItems)
				{
					var product = cartItem.Item.Product;
					Dictionary<string, object> mergedValuesClone = null;

					// we must reapply merged values because CheckCurrentSubTotalRequirement uses price calculation and is called by it itself.
					// this can cause wrong discount calculation if the cart contains a product several times.
					if (product.MergedDataValues != null)
						mergedValuesClone = new Dictionary<string, object>(product.MergedDataValues);

					// includeDiscounts == true produces a stack overflow!
					spentAmount += cartItem.Item.Quantity * _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(cartItem, false), out taxRate);

					if (mergedValuesClone != null)
						product.MergedDataValues = new Dictionary<string, object>(mergedValuesClone);
				}
			}
			catch (Exception exception)
			{
				_logger.Error(exception);
				return false;
			}

			return spentAmount >= request.DiscountRequirement.SpentAmount;
		}

		internal static HadSpentAmountSettings DeserializeSettings(string expression)
		{
			var settings = new HadSpentAmountSettings();
			if (expression.HasValue())
			{
				try
				{
					settings = JsonConvert.DeserializeObject<HadSpentAmountSettings>(expression);
				}
				catch { }
			}

			return settings;
		}
        
    }
}