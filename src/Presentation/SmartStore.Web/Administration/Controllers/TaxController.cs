using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Tax;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
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
		#region Fields

        private readonly ITaxService _taxService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly TaxSettings _taxSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
		private readonly ILocalizationService _localizationService;
		private readonly PluginMediator _pluginMediator;

	    #endregion

		#region Constructors

        public TaxController(
			ITaxService taxService,
            ITaxCategoryService taxCategoryService, 
			TaxSettings taxSettings,
            ISettingService settingService, 
			IPermissionService permissionService,
			ILocalizationService localizationService,
			PluginMediator pluginMediator)
		{
            this._taxService = taxService;
            this._taxCategoryService = taxCategoryService;
            this._taxSettings = taxSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
			this._localizationService = localizationService;
			this._pluginMediator = pluginMediator;
		}

		#endregion 

        #region Tax Providers

        public ActionResult Providers(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            // mark as active tax provider (if selected)
            if (!String.IsNullOrEmpty(systemName))
            {
                var taxProvider = _taxService.LoadTaxProviderBySystemName(systemName);
                if (taxProvider != null)
                {
                    _taxSettings.ActiveTaxProviderSystemName = systemName;
                    _settingService.SaveSetting(_taxSettings);
					_pluginMediator.ActivateDependentWidgets(taxProvider.Metadata, true);
                }
            }
			
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

        #endregion

        #region Tax categories

        public ActionResult Categories()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

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
        public ActionResult Categories(GridCommand command)
        {
			var model = new GridModel<TaxCategoryModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
			{
				var categoriesModel = _taxCategoryService.GetAllTaxCategories()
					.Select(x => x.ToModel())
					.ForCommand(command)
					.ToList();

				model.Data = categoriesModel;
				model.Total = categoriesModel.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<TaxCategoryModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CategoryUpdate(TaxCategoryModel model, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var taxCategory = _taxCategoryService.GetTaxCategoryById(model.Id);
				taxCategory = model.ToEntity(taxCategory);

				_taxCategoryService.UpdateTaxCategory(taxCategory);
			}

            return Categories(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CategoryAdd([Bind(Exclude = "Id")] TaxCategoryModel model, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var taxCategory = new TaxCategory();
				taxCategory = model.ToEntity(taxCategory);

				_taxCategoryService.InsertTaxCategory(taxCategory);
			}

            return Categories(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CategoryDelete(int id, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
			{
				var taxCategory = _taxCategoryService.GetTaxCategoryById(id);

				_taxCategoryService.DeleteTaxCategory(taxCategory);
			}

            return Categories(command);
        }

        #endregion
    }
}
