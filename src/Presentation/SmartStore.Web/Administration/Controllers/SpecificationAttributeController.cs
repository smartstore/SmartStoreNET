using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
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
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly AdminAreaSettings _adminAreaSettings;

        public SpecificationAttributeController(
            ISpecificationAttributeService specificationAttributeService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            ICustomerActivityService customerActivityService,
            AdminAreaSettings adminAreaSettings)
        {
            _specificationAttributeService = specificationAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _customerActivityService = customerActivityService;
            _adminAreaSettings = adminAreaSettings;
        }

        #region Utilities

        private void UpdateAttributeLocales(SpecificationAttribute specificationAttribute, SpecificationAttributeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(specificationAttribute, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(specificationAttribute, x => x.Alias, localized.Alias, localized.LanguageId);
            }
        }

        private void UpdateOptionLocales(SpecificationAttributeOption specificationAttributeOption, SpecificationAttributeOptionModel model)
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

            for (var i = 0; i < values.Length; ++i)
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return false;
                }

                try
                {
                    // Save localized properties.
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
                catch { }
            }

            return true;
        }

        #endregion

        #region Specification attributes

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Catalog.Attribute.Read)]
        public ActionResult List()
        {
            ViewData["GridPageSize"] = _adminAreaSettings.GridPageSize;

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Attribute.Read)]
        public ActionResult List(GridCommand command)
        {
            var gridModel = new GridModel<SpecificationAttributeModel>();

            var data = _specificationAttributeService.GetSpecificationAttributes()
                .ForCommand(command)
                .Select(x => x.ToModel())
                .ToList();

            gridModel.Data = data.PagedForCommand(command);
            gridModel.Total = data.Count;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Catalog.Attribute.Create)]
        public ActionResult Create()
        {
            var model = new SpecificationAttributeModel();

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.Create)]
        public ActionResult Create(SpecificationAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var specificationAttribute = model.ToEntity();

                try
                {
                    _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                try
                {
                    UpdateAttributeLocales(specificationAttribute, model);
                }
                catch (Exception ex)
                {
                    continueEditing = true;
                    NotifyError(ex.Message);
                }

                _customerActivityService.InsertActivity("AddNewSpecAttribute", T("ActivityLog.AddNewSpecAttribute"), specificationAttribute.Name);

                NotifySuccess(T("Admin.Catalog.Attributes.SpecificationAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = specificationAttribute.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [Permission(Permissions.Catalog.Attribute.Read)]
        public ActionResult Edit(int id)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
            if (specificationAttribute == null)
            {
                return RedirectToAction("List");
            }

            var model = specificationAttribute.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = specificationAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.Alias = specificationAttribute.GetLocalized(x => x.Alias, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.Update)]
        public ActionResult Edit(SpecificationAttributeModel model, bool continueEditing)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.Id);
            if (specificationAttribute == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                specificationAttribute = model.ToEntity(specificationAttribute);

                try
                {
                    _specificationAttributeService.UpdateSpecificationAttribute(specificationAttribute);

                    UpdateAttributeLocales(specificationAttribute, model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                _customerActivityService.InsertActivity("EditSpecAttribute", T("ActivityLog.EditSpecAttribute"), specificationAttribute.Name);

                NotifySuccess(T("Admin.Catalog.Attributes.SpecificationAttributes.Updated"));
                return continueEditing ? RedirectToAction("Edit", specificationAttribute.Id) : RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(id);
            if (specificationAttribute == null)
            {
                return RedirectToAction("List");
            }

            _specificationAttributeService.DeleteSpecificationAttribute(specificationAttribute);

            _customerActivityService.InsertActivity("DeleteSpecAttribute", T("ActivityLog.DeleteSpecAttribute"), specificationAttribute.Name);

            NotifySuccess(T("Admin.Catalog.Attributes.SpecificationAttributes.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Count > 0)
            {
                var attributes = _specificationAttributeService.GetSpecificationAttributesByIds(selectedIds.ToArray()).ToList();
                var deletedNames = string.Join(", ", attributes.Select(x => x.Name));

                attributes.Each(x => _specificationAttributeService.DeleteSpecificationAttribute(x));

                _customerActivityService.InsertActivity("DeleteSpecAttribute", T("ActivityLog.DeleteSpecAttribute"), deletedNames);
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Specification attribute options

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Attribute.Read)]
        public ActionResult OptionList(int specificationAttributeId, GridCommand command)
        {
            var gridModel = new GridModel<SpecificationAttributeOptionModel>();
            var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(specificationAttributeId);

            gridModel.Data = options.Select(x =>
            {
                var model = x.ToModel();
                model.NameString = Server.HtmlEncode(x.Color.IsEmpty() ? x.Name : $"{x.Name} - {x.Color}");

                return model;
            });
            gridModel.Total = options.Count();

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Catalog.Attribute.EditOption)]
        public ActionResult OptionCreatePopup(int specificationAttributeId)
        {
            var model = new SpecificationAttributeOptionModel
            {
                SpecificationAttributeId = specificationAttributeId,
                Color = "",
            };

            AddLocales(_languageService, model.Locales);

            ViewBag.MultipleEnabled = true;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.EditOption)]
        public ActionResult OptionCreatePopup(string btnId, string formId, SpecificationAttributeOptionModel model)
        {
            var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(model.SpecificationAttributeId);
            if (specificationAttribute == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                if (model.Multiple)
                {
                    if (!AddMultipleOptionNames(model))
                    {
                        return View(model);
                    }
                }
                else
                {
                    var sao = model.ToEntity();

                    try
                    {
                        _specificationAttributeService.InsertSpecificationAttributeOption(sao);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View(model);
                    }

                    try
                    {
                        UpdateOptionLocales(sao, model);
                    }
                    catch { }
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        [Permission(Permissions.Catalog.Attribute.Read)]
        public ActionResult OptionEditPopup(int id)
        {
            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(id);
            if (sao == null)
            {
                return RedirectToAction("List");
            }

            var model = sao.ToModel();
            model.NameString = Server.HtmlEncode(sao.Color.IsEmpty() ? sao.Name : $"{sao.Name} - {sao.Color}");

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sao.GetLocalized(x => x.Name, languageId, false, false);
                locale.Alias = sao.GetLocalized(x => x.Alias, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.EditOption)]
        public ActionResult OptionEditPopup(string btnId, string formId, SpecificationAttributeOptionModel model)
        {
            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(model.Id);
            if (sao == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                sao = model.ToEntity(sao);

                try
                {
                    _specificationAttributeService.UpdateSpecificationAttributeOption(sao);

                    UpdateOptionLocales(sao, model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Attribute.EditOption)]
        public ActionResult OptionDelete(int optionId, int specificationAttributeId, GridCommand command)
        {
            var sao = _specificationAttributeService.GetSpecificationAttributeOptionById(optionId);

            _specificationAttributeService.DeleteSpecificationAttributeOption(sao);

            return OptionList(specificationAttributeId, command);
        }

        // Ajax.
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetOptionsByAttributeId(int attributeId)
        {
            var options = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(attributeId);
            var result =
                from o in options
                select new { id = o.Id, name = o.Name, text = o.Name };

            return Json(result.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Attribute.Update)]
        public ActionResult SetAttributeValue(string pk, string value, string name, FormCollection form)
        {
            var success = false;
            var message = string.Empty;

            // name is the entity id of product specification attribute mapping.
            var attribute = _specificationAttributeService.GetProductSpecificationAttributeById(Convert.ToInt32(name));

            try
            {
                attribute.SpecificationAttributeOptionId = Convert.ToInt32(value);

                _specificationAttributeService.UpdateProductSpecificationAttribute(attribute);
                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            // we give back the name to xeditable to overwrite the grid data in success event when a grid element got updated.
            return Json(new { success, message, name = attribute.SpecificationAttributeOption?.Name });
        }

        #endregion
    }
}
