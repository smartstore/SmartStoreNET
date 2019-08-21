using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class QuantityUnitController :  AdminControllerBase
    {
        private readonly IQuantityUnitService _quantityUnitService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;

        public QuantityUnitController(
			IQuantityUnitService quantityUnitService,
            ILocalizedEntityService localizedEntityService, 
            ILanguageService languageService)
        {
            _quantityUnitService = quantityUnitService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedView();
            }

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<QuantityUnitModel>();

            if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                var quantityUnitModel = _quantityUnitService.GetAllQuantityUnits()
                    .Select(x => x.ToModel())
                    .ToList();

                model.Data = quantityUnitModel;
                model.Total = quantityUnitModel.Count;
            }
            else
            {
                model.Data = Enumerable.Empty<QuantityUnitModel>();

                NotifyAccessDenied();
            }

            return new JsonResult
            {
                Data = model
            };
        }

        // Ajax.
        public ActionResult AllQuantityUnits(string label, int selectedId)
        {
            var quantityUnits = _quantityUnitService.GetAllQuantityUnits();
            if (label.HasValue())
            {
                quantityUnits.Insert(0, new QuantityUnit { Name = label, Id = 0 });
            }

            var list = from m in quantityUnits
                       select new
                       {
                           id = m.Id.ToString(),
                           text = m.Name,
                           selected = m.Id == selectedId
                       };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult CreateQuantityUnitPopup()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var model = new QuantityUnitModel();
            AddLocales(_languageService, model.Locales);
            
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateQuantityUnitPopup(string btnId, QuantityUnitModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var entity = model.ToEntity();

                    _quantityUnitService.InsertQuantityUnit(entity);

                    UpdateLocales(entity, model);

                    NotifySuccess(T("Admin.Configuration.QuantityUnit.Added"));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
            }

            return View(model);
        }

        public ActionResult EditQuantityUnitPopup(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _quantityUnitService.GetQuantityUnitById(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = entity.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
                locale.NamePlural = entity.GetLocalized(x => x.NamePlural, languageId, false, false);
                locale.Description = entity.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditQuantityUnitPopup(string btnId, QuantityUnitModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _quantityUnitService.GetQuantityUnitById(model.Id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    entity = model.ToEntity(entity);

                    _quantityUnitService.UpdateQuantityUnit(entity);

                    UpdateLocales(entity, model);

                    NotifySuccess(T("Admin.Configuration.QuantityUnits.Updated"));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
            }

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DeleteQuantityUnit(int id, GridCommand command)
        {
            if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                var entity = _quantityUnitService.GetQuantityUnitById(id);

                try
                {
                    _quantityUnitService.DeleteQuantityUnit(entity);

                    NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
                }
                catch (Exception ex)
                {
                    NotifyError(ex);
                }
            }

            return List(command);
        }

        private void UpdateLocales(QuantityUnit quantityUnit, QuantityUnitModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(quantityUnit, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(quantityUnit, x => x.NamePlural, localized.NamePlural, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(quantityUnit, x => x.Description, localized.Description, localized.LanguageId);
            }
        }
    }
}
