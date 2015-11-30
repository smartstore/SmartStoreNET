using System;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Services.Customers;
using SmartStore.Services.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Tax;
using SmartStore.Core.Localization;
using SmartStore.DiscountRules.Settings;
using Newtonsoft.Json;

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

		public HadSpentAmountRule(
            IOrderService orderService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService)
        {
			this._orderService = orderService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
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
			var cartItems = request.Customer.GetCartItems(ShoppingCartType.ShoppingCart, request.Store.Id);

			decimal spentAmount = decimal.Zero;
			decimal taxRate = decimal.Zero;
			foreach (var sci in cartItems)
			{
				// includeDiscounts == true produces a stack overflow!
				spentAmount += sci.Item.Quantity * _taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, false), out taxRate);
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