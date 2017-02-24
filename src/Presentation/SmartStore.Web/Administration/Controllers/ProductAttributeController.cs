using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class ProductAttributeController : AdminControllerBase
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
		private readonly AdminAreaSettings _adminAreaSettings;

		#endregion Fields

		#region Constructors

		public ProductAttributeController(
			IProductAttributeService productAttributeService,
            ILanguageService languageService,
			ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
			ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
			AdminAreaSettings adminAreaSettings)
        {
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _permissionService = permissionService;
			_adminAreaSettings = adminAreaSettings;
        }

        #endregion
        
        #region Utilities

        private void UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Name, localized.Name, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Alias, localized.Alias, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Description, localized.Description, localized.LanguageId);
            }
        }

        #endregion
        
        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			ViewData["GridPageSize"] = _adminAreaSettings.GridPageSize;

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var gridModel = new GridModel<ProductAttributeModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var productAttributes = _productAttributeService.GetAllProductAttributes();

				var data = productAttributes
					.ForCommand(command)
					.Select(x => x.ToModel())
					.ToList();

				gridModel.Data = data.PagedForCommand(command);
				gridModel.Total = data.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ProductAttributeModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new ProductAttributeModel();
			model.AllowFiltering = true;

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var productAttribute = model.ToEntity();

				try
				{
					_productAttributeService.InsertProductAttribute(productAttribute);

					UpdateLocales(productAttribute, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return Create();
				}

				// activity log
				_customerActivityService.InsertActivity("AddNewProductAttribute", _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = productAttribute.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                return RedirectToAction("List");

            var model = productAttribute.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productAttribute.GetLocalized(x => x.Name, languageId, false, false);
				locale.Alias = productAttribute.GetLocalized(x => x.Alias, languageId, false, false);
				locale.Description = productAttribute.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ProductAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
            if (productAttribute == null)
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                productAttribute = model.ToEntity(productAttribute);

				try
				{
					_productAttributeService.UpdateProductAttribute(productAttribute);

					UpdateLocales(productAttribute, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return Edit(productAttribute.Id);
				}

				// activity log
				_customerActivityService.InsertActivity("EditProductAttribute", _localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", productAttribute.Id) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productAttribute = _productAttributeService.GetProductAttributeById(id);
            if (productAttribute == null)
                return RedirectToAction("List");

            _productAttributeService.DeleteProductAttribute(productAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductAttribute", _localizationService.GetResource("ActivityLog.DeleteProductAttribute"), productAttribute.Name);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
