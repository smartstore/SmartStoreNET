using System;
using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Shipping.ByTotal.Data;
using SmartStore.Plugin.Shipping.ByTotal.Data.Migrations;
using SmartStore.Plugin.Shipping.ByTotal.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;

namespace SmartStore.Plugin.Shipping.ByTotal
{
    public class ByTotalShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
		private readonly IStoreContext _storeContext;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly ShippingByTotalObjectContext _objectContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shippingByTotalService">ShippingByTotal service</param>
        /// <param name="shippingByTotalSettings">ShippingByTotal settings</param>
        /// <param name="objectContext">ShippingByTotal object context</param>
        /// <param name="priceCalculationService">PriceCalculation service</param>
        /// <param name="settingService">Settings service</param>
        /// <param name="logger">Logger</param>
        public ByTotalShippingComputationMethod(IShippingService shippingService,
			IStoreContext storeContext,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings,
            ShippingByTotalObjectContext objectContext,
            IPriceCalculationService priceCalculationService,
            ILogger logger,
            ISettingService settingService,
            ILocalizationService localizationService)
        {
            this._shippingService = shippingService;
			this._storeContext = storeContext;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._objectContext = objectContext;
            this._priceCalculationService = priceCalculationService;
            this._logger = logger;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets the rate for the shipping method
        /// </summary>
        /// <param name="subtotal">the order's subtotal</param>
        /// <param name="shippingMethodId">the shipping method identifier</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="stateProvinceId">state province identifier</param>
        /// <param name="zip">Zip code</param>
        /// <returns>the rate for the shipping method</returns>
		private decimal? GetRate(decimal subtotal, int shippingMethodId, int storeId, int countryId, int stateProvinceId, string zip)
        {
            decimal? shippingTotal = null;

            var shippingByTotalRecord = _shippingByTotalService.FindShippingByTotalRecord(shippingMethodId, storeId, countryId, subtotal, stateProvinceId, zip);

            if (shippingByTotalRecord == null)
            {
                if (_shippingByTotalSettings.LimitMethodsToCreated)
                {
                    return null;
                }
                else
                {
                    return decimal.Zero;
                }
            }

            decimal baseCharge = shippingByTotalRecord.BaseCharge;
            decimal? maxCharge = shippingByTotalRecord.MaxCharge;

            if (shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargePercentage <= decimal.Zero)
            {
                return baseCharge; //decimal.Zero;
            }

            if (!shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargeAmount <= decimal.Zero)
            {
                return decimal.Zero;
            }

            if (shippingByTotalRecord.UsePercentage)
            {
                shippingTotal = Math.Round((decimal)((((float)subtotal) * ((float)shippingByTotalRecord.ShippingChargePercentage)) / 100f), 2);
                shippingTotal += baseCharge;
                if (maxCharge.HasValue && maxCharge > baseCharge)
                {
                    // shipping charge should not exceed MaxCharge
                    shippingTotal = Math.Min(shippingTotal.Value, maxCharge.Value);
                }
            }
            else
            {
                shippingTotal = shippingByTotalRecord.ShippingChargeAmount;
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
            {
                throw new ArgumentNullException("getShippingOptionRequest");
            }

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
            int stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId.HasValue ? getShippingOptionRequest.ShippingAddress.StateProvinceId.Value : 0;
            string zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            decimal subTotal = decimal.Zero;

            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.Item.IsFreeShipping || !shoppingCartItem.Item.IsShipEnabled)
                {
                    continue;
                }
                subTotal += _priceCalculationService.GetSubTotal(shoppingCartItem, true);
            }

            decimal sqThreshold = _shippingByTotalSettings.SmallQuantityThreshold;
            decimal sqSurcharge = _shippingByTotalSettings.SmallQuantitySurcharge;

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, shippingMethod.Id, storeId, countryId, stateProvinceId, zip);
                if (rate.HasValue)
                {
                    if (rate > 0 && sqThreshold > 0 && subTotal <= sqThreshold)
                    {
                        // add small quantity surcharge (Mindermengenzuschalg)
                        rate += sqSurcharge;
                    }
                    
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.Name;
                    shippingOption.Description = shippingMethod.Description;
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
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingByTotal";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Shipping.ByTotal.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {            
            var settings = new ShippingByTotalSettings()
            {                
                LimitMethodsToCreated = false,
                SmallQuantityThreshold = 0,
                SmallQuantitySurcharge = 0
            };
            _settingService.SaveSetting(settings);

            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();

            _logger.Information(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _settingService.DeleteSetting<ShippingByTotalSettings>();

            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Shipping.ByTotal", false);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }

        #endregion
    }
}
