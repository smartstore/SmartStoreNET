using System;
using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;
using SmartStore.Services.Tax;
using SmartStore.ShippingByWeight.Data.Migrations;
using SmartStore.ShippingByWeight.Services;

namespace SmartStore.ShippingByWeight
{
    public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod, IConfigurable
    {
        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICommonServices _services;
        private readonly ITaxService _taxService;

        public ByWeightShippingComputationMethod(
            IShippingService shippingService,
            IStoreContext storeContext,
            IShippingByWeightService shippingByWeightService,
            IPriceCalculationService priceCalculationService,
            ShippingByWeightSettings shippingByWeightSettings,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            ICommonServices services,
            ITaxService taxService)
        {
            _shippingService = shippingService;
            _storeContext = storeContext;
            _shippingByWeightService = shippingByWeightService;
            _priceCalculationService = priceCalculationService;
            _shippingByWeightSettings = shippingByWeightSettings;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _services = services;
            _taxService = taxService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

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
        /// <param name="request">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (request.Items == null || request.Items.Count == 0)
            {
                response.AddError(T("Admin.System.Warnings.NoShipmentItems"));
                return response;
            }

            int storeId = request.StoreId > 0 ? request.StoreId : _storeContext.CurrentStore.Id;
            var taxRate = decimal.Zero;
            decimal subTotalInclTax = decimal.Zero;
            decimal subTotalExclTax = decimal.Zero;
            decimal currentSubTotal = decimal.Zero;
            int countryId = 0;
            string zip = null;

            if (request.ShippingAddress != null)
            {
                countryId = request.ShippingAddress.CountryId ?? 0;
                zip = request.ShippingAddress.ZipPostalCode;
            }

            foreach (var shoppingCartItem in request.Items)
            {
                if (shoppingCartItem.Item.IsFreeShipping || !shoppingCartItem.Item.IsShipEnabled)
                {
                    continue;
                }

                var itemSubTotal = _priceCalculationService.GetSubTotal(shoppingCartItem, true);

                var itemSubTotalInclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, itemSubTotal, true, request.Customer, out taxRate);
                subTotalInclTax += itemSubTotalInclTax;

                var itemSubTotalExclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, itemSubTotal, false, request.Customer, out taxRate);
                subTotalExclTax += itemSubTotalExclTax;
            }

            var weight = _shippingService.GetShoppingCartTotalWeight(request.Items, _shippingByWeightSettings.IncludeWeightOfFreeShippingProducts);
            var shippingMethods = _shippingService.GetAllShippingMethods(request, storeId);
            currentSubTotal = _services.WorkContext.TaxDisplayType == TaxDisplayType.ExcludingTax ? subTotalExclTax : subTotalInclTax;

            foreach (var shippingMethod in shippingMethods)
            {
                var record = _shippingByWeightService.FindRecord(shippingMethod.Id, storeId, countryId, weight, zip);

                decimal? rate = GetRate(subTotalInclTax, weight, shippingMethod.Id, storeId, countryId, zip);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.ShippingMethodId = shippingMethod.Id;
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);

                    if (record != null && record.SmallQuantityThreshold > currentSubTotal)
                    {
                        string surchargeHint = T("Plugins.Shipping.ByWeight.SmallQuantitySurchargeNotReached",
                            _priceFormatter.FormatPrice(record.SmallQuantitySurcharge),
                            _priceFormatter.FormatPrice(record.SmallQuantityThreshold));

                        shippingOption.Description = shippingMethod.GetLocalized(x => x.Description) + surchargeHint;
                        shippingOption.Rate = rate.Value + record.SmallQuantitySurcharge;
                    }
                    else
                    {
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
        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Offline;


        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker =>
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
                null;

        public bool IsActive => true;

        #endregion
    }
}
