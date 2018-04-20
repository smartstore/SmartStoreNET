using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Shipping.Domain;
using SmartStore.Shipping.Models;
using SmartStore.Shipping.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Shipping.Controllers
{
    [AdminAuthorize]
    public class ByTotalController : PluginControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly ICountryService _countryService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ICommonServices _services;

        public ByTotalController(
			IShippingService shippingService,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings, 
            ICountryService countryService,
			AdminAreaSettings adminAreaSettings,
			ICommonServices services)
        {
            _shippingService = shippingService;
            _shippingByTotalService = shippingByTotalService;
            _shippingByTotalSettings = shippingByTotalSettings;
            _countryService = countryService;
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

            var model = new ByTotalListModel();
			var allStores = _services.StoreService.GetAllStores();

            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            }

			//stores
			model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
			foreach (var store in allStores)
			{
				model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
			}

            //model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(true);
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }

            model.LimitMethodsToCreated = _shippingByTotalSettings.LimitMethodsToCreated;
            model.SmallQuantityThreshold = _shippingByTotalSettings.SmallQuantityThreshold;
            model.SmallQuantitySurcharge = _shippingByTotalSettings.SmallQuantitySurcharge;
            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
			model.GridPageSize = _adminAreaSettings.GridPageSize;

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
			int totalCount;
            var data = _shippingByTotalService.GetShippingByTotalModels(command.Page - 1, command.PageSize, out totalCount);

            var model = new GridModel<ByTotalModel>
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
        public ActionResult RateUpdate(ByTotalModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult { Data = T("Admin.Common.UnknownError").Text };
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
        public ActionResult AddShippingRate(ByTotalListModel model)
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

			NotifySuccess(T("Plugins.Shipping.ByTotal.AddNewRecord.Success"));

			return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult Configure(ByTotalListModel model)
        {
            _shippingByTotalSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _shippingByTotalSettings.SmallQuantityThreshold = model.SmallQuantityThreshold;
            _shippingByTotalSettings.SmallQuantitySurcharge = model.SmallQuantitySurcharge;

            _services.Settings.SaveSetting(_shippingByTotalSettings);

			NotifySuccess(T("Admin.Configuration.Updated"));

			return Configure();
		}
    }
}
