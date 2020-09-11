using System;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;
using SmartStore.Services.Tax;
using SmartStore.Shipping.Services;

namespace SmartStore.Shipping
{
    [SystemName("Shipping.ByTotal")]
    [FriendlyName("Shipping by total")]
    [DisplayOrder(1)]
    public class ByTotalProvider : IShippingRateComputationMethod, IConfigurable
    {
        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;

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
        public ByTotalProvider(IShippingService shippingService,
            IStoreContext storeContext,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings,
            IPriceCalculationService priceCalculationService,
            ILogger logger,
            ISettingService settingService,
            ILocalizationService localizationService,
            ITaxService taxService)
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._priceCalculationService = priceCalculationService;
            this._logger = logger;
            this._settingService = settingService;
            this._localizationService = localizationService;
            _taxService = taxService;

            T = NullLocalizer.Instance;
        }

        #region Properties

        public Localizer T { get; set; }

        /// <summary>
        ///  Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Offline;

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
                if (maxCharge.HasValue && shippingTotal > maxCharge)
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
                response.AddError(T("Admin.System.Warnings.NoShipmentItems"));
                return response;
            }

            int countryId = 0;
            int stateProvinceId = 0;
            string zip = null;
            decimal subTotal = decimal.Zero;
            int storeId = _storeContext.CurrentStore.Id;

            if (getShippingOptionRequest.ShippingAddress != null)
            {
                countryId = getShippingOptionRequest.ShippingAddress.CountryId ?? 0;
                stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId ?? 0;
                zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            }

            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.Item.IsFreeShipping || !shoppingCartItem.Item.IsShipEnabled)
                {
                    continue;
                }

                var itemSubTotalBase = _priceCalculationService.GetSubTotal(shoppingCartItem, true);
                var itemSubTotal = _taxService.GetProductPrice(
                    shoppingCartItem.Item.Product,
                    itemSubTotalBase,
                    _shippingByTotalSettings.CalculateTotalIncludingTax,
                    getShippingOptionRequest.Customer,
                    out var _);

                subTotal += itemSubTotal;
            }

            var sqThreshold = _shippingByTotalSettings.SmallQuantityThreshold;
            var sqSurcharge = _shippingByTotalSettings.SmallQuantitySurcharge;

            var shippingMethods = _shippingService.GetAllShippingMethods(getShippingOptionRequest, storeId);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, shippingMethod.Id, storeId, countryId, stateProvinceId, zip);
                if (rate.HasValue)
                {
                    if (rate > 0 && sqThreshold > 0 && subTotal <= sqThreshold)
                    {
                        // Add small quantity surcharge (Mindermengenzuschlag).
                        rate += sqSurcharge;
                    }

                    var shippingOption = new ShippingOption();
                    shippingOption.ShippingMethodId = shippingMethod.Id;
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
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker => null;

        public bool IsActive => true;

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ByTotal";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.Shipping" } };
        }

        #endregion
    }
}
