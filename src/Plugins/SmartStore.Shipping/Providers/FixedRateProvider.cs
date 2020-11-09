using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;

namespace SmartStore.Shipping
{
    /// <summary>
    /// Fixed rate shipping computation provider
    /// </summary>
    [SystemName("Shipping.FixedRate")]
    [FriendlyName("Fixed Rate Shipping")]
    [DisplayOrder(0)]
    public class FixedRateProvider : IShippingRateComputationMethod, IConfigurable
    {
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;

        public FixedRateProvider(ISettingService settingService,
            IShippingService shippingService)
        {
            this._settingService = settingService;
            this._shippingService = shippingService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private decimal GetRate(int shippingMethodId)
        {
            string key = string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId);
            decimal rate = this._settingService.GetSettingByKey<decimal>(key);
            return rate;
        }

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
                response.AddError(T("Admin.System.Warnings.NoShipmentItems"));
                return response;
            }

            var shippingMethods = this._shippingService.GetAllShippingMethods(getShippingOptionRequest, getShippingOptionRequest.StoreId);
            foreach (var shippingMethod in shippingMethods)
            {
                var shippingOption = new ShippingOption();
                shippingOption.ShippingMethodId = shippingMethod.Id;
                shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                shippingOption.Rate = GetRate(shippingMethod.Id);
                response.ShippingOptions.Add(shippingOption);
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
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var shippingMethods = _shippingService.GetAllShippingMethods(getShippingOptionRequest, getShippingOptionRequest.StoreId);

            var rates = new List<decimal>();
            foreach (var shippingMethod in shippingMethods)
            {
                decimal rate = GetRate(shippingMethod.Id);
                if (!rates.Contains(rate))
                    rates.Add(rate);
            }

            //return default rate if all of them equal
            if (rates.Count == 1)
                return rates[0];

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
            controllerName = "FixedRate";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.Shipping" } };
        }

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Offline;

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker =>
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
                null;

        public bool IsActive => true;
    }
}
