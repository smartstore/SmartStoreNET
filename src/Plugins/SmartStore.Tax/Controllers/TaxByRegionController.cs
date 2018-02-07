using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Tax.Domain;
using SmartStore.Tax.Models;
using SmartStore.Tax.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Tax.Controllers
{
	[AdminAuthorize]
    public class TaxByRegionController : PluginControllerBase
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxRateService _taxRateService;

		public TaxByRegionController(ITaxCategoryService taxCategoryService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            ITaxRateService taxRateService)
        {
            this._taxCategoryService = taxCategoryService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._taxRateService = taxRateService;
        }


        public ActionResult Configure()
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");

            var model = new ByRegionTaxRateListModel();
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString() });
            var countries = _countryService.GetAllCountries(true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            model.AvailableStates.Add(new SelectListItem() { Text = "*", Value = "0" });
            var states = _stateProvinceService.GetStateProvincesByCountryId(countries.FirstOrDefault().Id);
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString() });
            }

            model.TaxRates = _taxRateService.GetAllTaxRates()
                .Select(x =>
                {
					var m = new ByRegionTaxRateModel
                    {
                        Id = x.Id,
                        TaxCategoryId = x.TaxCategoryId,
                        CountryId = x.CountryId,
                        StateProvinceId = x.StateProvinceId,
                        Zip = x.Zip,
                        Percentage = x.Percentage,
                    };
                    var tc = _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
                    m.TaxCategoryName = (tc != null) ? tc.Name : "";
                    var c = _countryService.GetCountryById(x.CountryId);
                    m.CountryName = (c != null) ? c.Name : T("Common.Unavailable").Text;
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                    m.StateProvinceName = (s != null) ? s.Name : "*";
                    m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";
                    return m;
                })
                .ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
            var taxRatesModel = _taxRateService.GetAllTaxRates()
                .Select(x =>
                {
					var m = new ByRegionTaxRateModel()
                    {
                        Id = x.Id,
                        TaxCategoryId = x.TaxCategoryId,
                        CountryId = x.CountryId,
                        StateProvinceId = x.StateProvinceId,
                        Zip = x.Zip,
                        Percentage = x.Percentage,
                    };
                    var tc = _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
                    m.TaxCategoryName = (tc != null) ? tc.Name : "";
                    var c = _countryService.GetCountryById(x.CountryId);
                    m.CountryName = (c != null) ? c.Name : T("Common.Unavailable").Text;
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                    m.StateProvinceName = (s != null) ? s.Name : "*";
                    m.Zip = (!String.IsNullOrEmpty(x.Zip)) ? x.Zip : "*";
                    return m;
                })
                .ToList();
			var model = new GridModel<ByRegionTaxRateModel>
            {
                Data = taxRatesModel,
                Total = taxRatesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
		public ActionResult RateUpdate(ByRegionTaxRateModel model, GridCommand command)
        {
            var taxRate = _taxRateService.GetTaxRateById(model.Id);
            taxRate.Zip = model.Zip == "*" ? null : model.Zip;
            taxRate.Percentage = model.Percentage;
            _taxRateService.UpdateTaxRate(taxRate);

            return RatesList(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateDelete(int id, GridCommand command)
        {
            var taxRate = _taxRateService.GetTaxRateById(id);
            _taxRateService.DeleteTaxRate(taxRate);

            return RatesList(command);
        }

		[HttpPost]
		public ActionResult Configure(ByRegionTaxRateListModel model)
		{
			return Configure();
		}

		[HttpPost]
		public ActionResult AddTaxByRegionRecord(ByRegionTaxRateListModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            var taxRate = new TaxRate()
            {
                TaxCategoryId = model.AddTaxCategoryId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                Zip = model.AddZip,
                Percentage = model.AddPercentage
            };

            _taxRateService.InsertTaxRate(taxRate);

			NotifySuccess(T("Plugins.Tax.CountryStateZip.AddNewRecord.Success"));

			return Json(new { Result = true });
		}
    }
}
