using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tax;
using SmartStore.Tax.Services;

namespace SmartStore.Tax
{
    [SystemName("Tax.CountryStateZip")]
    [FriendlyName("Tax By Region")]
    [DisplayOrder(10)]
    public class ByRegionTaxProvider : ITaxProvider, IConfigurable
    {
        private readonly ITaxRateService _taxRateService;
        private readonly ISettingService _settingService;
        private readonly TaxSettings _taxSettings;

        public ByRegionTaxProvider(
            ITaxRateService taxRateService,
            ISettingService settingService,
            TaxSettings taxSettings)
        {
            _taxRateService = taxRateService;
            _settingService = settingService;
            _taxSettings = taxSettings;
        }

        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult();

            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            if (_taxSettings.EuVatEnabled)
            {
                if (!(calculateTaxRequest.Address.Country?.SubjectToVat ?? false))
                {
                    // Fallback to fixed rate (merchant country VAT rate).
                    result.TaxRate = _settingService.GetSettingByKey<decimal>($"Tax.TaxProvider.FixedRate.TaxCategoryId{calculateTaxRequest.TaxCategoryId}");
                    return result;
                }
            }

            var taxRates = _taxRateService.GetAllTaxRates(
                calculateTaxRequest.TaxCategoryId,
                calculateTaxRequest.Address.Country?.Id ?? 0,
                calculateTaxRequest.Address.StateProvince?.Id ?? 0,
                calculateTaxRequest.Address.ZipPostalCode);

            if (taxRates.Any())
            {
                result.TaxRate = taxRates[0].Percentage;
            }

            return result;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxByRegion";
            routeValues = new RouteValueDictionary { { "area", "SmartStore.Tax" } };
        }
    }
}
