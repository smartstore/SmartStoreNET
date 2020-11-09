using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Directory;
using SmartStore.Services.Tax;
using SmartStore.Tax.Domain;
using SmartStore.Tax.Models;
using SmartStore.Tax.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Tax.Controllers
{
    [AdminAuthorize]
    public class TaxByRegionController : PluginControllerBase
    {
        private readonly ITaxRateService _taxRateService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        public TaxByRegionController(
            ITaxRateService taxRateService,
            ITaxCategoryService taxCategoryService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService)
        {
            _taxRateService = taxRateService;
            _taxCategoryService = taxCategoryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        private void PrepareModel(ByRegionTaxRateListModel model)
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories().ToDictionary(x => x.Id);
            var taxRates = _taxRateService.GetAllTaxRates();
            var countries = _countryService.GetAllCountries(true).ToDictionary(x => x.Id);
            var stateProvinces = _stateProvinceService.GetAllStateProvinces(true).ToDictionary(x => x.Id);
            var stateProvincesOfFirstCountry = stateProvinces.Values.Where(x => x.CountryId == countries.Values.FirstOrDefault().Id).ToList();
            var unavailable = T("Common.Unavailable").Text;

            model.AvailableTaxCategories = taxCategories.Values.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
            .ToList();

            model.AvailableCountries = countries.Values.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
            .ToList();

            model.AvailableStates = stateProvincesOfFirstCountry.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            })
            .ToList();
            model.AvailableStates.Insert(0, new SelectListItem { Text = "*", Value = "0" });

            model.TaxRates = taxRates.Select(x =>
            {
                var m = new ByRegionTaxRateModel
                {
                    Id = x.Id,
                    TaxCategoryId = x.TaxCategoryId,
                    CountryId = x.CountryId,
                    StateProvinceId = x.StateProvinceId,
                    Zip = x.Zip.HasValue() ? x.Zip : "*",
                    Percentage = x.Percentage
                };

                taxCategories.TryGetValue(x.TaxCategoryId, out TaxCategory tc);
                m.TaxCategoryName = tc?.Name.EmptyNull();

                countries.TryGetValue(x.CountryId, out Country c);
                m.CountryName = c?.Name ?? unavailable;

                stateProvinces.TryGetValue(x.StateProvinceId, out StateProvince s);
                m.StateProvinceName = s?.Name ?? "*";

                return m;
            })
            .ToList();
        }

        public ActionResult Configure()
        {
            var model = new ByRegionTaxRateListModel();
            PrepareModel(model);

            if (!model.AvailableTaxCategories.Any())
            {
                NotifyWarning(T("Plugins.Tax.CountryStateZip.NoTaxCategoriesFound"));
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
            var model = new ByRegionTaxRateListModel();
            PrepareModel(model);

            var data = new GridModel<ByRegionTaxRateModel>
            {
                Data = model.TaxRates,
                Total = model.TaxRates.Count
            };

            return new JsonResult
            {
                Data = data
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
        [ValidateAntiForgeryToken]
        public ActionResult AddTaxByRegionRecord(ByRegionTaxRateListModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            var taxRate = new TaxRate
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
