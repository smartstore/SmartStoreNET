using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tax;
using SmartStore.Tax.Models;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Tax.Controllers
{
    [AdminAuthorize]
    public class TaxFixedRateController : PluginControllerBase
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;

        public TaxFixedRateController(ITaxCategoryService taxCategoryService, ISettingService settingService)
        {
            _taxCategoryService = taxCategoryService;
            _settingService = settingService;
        }

        public ActionResult Configure()
        {
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");

            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in taxCategories)
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp,
                Total = tmp.Count
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Configure(GridCommand command)
        {
            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TaxRateUpdate(FixedTaxRateModel model, GridCommand command)
        {
            int taxCategoryId = model.TaxCategoryId;
            decimal rate = model.Rate;

            _settingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId), rate);

            var tmp = new List<FixedTaxRateModel>();
            foreach (var taxCategory in _taxCategoryService.GetAllTaxCategories())
                tmp.Add(new FixedTaxRateModel()
                {
                    TaxCategoryId = taxCategory.Id,
                    TaxCategoryName = taxCategory.Name,
                    Rate = GetTaxRate(taxCategory.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedTaxRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [NonAction]
        protected decimal GetTaxRate(int taxCategoryId)
        {
            decimal rate = _settingService.GetSettingByKey<decimal>(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", taxCategoryId));
            return rate;
        }
    }
}
