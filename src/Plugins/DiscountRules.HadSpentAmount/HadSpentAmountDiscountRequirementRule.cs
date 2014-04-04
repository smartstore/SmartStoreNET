using System;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Tax;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount
{
    public partial class HadSpentAmountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
		private readonly ILocalizationService _localizationService;
		private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;

		public HadSpentAmountDiscountRequirementRule(
            ILocalizationService localizationService, 
            IOrderService orderService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService)
        {
            this._localizationService = localizationService;
			this._orderService = orderService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public bool CheckRequirement(CheckDiscountRequirementRequest request)
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
                return CheckCurrentSubTotalRequirement(request, settings.BasketSubTotalIncludesDiscounts);
            }
            else
            {
                return CheckTotalHistoryRequirement(request);
            }
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

        private bool CheckCurrentSubTotalRequirement(CheckDiscountRequirementRequest request, bool includingDiscount = true)
        {
            var cartItems = request.Customer.GetCartItems(ShoppingCartType.ShoppingCart, request.Store.Id);

            decimal spentAmount = decimal.Zero;
            decimal taxRate = decimal.Zero;
            foreach (var sci in cartItems) 
            {
                spentAmount += sci.Item.Quantity * _taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, includingDiscount), out taxRate);
            }       
            
            return spentAmount >= request.DiscountRequirement.SpentAmount;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesHadSpentAmount/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);
            base.Install();
        }

        public override void Uninstall()
        {
            // locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.HadSpentAmount", false);

            base.Uninstall();
        }

        internal static RequirementSettings DeserializeSettings(string expression)
        {
            var settings = new RequirementSettings();
            if (expression.HasValue())
            {
                try
                {
                    settings = JsonConvert.DeserializeObject<RequirementSettings>(expression);
                }
                catch { }
            }

            return settings;
        }
    }
}