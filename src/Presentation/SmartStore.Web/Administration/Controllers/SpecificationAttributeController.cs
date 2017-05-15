using System;
using System.Collections.Generic;
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
    public class SpecificationAttributeController : AdminControllerBase
    {
        #region Fields

        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
		private readonly AdminAreaSettings _adminAreaSettings;

        #endregion Fields

        #region Constructors

        public SpecificationAttributeController(ISpecificationAttributeService specificationAttributeService,
            ILanguageService languageService, ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService, ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
			AdminAreaSettings adminAreaSettings)
        {
            this._specificationAttributeService = specificationAttributeService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
			this._adminAreaSettings = adminAreaSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateAttributeLocales(SpecificationAttribute specificationAttribute, SpecificationAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(specificationAttribute, x => x.Name, localized.Name, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(specificationAttribute, x => x.Alias, localized.Alias, localized.LanguageId);
			}
        }

        [NonAction]
        public void UpdateOptionLocales(SpecificationAttributeOption specificationAttributeOption, SpecificationAttributeOptionModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(specificationAttributeOption, x => x.Name, localized.Name, localized.LanguageId);
				_localizedEntityService.SaveLocalizedValue(specificationAttributeOption, x => x.Alias, localized.Alias, localized.LanguageId);
			}
        }

		private bool AddMultipleOptionNames(SpecificationAttributeOptionModel model)
		{
			var values = model.Name.SplitSafe(";");
			var alias = model.Alias.SplitSafe(";");
			var order = model.DisplayOrder;

			for (int i = 0; i < values.Length; ++i)
			{
				var sao = new SpecificationAttributeOption
				{
					Name = values.SafeGet(i).Trim(),
					Alias = alias.SafeGet(i).Trim(),
					DisplayOrder = order++,
					SpecificationAttributeId = model.SpecificationAttributeId
				};

				try
				{
					_specificationAttributeService.InsertSpecificationAttributeOption(sao);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return false;
				}

				try
				{
					// save localized properties
					foreach (var localized in model.Locales.Where(l => l.Name.HasValue()))
					{
						var localizedValues = localized.Name.SplitSafe(";");
						var value = (i < localizedValues.Length ? localizedValues[i].Trim() : sao.Name);

						_localizedEntityService.SaveLocalizedValue(sao, x => x.Name, value, localized.LanguageId);
					}

					foreach (var localized in model.Locales.Where(l => l.Alias.HasValue()))
					{
						var localizedAlias = localized.Alias.SplitSafe(";");
						var value = localizedAlias.SafeGet(i).Trim();

						if (value.HasValue())
						{
							_localizedEntityService.SaveLocalizedValue(sao, x => x.Alias, value, localized.LanguageId);
						}
					}
				}
				catch (Exception)
				{
					// TODO: what?
				}
			}

			return true;
		}

        #endregion
        
        #region Specification attributes

        //list
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
			var gridModel = new GridModel<SpecificationAttributeModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var data = _specificationAttributeService.GetSpecificationAttributes()
					.Expand(x => x.SpecificationAttributeOptions)
					.ForCommand(command)
					.Select(x =>
					{
						var model = x.ToModel();
						model.OptionCount = x.SpecificationAttributeOptions.Count;

						return model;
					})
					.ToList();

				gridModel.Data = data.PagedForCommand(command);
				gridModel.Total = data.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<SpecificationAttributeModel>();

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

            var model = new SpecificationAttributeModel();

			AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(SpecificationAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var specificationAttribute = model.ToEntity();

				try
				{
					_specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

				try
				{
					UpdateAttributeLocales(specificationAttribute, model);
				}
				catch (Exception exception)
				{
					continueEditing = true;
					NotifyError(exception.Message);
				}

				//activity log
				_customerActivityService.InsertActivity("AddNewSpecAttribute", _localizationService.GetResource("ActivityLog.AddNewSpecAttribute"), specificationAttribute.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = specificationAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            var model = specificationAttribute.ToModel();

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = specificationAttribute.GetLocalized(x => x.Name, languageId, false, false);
				locale.Alias = specificationAttribute.GetLocalized(x => x.Alias, languageId, false, false);
			});

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(SpecificationAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                specificationAttribute = model.ToEntity(specificationAttribute);

				try
				{
					_specificationAttributeService.UpdateSpecificationAttribute(specificationAttribute);

					UpdateAttributeLocales(specificationAttribute, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

                //activity log
                _customerActivityService.InsertActivity("EditSpecAttribute", _localizationService.GetResource("ActivityLog.EditSpecAttribute"), specificationAttribute.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", specificationAttribute.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            _specificationAttributeService.DeleteSpecificationAttribute(specificationAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteSpecAttribute", _localizationService.GetResource("ActivityLog.DeleteSpecAttribute"), specificationAttribute.Name);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Deleted"));
            return RedirectToAction("List");
        }

		[HttpPost]
		public ActionResult DeleteSelected(ICollection<int> selectedIds)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			if (selectedIds != null && selectedIds.Count > 0)
			{
				var attributes = _specificationAttributeService.GetSpecificationAttributesByIds(selectedIds.ToArray()).ToList();
				var deletedNames = string.Join(", ", attributes.Select(x => x.Name));

				attributes.Each(x => _specificationAttributeService.DeleteSpecificationAttribute(x));

				_customerActivityService.InsertActivity("DeleteSpecAttribute", _localizationService.GetResource("ActivityLog.DeleteSpecAttribute"), deletedNames);
			}

			return Json(new { Result = true });
		}

        #endregion

        #region Specification attribute options

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult OptionList(int specificationAttributeId, GridCommand command)
        {
			var gridModel = new GridModel<SpecificationAttributeOptionModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(specificationAttributeId);

				gridModel.Data = options.Select(x => x.ToModel());
				gridModel.Total = options.Count();
			}
			else
			{
				gridModel.Data = Enumerable.Empty<SpecificationAttributeOptionModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult OptionCreatePopup(int specificationAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new SpecificationAttributeOptionModel();
            model.SpecificationAttributeId = specificationAttributeId;
            //locales
            AddLocales(_languageService, model.Locales);

			ViewBag.MultipleEnabled = true;

            return View(model);
        }

        [HttpPost]
        public ActionResult OptionCreatePopup(string btnId, string formId, SpecificationAttributeOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.SpecificationAttributeId);
            if (specificationAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
				if (model.Multiple)
				{
					if (!AddMultipleOptionNames(model))
						return View(model);
				}
				else
				{
					var sao = model.ToEntity();

					try
					{
						_specificationAttributeService.InsertSpecificationAttributeOption(sao);
					}
					catch (Exception exception)
					{
						ModelState.AddModelError("", exception.Message);
						return View(model);
					}

					try
					{
						UpdateOptionLocales(sao, model);
					}
					catch (Exception)
					{
						// TODO: what?
					}
				}

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult OptionEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(id);
            if (sao == null)
                return RedirectToAction("List");

            var model = sao.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sao.GetLocalized(x => x.Name, languageId, false, false);
				locale.Alias = sao.GetLocalized(x => x.Alias, languageId, false, false);
			});

            return View(model);
        }

        [HttpPost]
        public ActionResult OptionEditPopup(string btnId, string formId, SpecificationAttributeOptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(model.Id);
            if (sao == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                sao = model.ToEntity(sao);

				try
				{
					_specificationAttributeService.UpdateSpecificationAttributeOption(sao);

					UpdateOptionLocales(sao, model);
				}
				catch (Exception exception)
				{
					ModelState.AddModelError("", exception.Message);
					return View(model);
				}

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult OptionDelete(int optionId, int specificationAttributeId, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(optionId);

				_specificationAttributeService.DeleteSpecificationAttributeOption(sao);
			}

            return OptionList(specificationAttributeId, command);
        }

        //ajax
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOptionsByAttributeId(string attributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            // This action method gets called via an ajax request
            if (string.IsNullOrEmpty(attributeId))
                throw new ArgumentNullException("attributeId");

            var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(Convert.ToInt32(attributeId));
            var result = 
				from o in options
				select new { id = o.Id, name = o.Name, text = o.Name };

            return Json(result.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetAttributeValue(string pk, string value, string name, FormCollection form)
        {
			var success = false;
			var message = string.Empty;

			// name is the entity id of product specification attribute mapping
			var attribute = _specificationAttributeService.GetProductSpecificationAttributeById(Convert.ToInt32(name));

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				try
				{
					attribute.SpecificationAttributeOptionId = Convert.ToInt32(value);

					_specificationAttributeService.UpdateProductSpecificationAttribute(attribute);
					success = true;
				}
				catch (Exception exception)
				{
					message = exception.Message;
				}
			}
			else
			{
				NotifyAccessDenied();
			}

			// we give back the name to xeditable to overwrite the grid data in success event when a grid element got updated
			return Json(new { success = success, message = message, name = attribute.SpecificationAttributeOption?.Name });
        }

        #endregion
    }
}
