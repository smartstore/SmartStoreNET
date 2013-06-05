using System;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.DiscountRules.Store
{
    public partial class StoreDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
		private readonly ILocalizationService _localizationService;
		private readonly ISettingService _settingService;
		private readonly IWorkContext _workContext;

		public StoreDiscountRequirementRule(ILocalizationService localizationService, ISettingService settingService, IWorkContext workContext)
		{
			this._localizationService = _localizationService;
			this._settingService = settingService;
			this._workContext = workContext;
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

			var storeId = _settingService.GetSettingByKey<int>(string.Format("DiscountRequirement.Store-{0}", request.DiscountRequirement.Id));

			if (storeId == 0)
				return false;

			bool result = _workContext.CurrentStore.Id == storeId;
			return result;
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
			string result = "Plugins/DiscountRulesStore/Configure/?discountId=" + discountId;
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
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.DiscountRequirement.Store", false);
            base.Uninstall();
        }
    }
}