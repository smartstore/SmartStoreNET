using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount
{
    public partial class HadSpentAmountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
		private readonly ILocalizationService _localizationService;
		private readonly IOrderService _orderService;
		private readonly IWorkContext _workContext;

		public HadSpentAmountDiscountRequirementRule(ILocalizationService localizationService, IOrderService orderService,
			IWorkContext workContext)
        {
            _localizationService = localizationService;
			_orderService = orderService;
			_workContext = workContext;
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

            if (request.Customer == null || request.Customer.IsGuest())
                return false;

			var orders = _orderService.SearchOrders(_workContext.CurrentStore.Id, request.Customer.Id,
				null, null, OrderStatus.Complete, null, null, null, null, 0, int.MaxValue);
            decimal spentAmount = orders.Sum(o => o.OrderTotal);
            return spentAmount > request.DiscountRequirement.SpentAmount;
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
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.HadSpentAmount", false);

            base.Uninstall();
        }
    }
}