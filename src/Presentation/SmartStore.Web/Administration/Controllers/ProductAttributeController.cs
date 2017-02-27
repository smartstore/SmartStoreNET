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

		private readonly IProductService _productService;
		private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
		private readonly AdminAreaSettings _adminAreaSettings;

		#endregion Fields

		#region Constructors

		public ProductAttributeController(
			IProductService productService,
			IProductAttributeService productAttributeService,
            ILanguageService languageService,
			ILocalizedEntityService localizedEntityService,
			ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
			AdminAreaSettings adminAreaSettings)
        {
			_productService = productService;
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _customerActivityService = customerActivityService;
            _permissionService = permissionService;
			_adminAreaSettings = adminAreaSettings;
        }

		#endregion

		#region Utilities

		private void PrepareProductAttributeOptionModel(ProductAttributeOptionModel model, ProductAttributeOption option)
		{
			// TODO: DRY, similar code in ProductController (ProductAttributeValueList, ProductAttributeValueEditPopup...)
			if (option != null)
			{
				model.Name = option.ColorSquaresRgb.IsEmpty() ? option.Name : $"{option.Name} - {option.ColorSquaresRgb}";
				model.PriceAdjustmentString = (option.ValueType == ProductVariantAttributeValueType.Simple ? option.PriceAdjustment.ToString("G29") : "");
				model.WeightAdjustmentString = (option.ValueType == ProductVariantAttributeValueType.Simple ? option.WeightAdjustment.ToString("G29") : "");
				model.TypeName = option.ValueType.GetLocalizedEnum(Services.Localization, Services.WorkContext);
				model.TypeNameClass = (option.ValueType == ProductVariantAttributeValueType.ProductLinkage ? "fa fa-link mr8" : "hide hidden-xs-up");

				var linkedProduct = _productService.GetProductById(option.LinkedProductId);
				if (linkedProduct != null)
				{
					model.LinkedProductName = linkedProduct.GetLocalized(p => p.Name);
					model.LinkedProductTypeName = linkedProduct.GetProductTypeLabel(Services.Localization);
					model.LinkedProductTypeLabelHint = linkedProduct.ProductTypeLabelHint;

					if (model.Quantity > 1)
						model.QuantityInfo = $" × {model.Quantity}";
				}

				AddLocales(_languageService, model.Locales, (locale, languageId) =>
				{
					locale.Name = option.GetLocalized(x => x.Name, languageId, false, false);
					locale.Alias = option.GetLocalized(x => x.Alias, languageId, false, false);
				});
			}
		}

		private void UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Name, localized.Name, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Alias, localized.Alias, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(productAttribute, x => x.Description, localized.Description, localized.LanguageId);
            }
        }

		private void UpdateOptionLocales(ProductAttributeOption productAttributeOption, ProductAttributeOptionModel model)
		{
			foreach (var localized in model.Locales)
			{
				_localizedEntityService.SaveLocalizedValue(productAttributeOption, x => x.Name, localized.Name, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(productAttributeOption, x => x.Alias, localized.Alias, localized.LanguageId);
			}
		}

		#endregion

		#region Product attribute

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
					.Select(x =>
					{
						var model = x.ToModel();
						model.OptionCount = x.ProductAttributeOptions.Count;

						return model;
					})
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
				_customerActivityService.InsertActivity("AddNewProductAttribute", T("ActivityLog.AddNewProductAttribute", productAttribute.Name));

                NotifySuccess(T("Admin.Catalog.Attributes.ProductAttributes.Added"));
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
				_customerActivityService.InsertActivity("EditProductAttribute", T("ActivityLog.EditProductAttribute", productAttribute.Name));

                NotifySuccess(T("Admin.Catalog.Attributes.ProductAttributes.Updated"));
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
            _customerActivityService.InsertActivity("DeleteProductAttribute", T("ActivityLog.DeleteProductAttribute", productAttribute.Name));

            NotifySuccess(T("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
            return RedirectToAction("List");
        }

		#endregion

		#region Product attribute options

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult OptionList(int productAttributeId, GridCommand command)
		{
			var gridModel = new GridModel<ProductAttributeOptionModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var options = _productAttributeService.GetProductAttributeOptionByAttributeId(productAttributeId);

				gridModel.Total = options.Count();
				gridModel.Data = options.Select(x =>
				{
					var model = x.ToModel();
					PrepareProductAttributeOptionModel(model, x);
					return model;
				});
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ProductAttributeOptionModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = gridModel
			};
		}

		public ActionResult OptionCreatePopup(int productAttributeId)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var model = new ProductAttributeOptionModel();
			model.ProductAttributeId = productAttributeId;

			PrepareProductAttributeOptionModel(model, null);
			AddLocales(_languageService, model.Locales);

			return View(model);
		}

		[HttpPost]
		public ActionResult OptionCreatePopup(string btnId, string formId, ProductAttributeOptionModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			if (ModelState.IsValid)
			{
				var entity = model.ToEntity();

				try
				{
					_productAttributeService.InsertProductAttributeOption(entity);

					UpdateOptionLocales(entity, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return OptionCreatePopup(model.ProductAttributeId);
				}

				ViewBag.RefreshPage = true;
				ViewBag.btnId = btnId;
				ViewBag.formId = formId;
			}

			return View(model);
		}

		public ActionResult OptionEditPopup(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var option = _productAttributeService.GetProductAttributeOptionById(id);
			if (option == null)
				return RedirectToAction("List");

			var model = option.ToModel();
			PrepareProductAttributeOptionModel(model, option);

			return View(model);
		}

		[HttpPost]
		public ActionResult OptionEditPopup(string btnId, string formId, ProductAttributeOptionModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var option = _productAttributeService.GetProductAttributeOptionById(model.Id);
			if (option == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				option = model.ToEntity(option);

				try
				{
					_productAttributeService.UpdateProductAttributeOption(option);

					UpdateOptionLocales(option, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return OptionEditPopup(option.Id);
				}

				ViewBag.RefreshPage = true;
				ViewBag.btnId = btnId;
				ViewBag.formId = formId;
			}

			return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult OptionDelete(int optionId, int productAttributeId, GridCommand command)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var entity = _productAttributeService.GetProductAttributeOptionById(optionId);

				_productAttributeService.DeleteProductAttributeOption(entity);
			}

			return OptionList(productAttributeId, command);
		}

		#endregion
	}
}
