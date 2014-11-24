using System;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;

namespace SmartStore.DiscountRules
{

	public partial class Plugin : BasePlugin
    {
        private readonly ILocalizationService _localizationService;

        public Plugin(ILocalizationService _localizationService)
        {
            this._localizationService = _localizationService;
        }

        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);
            
            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);

			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.BillingCountry");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.ShippingCountry");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.Store");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.HasOneProduct");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.HasAllProducts");
			_localizationService.DeleteLocaleStringResources("Plugins.DiscountRules.HadSpentAmount");

            base.Uninstall();
        }
    }
}