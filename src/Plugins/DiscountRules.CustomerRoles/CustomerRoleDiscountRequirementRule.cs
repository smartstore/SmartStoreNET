using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;


namespace SmartStore.Plugin.DiscountRules.CustomerRoles
{
    public partial class CustomerRoleDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        /// 

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public CustomerRoleDiscountRequirementRule(ILocalizationService _localizationService, ISettingService _settingService)
        {
            
            this._localizationService = _localizationService;
            this._settingService = _settingService;
        }

        public bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.DiscountRequirement == null)
                throw new SmartException("Discount requirement is not set");

            if (request.Customer == null)
                return false;

            if (!request.DiscountRequirement.RestrictedToCustomerRoleId.HasValue)
                return false;

            foreach (var customerRole in request.Customer.CustomerRoles.Where(cr => cr.Active).ToList())
                if (request.DiscountRequirement.RestrictedToCustomerRoleId == customerRole.Id)
                    return true;

            return false;
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
            string result = "Plugins/DiscountRulesCustomerRoles/Configure/?discountId=" + discountId;
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
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.MustBeAssignedToCustomerRole", false);
            _settingService.DeleteSettings("DiscountRequirement.MustBeAssignedToCustomerRole");

            base.Uninstall();
        }
    }
}