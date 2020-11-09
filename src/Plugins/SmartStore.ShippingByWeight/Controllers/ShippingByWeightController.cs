using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Shipping;
using SmartStore.ShippingByWeight.Domain;
using SmartStore.ShippingByWeight.Models;
using SmartStore.ShippingByWeight.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.ShippingByWeight.Controllers
{
    [AdminAuthorize]
    public class ShippingByWeightController : PluginControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly ICountryService _countryService;
        private readonly ShippingByWeightSettings _shippingByWeightSettings;
        private readonly IShippingByWeightService _shippingByWeightService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly ICommonServices _services;

        public ShippingByWeightController(
            IShippingService shippingService,
            ICountryService countryService,
            ShippingByWeightSettings shippingByWeightSettings,
            IShippingByWeightService shippingByWeightService,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            AdminAreaSettings adminAreaSettings,
            ICommonServices services)
        {
            _shippingService = shippingService;
            _countryService = countryService;
            _shippingByWeightSettings = shippingByWeightSettings;
            _shippingByWeightService = shippingByWeightService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _adminAreaSettings = adminAreaSettings;
            _services = services;
        }

        public ActionResult Configure()
        {
            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
            {
                return Content(T("Admin.Configuration.Shipping.Methods.NoMethodsLoaded"));
            }

            var model = new ShippingByWeightListModel();
            var countries = _countryService.GetAllCountries(true);
            var allStores = _services.StoreService.GetAllStores();

            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            }

            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in allStores)
            {
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            }

            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }

            model.LimitMethodsToCreated = _shippingByWeightSettings.LimitMethodsToCreated;
            model.CalculatePerWeightUnit = _shippingByWeightSettings.CalculatePerWeightUnit;
            model.IncludeWeightOfFreeShippingProducts = _shippingByWeightSettings.IncludeWeightOfFreeShippingProducts;
            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)?.GetLocalized(x => x.Name) ?? string.Empty;
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
            sbw.Zip = model.Zip == "*" ? null : model.Zip;
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

        [HttpPost]
        public ActionResult AddShippingByWeightRecord(ShippingByWeightListModel model)
        {
            var sbw = new ShippingByWeightRecord()
            {
                StoreId = model.AddStoreId,
                ShippingMethodId = model.AddShippingMethodId,
                CountryId = model.AddCountryId,
                Zip = model.AddZip,
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

            NotifySuccess(T("Plugins.Shipping.ByWeight.AddNewRecord.Success"));

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult Configure(ShippingByWeightListModel model)
        {
            _shippingByWeightSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _shippingByWeightSettings.CalculatePerWeightUnit = model.CalculatePerWeightUnit;
            _shippingByWeightSettings.IncludeWeightOfFreeShippingProducts = model.IncludeWeightOfFreeShippingProducts;

            _services.Settings.SaveSetting(_shippingByWeightSettings);

            NotifySuccess(T("Admin.Configuration.Updated"));

            return Configure();
        }
    }
}
