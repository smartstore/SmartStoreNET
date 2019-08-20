using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MeasureController : AdminControllerBase
	{
        private readonly IMeasureService _measureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly MeasureSettings _measureSettings;

        public MeasureController(
            IMeasureService measureService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            MeasureSettings measureSettings)
		{
            _measureService = measureService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _measureSettings = measureSettings;
        }
       
        #region Weights

        public ActionResult Weights()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedView();
            }

            return View();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Weights(GridCommand command)
        {
			var model = new GridModel<MeasureWeightModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				var weightsModel = _measureService.GetAllMeasureWeights()
					.Select(x => x.ToModel())
					.ForCommand(command)
					.ToList();

				foreach (var wm in weightsModel)
				{
					wm.IsPrimaryWeight = wm.Id == _measureSettings.BaseWeightId;
				}

				model.Data = weightsModel;
				model.Total = weightsModel.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<MeasureWeightModel>();

				NotifyAccessDenied();
			}

		    return new JsonResult
			{
				Data = model
			};
		}

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DeleteWeight(int id, GridCommand command)
        {
			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				var entity = _measureService.GetMeasureWeightById(id);

                if (entity.Id == _measureSettings.BaseWeightId)
                {
                    NotifyError(T("Admin.Configuration.Measures.Weights.CantDeletePrimary"));
                }
                else
                {
                    _measureService.DeleteMeasureWeight(entity);
                }
			}

            return Weights(command);
        }

        public ActionResult CreateWeightPopup()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var model = new MeasureWeightModel();

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateWeightPopup(string btnId, MeasureWeightModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var entity = model.ToEntity();

                    _measureService.InsertMeasureWeight(entity);

                    if (model.IsPrimaryWeight)
                    {
                        _measureSettings.BaseWeightId = entity.Id;
                        Services.Settings.SaveSetting(_measureSettings);
                    }

                    foreach (var localized in model.Locales)
                    {
                        _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
                    }
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

        public ActionResult EditWeightPopup(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _measureService.GetMeasureWeightById(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = entity.ToModel();
            model.IsPrimaryWeight = entity.Id == _measureSettings.BaseWeightId;

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditWeightPopup(string btnId, MeasureWeightModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _measureService.GetMeasureWeightById(model.Id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    entity = model.ToEntity(entity);

                    _measureService.UpdateMeasureWeight(entity);

                    if (model.IsPrimaryWeight && _measureSettings.BaseWeightId != entity.Id)
                    {
                        _measureSettings.BaseWeightId = entity.Id;
                        Services.Settings.SaveSetting(_measureSettings);
                    }

                    foreach (var localized in model.Locales)
                    {
                        _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
                    }
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

        #endregion

        #region Dimensions

        public ActionResult Dimensions(string id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedView();
            }

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Dimensions(GridCommand command)
        {
			var model = new GridModel<MeasureDimensionModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				var dimensionsModel = _measureService.GetAllMeasureDimensions()
					.Select(x => x.ToModel())
					.ForCommand(command)
					.ToList();

				foreach (var wm in dimensionsModel)
				{
					wm.IsPrimaryDimension = wm.Id == _measureSettings.BaseDimensionId;
				}

				model.Data = dimensionsModel;
				model.Total = dimensionsModel.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<MeasureDimensionModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DeleteDimension(int id, GridCommand command)
        {
			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				var entity = _measureService.GetMeasureDimensionById(id);

                if (entity.Id == _measureSettings.BaseDimensionId)
                {
                    NotifyError(T("Admin.Configuration.Measures.Dimensions.CantDeletePrimary"));
                }
                else
                {
                    _measureService.DeleteMeasureDimension(entity);
                }
			}

            return Dimensions(command);
        }

        public ActionResult CreateDimensionPopup()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var model = new MeasureDimensionModel();

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateDimensionPopup(string btnId, MeasureDimensionModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var entity = model.ToEntity();

                    _measureService.InsertMeasureDimension(entity);

                    if (model.IsPrimaryDimension)
                    {
                        _measureSettings.BaseDimensionId = entity.Id;
                        Services.Settings.SaveSetting(_measureSettings);
                    }

                    foreach (var localized in model.Locales)
                    {
                        _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
                    }
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

        public ActionResult EditDimensionPopup(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _measureService.GetMeasureDimensionById(id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            var model = entity.ToModel();
            model.IsPrimaryDimension = entity.Id == _measureSettings.BaseDimensionId;

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult EditDimensionPopup(string btnId, MeasureDimensionModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            var entity = _measureService.GetMeasureDimensionById(model.Id);
            if (entity == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    entity = model.ToEntity(entity);

                    _measureService.UpdateMeasureDimension(entity);

                    if (model.IsPrimaryDimension && _measureSettings.BaseDimensionId != entity.Id)
                    {
                        _measureSettings.BaseDimensionId = entity.Id;
                        Services.Settings.SaveSetting(_measureSettings);
                    }

                    foreach (var localized in model.Locales)
                    {
                        _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
                    }
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

        #endregion
    }
}
