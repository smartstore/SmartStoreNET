using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services.Tax;

namespace SmartStore.Plugin.Tax.Free
{
    /// <summary>
    /// Free tax provider
    /// </summary>
    public class FreeTaxProvider : BasePlugin, ITaxProvider
    {

        
        private readonly ILocalizationService _localizationService;

        public FreeTaxProvider(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult()
            {
                 TaxRate = decimal.Zero
            };
            return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }

        public override void Install()
        {
            //locales
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Tax.FixedRate.Fields.TaxCategoryName");
            this.DeletePluginLocaleResource("Plugins.Tax.FixedRate.Fields.Rate");

            base.Uninstall();
        }
    }
}
