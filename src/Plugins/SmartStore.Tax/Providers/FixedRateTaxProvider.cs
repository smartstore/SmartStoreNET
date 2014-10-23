using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Tax;

namespace SmartStore.Tax
{
	[SystemName("Tax.FixedRate")]
	[FriendlyName("Fixed Tax Rate")]
	[DisplayOrder(5)]
	public class FixedRateTaxProvider : ITaxProvider, IConfigurable
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        public FixedRateTaxProvider(ISettingService settingService, ILocalizationService localizationService)
        {
            this._settingService = settingService;
            _localizationService = localizationService;
        }
        
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult()
            {
                TaxRate = GetTaxRate(calculateTaxRequest.TaxCategoryId)
            };
            return result;
        }

        protected decimal GetTaxRate(int taxCategoryId)
        {
            decimal rate = this._settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }
        
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxFixedRate";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.Tax" } };
        }

    }
}
