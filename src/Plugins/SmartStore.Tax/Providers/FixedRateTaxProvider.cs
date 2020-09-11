using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tax;

namespace SmartStore.Tax
{
    [SystemName("Tax.FixedRate")]
    [FriendlyName("Fixed Tax Rate")]
    [DisplayOrder(5)]
    public class FixedRateTaxProvider : ITaxProvider, IConfigurable
    {
        private readonly ISettingService _settingService;

        public FixedRateTaxProvider(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult
            {
                TaxRate = GetTaxRate(calculateTaxRequest.TaxCategoryId)
            };

            return result;
        }

        protected decimal GetTaxRate(int taxCategoryId)
        {
            var rate = _settingService.GetSettingByKey<decimal>($"Tax.TaxProvider.FixedRate.TaxCategoryId{taxCategoryId}");
            return rate;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxFixedRate";
            routeValues = new RouteValueDictionary { { "area", "SmartStore.Tax" } };
        }
    }
}
