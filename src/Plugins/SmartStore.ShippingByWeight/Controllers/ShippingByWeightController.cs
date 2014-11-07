using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.ShippingByWeight.Domain;
using SmartStore.ShippingByWeight.Models;
using SmartStore.ShippingByWeight.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.ShippingByWeight.Controllers
{

	[AdminAuthorize]
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
		private readonly AdminAreaSettings _adminAreaSettings;

        public ShippingByWeightController(IShippingService shippingService,
			IStoreService storeService, ICountryService countryService, ShippingByWeightSettings shippingByWeightSettings,
            IShippingByWeightService shippingByWeightService, ISettingService settingService,
            ICurrencyService currencyService, CurrencySettings currencySettings,
            IMeasureService measureService, MeasureSettings measureSettings,
			AdminAreaSettings adminAreaSettings)
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
			this._adminAreaSettings = adminAreaSettings;
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
			model.GridPageSize = _adminAreaSettings.GridPageSize;

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
			int totalCount;
            var data = _shippingByWeightService.GetShippingByWeightModels(command.Page - 1, command.PageSize, out totalCount);

            var model = new GridModel<ShippingByWeightModel>
            {
                Data = data,
                Total = totalCount
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
            sbw.SmallQuantitySurcharge = model.SmallQuantitySurcharge;
            sbw.SmallQuantityThreshold = model.SmallQuantityThreshold;
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
                ShippingChargePercentage = model.AddShippingChargePercentage,
                SmallQuantitySurcharge = model.SmallQuantitySurcharge,
                SmallQuantityThreshold = model.SmallQuantityThreshold,
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
