using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Tax.Data;
using SmartStore.Tax.Data.Migrations;
using SmartStore.Tax.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Tax;

namespace SmartStore.Tax
{
	[SystemName("Tax.CountryStateZip")]
	[FriendlyName("Tax By Region")]
	[DisplayOrder(10)]
	public class ByRegionTaxProvider : ITaxProvider, IConfigurable
    {
        private readonly ITaxRateService _taxRateService;
        private readonly TaxRateObjectContext _objectContext;
        private readonly ILocalizationService _localizationService;

		public ByRegionTaxProvider(ITaxRateService taxRateService,
            TaxRateObjectContext objectContext,
            ILocalizationService localizationService)
        {
            this._taxRateService = taxRateService;
            this._objectContext = objectContext;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult();

            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            var taxRates = _taxRateService.GetAllTaxRates(calculateTaxRequest.TaxCategoryId,
                calculateTaxRequest.Address.Country != null ? calculateTaxRequest.Address.Country.Id: 0,
                calculateTaxRequest.Address.StateProvince != null ? calculateTaxRequest.Address.StateProvince.Id : 0, 
                calculateTaxRequest.Address.ZipPostalCode);
            if (taxRates.Count > 0)
                result.TaxRate = taxRates[0].Percentage;

            return result;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TaxByRegion";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.Tax" } };
        }

    }
}
