using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class QuantityUnitController :  AdminControllerBase
    {
        #region Fields

        private readonly IQuantityUnitService _quantityUnitService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
		private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public QuantityUnitController(
			IQuantityUnitService quantityUnitService,
            ILocalizedEntityService localizedEntityService, 
            ILanguageService languageService,
			ICommonServices services)
        {
            _quantityUnitService = quantityUnitService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
			_services = services;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        public void UpdateLocales(QuantityUnit quantityUnit, QuantityUnitModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(quantityUnit, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(quantityUnit, x => x.Description, localized.Description, localized.LanguageId);
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var quantityUnitModel = _quantityUnitService.GetAllQuantityUnits().Select(x => x.ToModel()).ToList();

            var gridModel = new GridModel<QuantityUnitModel>
            {
                Data = quantityUnitModel,
                Total = quantityUnitModel.Count()
            };
            return View(gridModel);
        }

        //ajax
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

        #endregion

        #region Create / Edit / Delete / Save

        public ActionResult Create()
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var model = new QuantityUnitModel();
            //locales
            AddLocales(_languageService, model.Locales);
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(QuantityUnitModel model, bool continueEditing)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var quantityUnit = model.ToEntity();

                _quantityUnitService.InsertQuantityUnit(quantityUnit);
                //locales
                UpdateLocales(quantityUnit, model);

                NotifySuccess(T("Admin.Configuration.QuantityUnit.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = quantityUnit.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var quantityUnit = _quantityUnitService.GetQuantityUnitById(id);
            if (quantityUnit == null)
                return RedirectToAction("List");

            var model = quantityUnit.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = quantityUnit.GetLocalized(x => x.Name, languageId, false, false);
				locale.Description = quantityUnit.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(QuantityUnitModel model, bool continueEditing)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var quantityUnit = _quantityUnitService.GetQuantityUnitById(model.Id);
            if (quantityUnit == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                quantityUnit = model.ToEntity(quantityUnit);

                UpdateLocales(quantityUnit, model);

                _quantityUnitService.UpdateQuantityUnit(quantityUnit);

                NotifySuccess(T("Admin.Configuration.QuantityUnits.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = quantityUnit.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            var quantityUnit = _quantityUnitService.GetQuantityUnitById(id);
            if (quantityUnit == null)
                //No delivery time found with the specified id
                return RedirectToAction("List");

            try
            {

                _quantityUnitService.DeleteQuantityUnit(quantityUnit);

                NotifySuccess(T("Admin.Configuration.QuantityUnits.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { id = quantityUnit.Id });
            }
        }

        [HttpPost]
        public ActionResult Save(FormCollection formValues)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
                return AccessDeniedView();

            return RedirectToAction("List", "QuantityUnit");
        }

        #endregion
    }
}
