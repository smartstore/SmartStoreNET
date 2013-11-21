using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Core.Domain.Directory;
using SmartStore.Plugin.Shipping.ByWeight.Domain;
using SmartStore.Plugin.Shipping.ByWeight.Models;
using SmartStore.Plugin.Shipping.ByWeight.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Shipping.ByWeight.Controllers
{

    public class ShippingByWeightController : PluginControllerBase
    {
        private readonly IShippingService _shippingService;
		private readonly IStoreService _storeService;
        private readonly ICountryService _countryService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly ISettingService _settingService;

        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;

        public ShippingByWeightController(IShippingService shippingService,
			IStoreService storeService, ICountryService countryService, ShippingByWeightSettings shippingByWeightSettings,
            IShippingByWeightService shippingByWeightService, ISettingService settingService,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            IMeasureService measureService, MeasureSettings measureSettings)
        {
            this._shippingService = shippingService;
			this._storeService = storeService;
            this._countryService = countryService;
            this._shippingByWeightSettings = shippingByWeightSettings;
            this._shippingByWeightService = shippingByWeightService;
            this._settingService = settingService;

            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
        }

        public ActionResult Configure()
        {
            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
                return Content("No shipping methods can be loaded");

            var model = new ShippingByWeightListModel();
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem() { Text = sm.Name, Value = sm.Id.ToString() });

			//stores
			model.AvailableStores.Add(new SelectListItem() { Text = "*", Value = "0" });
			foreach (var store in _storeService.GetAllStores())
				model.AvailableStores.Add(new SelectListItem() { Text = store.Name, Value = store.Id.ToString() });

            model.AvailableCountries.Add(new SelectListItem() { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.LimitMethodsToCreated = _shippingByWeightSettings.LimitMethodsToCreated;
            model.CalculatePerWeightUnit = _shippingByWeightSettings.CalculatePerWeightUnit;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            model.Records = _shippingByWeightService.GetAll()
                .Select(x =>
                {
                    var m = new ShippingByWeightModel()
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
                    };
					//shipping method
                    var shippingMethodId = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                    m.ShippingMethodName = (shippingMethodId != null) ? shippingMethodId.Name : "Unavailable";
					//store
					var store = _storeService.GetStoreById(x.StoreId);
					m.StoreName = (store != null) ? store.Name : "*";
                    if (x.CountryId > 0)
                    {
                        var c = _countryService.GetCountryById(x.CountryId);
                        m.CountryName = (c != null) ? c.Name : "Unavailable";
                    }
                    else
                    {
                        m.CountryName = "*";
                    }

                    return m;
                })
                .ToList();

            return View("SmartStore.Plugin.Shipping.ByWeight.Views.ShippingByWeight.Configure", model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
            var sbwModel = _shippingByWeightService.GetAll()
                .Select(x =>
                {
                    var m = new ShippingByWeightModel()
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
                    };
					//shipping method
                    var shippingMethodId = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                    m.ShippingMethodName = (shippingMethodId != null) ? shippingMethodId.Name : "Unavailable";
					//store
					var store = _storeService.GetStoreById(x.StoreId);
					m.StoreName = (store != null) ? store.Name : "*";
                    if (x.CountryId > 0)
                    {
                        var c = _countryService.GetCountryById(x.CountryId);
                        m.CountryName = (c != null) ? c.Name : "Unavailable";
                    }
                    else
                    {
                        m.CountryName = "*";
                    }
                    return m;
                })
                .ToList();
            var model = new GridModel<ShippingByWeightModel>
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
        public ActionResult RateUpdate(ShippingByWeightModel model, GridCommand command)
        {
            var sbw = _shippingByWeightService.GetById(model.Id);
            sbw.From = model.From;
            sbw.To = model.To;
            sbw.UsePercentage = model.UsePercentage;
            sbw.ShippingChargeAmount = model.ShippingChargeAmount;
            sbw.ShippingChargePercentage = model.ShippingChargePercentage;
            _shippingByWeightService.UpdateShippingByWeightRecord(sbw);

            return RatesList(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateDelete(int id, GridCommand command)
        {
            var sbw = _shippingByWeightService.GetById(id);
            _shippingByWeightService.DeleteShippingByWeightRecord(sbw);

            return RatesList(command);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("addshippingbyweightrecord")]
        public ActionResult AddShippingByWeightRecord(ShippingByWeightListModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            var sbw = new ShippingByWeightRecord()
            {
				StoreId = model.AddStoreId,
                ShippingMethodId = model.AddShippingMethodId,
                CountryId = model.AddCountryId,
				//StateProvinceId = 0,
                From = model.AddFrom,
                To = model.AddTo,
                UsePercentage = model.AddUsePercentage,
                ShippingChargeAmount = model.AddShippingChargeAmount,
                ShippingChargePercentage = model.AddShippingChargePercentage
            };
            _shippingByWeightService.InsertShippingByWeightRecord(sbw);

            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("savegeneralsettings")]
        public ActionResult SaveGeneralSettings(ShippingByWeightListModel model)
        {
            //save settings
            _shippingByWeightSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _shippingByWeightSettings.CalculatePerWeightUnit = model.CalculatePerWeightUnit;
            _settingService.SaveSetting(_shippingByWeightSettings);
            
            return Configure();
        }
        
    }
}
