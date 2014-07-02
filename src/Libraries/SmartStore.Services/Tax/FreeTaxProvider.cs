using SmartStore;
using System.Web.Routing;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Tax
{
	[SystemName("Tax.Free")]
	[FriendlyName("Free tax rate provider")]
	[DisplayOrder(0)]
    public class FreeTaxProvider : ITaxProvider
    {
        
		public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult()
            {
                 TaxRate = decimal.Zero
            };
            return result;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }

    }
}
