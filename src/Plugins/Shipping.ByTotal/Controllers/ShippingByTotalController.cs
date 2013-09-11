using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Core.Domain.Directory;
using SmartStore.Plugin.Shipping.ByTotal.Domain;
using SmartStore.Plugin.Shipping.ByTotal.Models;
using SmartStore.Plugin.Shipping.ByTotal.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Shipping.ByTotal.Controllers
{
    [AdminAuthorize]
    public class ShippingByTotalController : PluginControllerBase
    {
        private readonly IShippingService _shippingService;
		private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        public ShippingByTotalController(IShippingService shippingService,
			IStoreService storeService, 
            ISettingService settingService, 
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings, 
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICurrencyService currencyService, 
            CurrencySettings currencySettings)
        {
            this._shippingService = shippingService;
			this._storeService = storeService;
            this._settingService = settingService;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
        }

        public ActionResult Configure()
        {
            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
            {
                return Content("No shipping methods can be loaded");
            }

            var model = new ShippingByTotalListModel();
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem() { Text = sm.Name, Value = sm.Id.ToString() });
            }

			//stores
			model.AvailableStores.Add(new SelectListItem() { Text = "*", Value = "0" });
			foreach (var store in _storeService.GetAllStores())
			{
				model.AvailableStores.Add(new SelectListItem() { Text = store.Name, Value = store.Id.ToString() });
			}

            //model.AvailableCountries.Add(new SelectListItem() { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(true);
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            }

            //model.AvailableStates.Add(new SelectListItem() { Text = "*", Value = "0" });
            model.LimitMethodsToCreated = _shippingByTotalSettings.LimitMethodsToCreated;
            model.SmallQuantityThreshold = _shippingByTotalSettings.SmallQuantityThreshold;
            model.SmallQuantitySurcharge = _shippingByTotalSettings.SmallQuantitySurcharge;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            model.Records = _shippingByTotalService.GetAllShippingByTotalRecords()
                .Select(x =>
                {
                    var m = new ShippingByTotalModel
                    {
                        Id = x.Id,
						StoreId = x.StoreId,
                        ShippingMethodId = x.ShippingMethodId,
                        CountryId = x.CountryId,
                        StateProvinceId = x.StateProvinceId,
                        Zip = x.Zip,
                        From = x.From,
                        To = x.To,
                        UsePercentage = x.UsePercentage,
                        ShippingChargePercentage = x.ShippingChargePercentage,
                        ShippingChargeAmount = x.ShippingChargeAmount,
                        BaseCharge = x.BaseCharge,
                        MaxCharge = x.MaxCharge
                    };
                    var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                    m.ShippingMethodName = (shippingMethod != null) ? shippingMethod.Name : "Unavailable";

					//store
					var store = _storeService.GetStoreById(x.StoreId);
					m.StoreName = (store != null) ? store.Name : "*";
                    
                    var c = _countryService.GetCountryById(x.CountryId ?? 0);
                    m.CountryName = (c != null) ? c.Name : "*";
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId ?? 0);
                    m.StateProvinceName = (s != null) ? s.Name : "*";
                    m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";

                    return m;
                })
                .ToList();

            return View("SmartStore.Plugin.Shipping.ByTotal.Views.ShippingByTotal.Configure", model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
            var sbwModel = _shippingByTotalService.GetAllShippingByTotalRecords()
                .Select(x =>
                {
                    var m = new ShippingByTotalModel
                    {
                        Id = x.Id,
						StoreId = x.StoreId,
                        ShippingMethodId = x.ShippingMethodId,
                        CountryId = x.CountryId,
                        From = x.From,
                        To = x.To,
                        UsePercentage = x.UsePercentage,
                        ShippingChargePercentage = x.ShippingChargePercentage,
                        ShippingChargeAmount = x.ShippingChargeAmount,
                        BaseCharge = x.BaseCharge,
                        MaxCharge = x.MaxCharge
                    };
                    var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                    m.ShippingMethodName = (shippingMethod != null) ? shippingMethod.Name : "Unavailable";

					//store
					var store = _storeService.GetStoreById(x.StoreId);
					m.StoreName = (store != null) ? store.Name : "*";
                    
                    var c = _countryService.GetCountryById(x.CountryId ?? 0);
                    m.CountryName = (c != null) ? c.Name : "*";
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId ?? 0);
                    m.StateProvinceName = (s != null) ? s.Name : "*";
                    m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";
                    
                    return m;
                })
                .ToList();
            var model = new GridModel<ShippingByTotalModel>
            {
                Data = sbwModel,
                Total = sbwModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateUpdate(ShippingByTotalModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult { Data = "error" };
            }

            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(model.Id);
            shippingByTotalRecord.Zip = model.Zip == "*" ? null : model.Zip;
            shippingByTotalRecord.From = model.From;
            shippingByTotalRecord.To = model.To;
            shippingByTotalRecord.UsePercentage = model.UsePercentage;
            shippingByTotalRecord.ShippingChargeAmount = model.ShippingChargeAmount;
            shippingByTotalRecord.ShippingChargePercentage = model.ShippingChargePercentage;
            shippingByTotalRecord.BaseCharge = model.BaseCharge;
            shippingByTotalRecord.MaxCharge = model.MaxCharge;
            _shippingByTotalService.UpdateShippingByTotalRecord(shippingByTotalRecord);

            return RatesList(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateDelete(int id, GridCommand command)
        {
            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(id);
            if (shippingByTotalRecord != null)
            {
                _shippingByTotalService.DeleteShippingByTotalRecord(shippingByTotalRecord);
            }
            return RatesList(command);
        }

        [HttpPost]
        public ActionResult AddShippingRate(ShippingByTotalListModel model)
        {
            var shippingByTotalRecord = new ShippingByTotalRecord
            {
				StoreId = model.AddStoreId,
                ShippingMethodId = model.AddShippingMethodId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                Zip = model.AddZip,
                From = model.AddFrom,
                To = model.AddTo,
                UsePercentage = model.AddUsePercentage,                
                ShippingChargePercentage = (model.AddUsePercentage) ? model.AddShippingChargePercentage : 0,
                ShippingChargeAmount = (model.AddUsePercentage) ? 0 : model.AddShippingChargeAmount,
                BaseCharge = model.AddBaseCharge,
                MaxCharge = model.AddMaxCharge
            };
            _shippingByTotalService.InsertShippingByTotalRecord(shippingByTotalRecord);

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult SaveGeneralSettings(ShippingByTotalListModel model)
        {
            //save settings
            _shippingByTotalSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _shippingByTotalSettings.SmallQuantityThreshold = model.SmallQuantityThreshold;
            _shippingByTotalSettings.SmallQuantitySurcharge = model.SmallQuantitySurcharge;

            _settingService.SaveSetting(_shippingByTotalSettings);

            return Json(new { Result = true });
        }
    }
}
