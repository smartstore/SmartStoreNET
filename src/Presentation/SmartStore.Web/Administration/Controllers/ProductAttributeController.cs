using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
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
				model.NameString = Server.HtmlEncode(option.Color.IsEmpty() ? option.Name : $"{option.Name} - {option.Color}");
				model.PriceAdjustmentString = (option.ValueType == ProductVariantAttributeValueType.Simple ? option.PriceAdjustment.ToString("G29") : "");
				model.WeightAdjustmentString = (option.ValueType == ProductVariantAttributeValueType.Simple ? option.WeightAdjustment.ToString("G29") : "");
				model.TypeName = option.ValueType.GetLocalizedEnum(Services.Localization, Services.WorkContext);
				model.TypeNameClass = (option.ValueType == ProductVariantAttributeValueType.ProductLinkage ? "fa fa-link mr-2" : "d-none hide hidden-xs-up");

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
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

				try
				{
					UpdateLocales(productAttribute, model);
				}
				catch (Exception)
				{
					// TODO: what?
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
					return View(model);
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

		#region Product attribute options sets

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult OptionsSetList(int productAttributeId, GridCommand command)
		{
			var gridModel = new GridModel<ProductAttributeOptionsSetModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var optionsSets = _productAttributeService.GetProductAttributeOptionsSetsByAttributeId(productAttributeId);

				gridModel.Total = optionsSets.Count();
				gridModel.Data = optionsSets.Select(x =>
				{
					return new ProductAttributeOptionsSetModel
					{
						Id = x.Id,
						ProductAttributeId = productAttributeId,
						Name = x.Name
					};
				});
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ProductAttributeOptionsSetModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult OptionsSetListDetails(int id)
		{
			var gridModel = new GridModel<ProductAttributeOptionModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var options = _productAttributeService.GetProductAttributeOptionsByOptionsSetId(id);

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

		[GridAction(EnableCustomBinding = true)]
		public ActionResult OptionsSetInsert(ProductAttributeOptionsSetModel model, GridCommand command)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var entity = new ProductAttributeOptionsSet
				{
					Name = model.Name,
					ProductAttributeId = model.ProductAttributeId
				};

				_productAttributeService.InsertProductAttributeOptionsSet(entity);
			}
			else
			{
				NotifyAccessDenied();
			}

			return OptionsSetList(model.ProductAttributeId, command);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult OptionsSetUpdate(ProductAttributeOptionsSetModel model, GridCommand command)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var entity = _productAttributeService.GetProductAttributeOptionsSetById(model.Id);
				entity.Name = model.Name;

				_productAttributeService.UpdateProductAttributeOptionsSet(entity);
			}
			else
			{
				NotifyAccessDenied();
			}

			return OptionsSetList(model.ProductAttributeId, command);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult OptionsSetDelete(int id, int productAttributeId, GridCommand command)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var entity = _productAttributeService.GetProductAttributeOptionsSetById(id);

				_productAttributeService.DeleteProductAttributeOptionsSet(entity);
			}

			return OptionsSetList(productAttributeId, command);
		}

		#endregion

		#region Product attribute options

		public ActionResult OptionCreatePopup(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var optionsSet = _productAttributeService.GetProductAttributeOptionsSetById(id);
			if (optionsSet == null)
				return RedirectToAction("List");

			var model = new ProductAttributeOptionModel
			{
				ProductAttributeOptionsSetId = id,
				Color = string.Empty,
				Quantity = 1
			};

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

				MediaHelper.UpdatePictureTransientStateFor(entity, m => m.PictureId);

				try
				{
					_productAttributeService.InsertProductAttributeOption(entity);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

				try
				{
					UpdateOptionLocales(entity, model);
				}
				catch (Exception)
				{
					// TODO: what?
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

			var entity = _productAttributeService.GetProductAttributeOptionById(model.Id);
			if (entity == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				entity = model.ToEntity(entity);
				entity.LinkedProductId = entity.ValueType == ProductVariantAttributeValueType.Simple ? 0 : model.LinkedProductId;

				MediaHelper.UpdatePictureTransientStateFor(entity, m => m.PictureId);

				try
				{
					_productAttributeService.UpdateProductAttributeOption(entity);

					UpdateOptionLocales(entity, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

				ViewBag.RefreshPage = true;
				ViewBag.btnId = btnId;
				ViewBag.formId = formId;
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult OptionDelete(int id)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var entity = _productAttributeService.GetProductAttributeOptionById(id);

				_productAttributeService.DeleteProductAttributeOption(entity);
			}

			return new EmptyResult();
		}

		#endregion
	}
}
