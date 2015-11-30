using System;
using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.ShippingByWeight.Data;
using SmartStore.ShippingByWeight.Data.Migrations;
using SmartStore.ShippingByWeight.Services;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;

namespace SmartStore.ShippingByWeight
{
	public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod, IConfigurable
    {
        #region Fields

        private readonly IShippingService _shippingService;
		private readonly IStoreContext _storeContext;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly ShippingByWeightObjectContext _objectContext;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICommonServices _services;
        
        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
			IStoreContext storeContext,
            IShippingByWeightService shippingByWeightService,
            IPriceCalculationService priceCalculationService, 
            ShippingByWeightSettings shippingByWeightSettings,
            ShippingByWeightObjectContext objectContext,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            ICommonServices services)
        {
            this._shippingService = shippingService;
			this._storeContext = storeContext;
            this._shippingByWeightService = shippingByWeightService;
            this._priceCalculationService = priceCalculationService;
            this._shippingByWeightSettings = shippingByWeightSettings;
            this._objectContext = objectContext;
            this._localizationService = localizationService;
            this._priceFormatter = priceFormatter;
            this._services = services;
        }
        #endregion

        #region Utilities

        private decimal? GetRate(decimal subTotal, decimal weight, int shippingMethodId, int storeId, int countryId, string zip)
        {
            decimal? shippingTotal = null;

			var shippingByWeightRecord = _shippingByWeightService.FindRecord(shippingMethodId, storeId, countryId, weight, zip);
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

			int storeId = _storeContext.CurrentStore.Id;
			decimal subTotal = decimal.Zero;
            int countryId = 0;
            string zip = null;

			if (getShippingOptionRequest.ShippingAddress != null)
			{
				countryId = getShippingOptionRequest.ShippingAddress.CountryId ?? 0;
                zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
			}
            
            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.Item.IsFreeShipping || !shoppingCartItem.Item.IsShipEnabled)
                    continue;
                subTotal += _priceCalculationService.GetSubTotal(shoppingCartItem, true);
            }
            decimal weight = _shippingService.GetShoppingCartTotalWeight(getShippingOptionRequest.Items);

            var shippingMethods = _shippingService.GetAllShippingMethods(getShippingOptionRequest.Customer);
            foreach (var shippingMethod in shippingMethods)
            {
                var record = _shippingByWeightService.FindRecord(shippingMethod.Id, storeId, countryId, weight, zip);
                
                decimal? rate = GetRate(subTotal, weight, shippingMethod.Id, storeId, countryId, zip);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
					shippingOption.ShippingMethodId = shippingMethod.Id;
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);

                    if (record != null && record.SmallQuantityThreshold > subTotal)
                    {
                        shippingOption.Description = shippingMethod.GetLocalized(x => x.Description)
                            + _localizationService.GetResource("Plugin.Shipping.ByWeight.SmallQuantitySurchargeNotReached").FormatWith(
                                _priceFormatter.FormatPrice(record.SmallQuantitySurcharge),
                                _priceFormatter.FormatPrice(record.SmallQuantityThreshold));

                        shippingOption.Rate = rate.Value + record.SmallQuantitySurcharge;
                    }
                    else {
                        shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                        shippingOption.Rate = rate.Value;
                    }
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
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.ShippingByWeight" } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
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

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);
            
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

		public bool IsActive
		{
			get { return true; }
		}

        #endregion
    }
}
