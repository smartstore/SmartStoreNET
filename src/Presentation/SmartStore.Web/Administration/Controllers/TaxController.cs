using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tax;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Security;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class TaxController : AdminControllerBase
    {
        private readonly ITaxService _taxService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly TaxSettings _taxSettings;
        private readonly ISettingService _settingService;
        private readonly PluginMediator _pluginMediator;

        public TaxController(
            ITaxService taxService,
            ITaxCategoryService taxCategoryService,
            TaxSettings taxSettings,
            ISettingService settingService,
            PluginMediator pluginMediator)
        {
            _taxService = taxService;
            _taxCategoryService = taxCategoryService;
            _taxSettings = taxSettings;
            _settingService = settingService;
            _pluginMediator = pluginMediator;
        }

        #region Tax Providers

        [Permission(Permissions.Configuration.Tax.Read)]
        public ActionResult Providers()
        {
            var taxProviderModels = _taxService.LoadAllTaxProviders()
                .Select(x =>
                {
                    var model = _pluginMediator.ToProviderModel<ITaxProvider, TaxProviderModel>(x);
                    if (x.Metadata.SystemName.Equals(_taxSettings.ActiveTaxProviderSystemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        model.IsPrimaryTaxProvider = true;
                    }
                    else
                    {
                        _pluginMediator.ActivateDependentWidgets(x.Metadata, false);
                    }

                    return model;
                })
                .ToList();

            return View(taxProviderModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Tax.Activate)]
        public ActionResult ActivateProvider(string systemName)
        {
            if (systemName.HasValue())
            {
                var taxProvider = _taxService.LoadTaxProviderBySystemName(systemName);
                if (taxProvider != null)
                {
                    _taxSettings.ActiveTaxProviderSystemName = systemName;
                    _settingService.SaveSetting(_taxSettings);
                    _pluginMediator.ActivateDependentWidgets(taxProvider.Metadata, true);
                }
            }

            return RedirectToAction("Providers");
        }

        #endregion

        #region Tax categories

        [Permission(Permissions.Configuration.Tax.Read)]
        public ActionResult Categories()
        {
            var categoriesModel = _taxCategoryService.GetAllTaxCategories()
                .Select(x => x.ToModel())
                .ToList();

            var model = new GridModel<TaxCategoryModel>
            {
                Data = categoriesModel,
                Total = categoriesModel.Count
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Tax.Read)]
        public ActionResult Categories(GridCommand command)
        {
            var model = new GridModel<TaxCategoryModel>();
            var categoriesModel = _taxCategoryService.GetAllTaxCategories()
                .Select(x => x.ToModel())
                .ForCommand(command)
                .ToList();

            model.Data = categoriesModel;
            model.Total = categoriesModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Tax.Update)]
        public ActionResult CategoryUpdate(TaxCategoryModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var taxCategory = _taxCategoryService.GetTaxCategoryById(model.Id);
            taxCategory = model.ToEntity(taxCategory);

            _taxCategoryService.UpdateTaxCategory(taxCategory);

            return Categories(command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Tax.Create)]
        public ActionResult CategoryAdd([Bind(Exclude = "Id")] TaxCategoryModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var taxCategory = new TaxCategory();
            taxCategory = model.ToEntity(taxCategory);

            _taxCategoryService.InsertTaxCategory(taxCategory);

            return Categories(command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Tax.Delete)]
        public ActionResult CategoryDelete(int id, GridCommand command)
        {
            var taxCategory = _taxCategoryService.GetTaxCategoryById(id);

            _taxCategoryService.DeleteTaxCategory(taxCategory);

            return Categories(command);
        }

        #endregion
    }
}
