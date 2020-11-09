using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class QuantityUnitController : AdminControllerBase
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

        // AJAX.
        public ActionResult AllQuantityUnits(string label, int selectedId)
        {
            var quantityUnits = _quantityUnitService.GetAllQuantityUnits();
            if (label.HasValue())
            {
                quantityUnits.Insert(0, new QuantityUnit { Name = label, Id = 0 });
            }

            var list =
                from m in quantityUnits
                select new ChoiceListItem
                {
                    Id = m.Id.ToString(),
                    Text = m.GetLocalized(x => x.Name).Value,
                    Selected = m.Id == selectedId
                };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Measure.Read)]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Measure.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<QuantityUnitModel>();

            var quantityUnitModel = _quantityUnitService.GetAllQuantityUnits()
                .Select(x => x.ToModel())
                .ToList();

            model.Data = quantityUnitModel;
            model.Total = quantityUnitModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Configuration.Measure.Create)]
        public ActionResult CreateQuantityUnitPopup()
        {
            var model = new QuantityUnitModel();
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Measure.Create)]
        public ActionResult CreateQuantityUnitPopup(string btnId, QuantityUnitModel model)
        {
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

        [Permission(Permissions.Configuration.Measure.Read)]
        public ActionResult EditQuantityUnitPopup(int id)
        {
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Measure.Update)]
        public ActionResult EditQuantityUnitPopup(string btnId, QuantityUnitModel model)
        {
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
        [Permission(Permissions.Configuration.Measure.Delete)]
        public ActionResult DeleteQuantityUnit(int id, GridCommand command)
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
