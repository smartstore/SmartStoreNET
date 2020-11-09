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
    public class DeliveryTimeController : AdminControllerBase
    {
        private readonly IDeliveryTimeService _deliveryTimeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;

        public DeliveryTimeController(
            IDeliveryTimeService deliveryTimeService,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService)
        {
            _deliveryTimeService = deliveryTimeService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
        }

        private void UpdateLocales(DeliveryTime deliveryTime, DeliveryTimeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(deliveryTime, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        #region Delivery time

        // AJAX.
        public ActionResult AllDeliveryTimes(string selectedIds)
        {
            var deliveryTimes = _deliveryTimeService.GetAllDeliveryTimes();
            var selectedArr = selectedIds.ToIntArray();

            var data = deliveryTimes
                .Select(x => new ChoiceListItem
                {
                    Id = x.Id.ToString(),
                    Text = x.GetLocalized(y => y.Name).Value,
                    Description = _deliveryTimeService.GetFormattedDeliveryDate(x),
                    Selected = selectedArr.Contains(x.Id)
                })
                .ToList();

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.DeliveryTime.Read)]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.DeliveryTime.Read)]
        public ActionResult List(GridCommand _)
        {
            var gridModel = new GridModel<DeliveryTimeModel>();

            var deliveryTimeModels = _deliveryTimeService.GetAllDeliveryTimes()
                .Select(x =>
                {
                    var model = x.ToModel();
                    model.DeliveryInfo = _deliveryTimeService.GetFormattedDeliveryDate(x);

                    return model;
                })
                .ToList();

            gridModel.Data = deliveryTimeModels;
            gridModel.Total = deliveryTimeModels.Count();

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Configuration.DeliveryTime.Create)]
        public ActionResult CreateDeliveryTimePopup()
        {
            var model = new DeliveryTimeModel();
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.DeliveryTime.Create)]
        public ActionResult CreateDeliveryTimePopup(string btnId, DeliveryTimeModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var entity = model.ToEntity();

                    _deliveryTimeService.InsertDeliveryTime(entity);

                    UpdateLocales(entity, model);

                    NotifySuccess(T("Admin.Configuration.DeliveryTime.Added"));
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

        [Permission(Permissions.Configuration.DeliveryTime.Read)]
        public ActionResult EditDeliveryTimePopup(int id)
        {
            var entity = _deliveryTimeService.GetDeliveryTimeById(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = entity.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.DeliveryTime.Update)]
        public ActionResult EditDeliveryTimePopup(string btnId, DeliveryTimeModel model)
        {
            var entity = _deliveryTimeService.GetDeliveryTimeById(model.Id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    entity = model.ToEntity(entity);

                    // If this is the default delivery time set all other delivery times to non default.
                    if (model.IsDefault)
                    {
                        _deliveryTimeService.SetToDefault(entity);
                    }

                    _deliveryTimeService.UpdateDeliveryTime(entity);
                    UpdateLocales(entity, model);

                    NotifySuccess(T("Admin.Configuration.DeliveryTimes.Updated"));
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
        [Permission(Permissions.Configuration.DeliveryTime.Delete)]
        public ActionResult DeleteDeliveryTime(int id, GridCommand command)
        {
            var entity = _deliveryTimeService.GetDeliveryTimeById(id);

            try
            {
                _deliveryTimeService.DeleteDeliveryTime(entity);

                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return List(command);
        }

        #endregion
    }
}
