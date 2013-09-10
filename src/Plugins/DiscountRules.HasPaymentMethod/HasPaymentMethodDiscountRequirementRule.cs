using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;

namespace SmartStore.Plugin.DiscountRules.HasPaymentMethod
{
    public partial class HasPaymentMethodDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
		private readonly ILocalizationService _localizationService;
		private readonly ISettingService _settingService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IStoreContext _storeContext;

		public HasPaymentMethodDiscountRequirementRule(
			ILocalizationService localizationService,
			ISettingService settingService,
			IGenericAttributeService genericAttributeService,
			IStoreContext storeContext)
        {
            _localizationService = localizationService;
			_settingService = settingService;
			_genericAttributeService = genericAttributeService;
			_storeContext = storeContext;
        }

		public static string GetSettingKey(int discountRequirementId)
		{
			return "DiscountRequirement.RestrictedPaymentMethods-{0}".FormatWith(discountRequirementId);
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

			if (request.Customer == null)
				return false;

			var settingPaymentMethods = _settingService.GetSettingByKey<string>(
				HasPaymentMethodDiscountRequirementRule.GetSettingKey(request.DiscountRequirement.Id));

			if (string.IsNullOrWhiteSpace(settingPaymentMethods))
				return false;

			var discountPaymentMethods = settingPaymentMethods.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

			var selectedPaymentMethod = request.Customer.GetAttribute<string>(
				SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, _storeContext.CurrentStore.Id);

			if (selectedPaymentMethod.IsNullOrEmpty() || discountPaymentMethods.Count <= 0)
				return false;

			return discountPaymentMethods.Exists(x => x.IsCaseInsensitiveEqual(selectedPaymentMethod));
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
			string result = "Plugins/DiscountRulesHasPaymentMethod/Configure/?discountId=" + discountId;
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
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.HasPaymentMethod", false);

            base.Uninstall();
        }
    }
}