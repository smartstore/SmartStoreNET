using System;
using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Shipping.ByWeight.Data;
using SmartStore.Plugin.Shipping.ByWeight.Data.Migrations;
using SmartStore.Plugin.Shipping.ByWeight.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;

namespace SmartStore.Plugin.Shipping.ByWeight
{
    public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
		private readonly IStoreContext _storeContext;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly ShippingByWeightObjectContext _objectContext;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
			IStoreContext storeContext,
            IShippingByWeightService shippingByWeightService,
            IPriceCalculationService priceCalculationService, 
            ShippingByWeightSettings shippingByWeightSettings,
            ShippingByWeightObjectContext objectContext,
            ILocalizationService localizationService)
        {
            this._shippingService = shippingService;
			this._storeContext = storeContext;
            this._shippingByWeightService = shippingByWeightService;
            this._priceCalculationService = priceCalculationService;
            this._shippingByWeightSettings = shippingByWeightSettings;
            this._objectContext = objectContext;
            _localizationService = localizationService;
        }
        #endregion

        #region Utilities

		private decimal? GetRate(decimal subTotal, decimal weight, int shippingMethodId, int storeId, int countryId)
        {
            decimal? shippingTotal = null;

			var shippingByWeightRecord = _shippingByWeightService.FindRecord(shippingMethodId, storeId, countryId, weight);
            if (shippingByWeightRecord == null)
            {
                if (_shippingByWeightSettings.LimitMethodsToCreated)
                    return null;
                else
                    return decimal.Zero;
            }

            if (shippingByWeightRecord.UsePercentage && shippingByWeightRecord.ShippingChargePercentage <= decimal.Zero)
            {
                return decimal.Zero;
            }
            if (!shippingByWeightRecord.UsePercentage && shippingByWeightRecord.ShippingChargeAmount <= decimal.Zero)
            {
                return decimal.Zero;
            }

            if (shippingByWeightRecord.UsePercentage)
            {
                shippingTotal = Math.Round((decimal)((((float)subTotal) * ((float)shippingByWeightRecord.ShippingChargePercentage)) / 100f), 2);
            }
            else
            {
                if (_shippingByWeightSettings.CalculatePerWeightUnit)
                {
                    shippingTotal = shippingByWeightRecord.ShippingChargeAmount * weight;
                }
                else
                {
                    shippingTotal = shippingByWeightRecord.ShippingChargeAmount;
                }
            }

            if (shippingTotal < decimal.Zero)
            {
                shippingTotal = decimal.Zero;
            }

            return shippingTotal;
        }
        
        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

			var storeId = _storeContext.CurrentStore.Id;
            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;
            
            decimal subTotal = decimal.Zero;
            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.Item.IsFreeShipping || !shoppingCartItem.Item.IsShipEnabled)
                    continue;
                subTotal += _priceCalculationService.GetSubTotal(shoppingCartItem, true);
            }
            decimal weight = _shippingService.GetShoppingCartTotalWeight(getShippingOptionRequest.Items);

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, weight, shippingMethod.Id, storeId, countryId);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                    shippingOption.Rate = rate.Value;
                    response.ShippingOptions.Add(shippingOption);
                }
            }


            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingByWeight";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Shipping.ByWeight.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
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
			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Shipping.ByWeight", false);
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }


        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
                return null; 
            }
        }

        #endregion
    }
}
