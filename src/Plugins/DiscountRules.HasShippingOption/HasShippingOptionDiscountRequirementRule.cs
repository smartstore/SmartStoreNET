using System;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.DiscountRules.HasShippingOption
{
    public partial class HasShippingOptionDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
		private readonly ILocalizationService _localizationService;
		private readonly IGenericAttributeService _genericAttributeService;

		public HasShippingOptionDiscountRequirementRule(
			ILocalizationService localizationService,
			IGenericAttributeService genericAttributeService)
        {
            _localizationService = localizationService;
			_genericAttributeService = genericAttributeService;
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

			if (request.Customer == null)
				return false;

            if (request.DiscountRequirement == null)
                throw new SmartException("Discount requirement is not set");

			if (string.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedShippingOptions))
				return false;

			var discountShippingOptions = request.DiscountRequirement.RestrictedShippingOptions
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

			var selectedShippingOption = request.Customer.GetAttribute<ShippingOption>(
				SystemCustomerAttributeNames.SelectedShippingOption, _genericAttributeService, request.Store.Id);

			if (selectedShippingOption == null || selectedShippingOption.Name.IsNullOrEmpty() || discountShippingOptions.Count <= 0)
				return false;

			return discountShippingOptions.Exists(x => x.IsCaseInsensitiveEqual(selectedShippingOption.Name));
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
			string result = "Plugins/DiscountRulesHasShippingOption/Configure/?discountId=" + discountId;
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
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.HasShippingOption", false);

            base.Uninstall();
        }
    }
}