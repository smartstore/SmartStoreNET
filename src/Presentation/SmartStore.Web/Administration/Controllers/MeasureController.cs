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
        public ActionResult WeightDelete(int id, GridCommand command)
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

        public ActionResult WeightCreatePopup()
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
        public ActionResult WeightCreatePopup(string btnId, MeasureWeightModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
            {
                return AccessDeniedPartialView();
            }

            if (ModelState.IsValid)
            {
                var entity = new MeasureWeight();

                try
                {
                    entity = model.ToEntity(entity);

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

        public ActionResult WeightEditPopup(int id)
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
        public ActionResult WeightEditPopup(string btnId, MeasureWeightModel model)
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
                return AccessDeniedView();

            // Mark as primary dimension (if selected).
            if (id.HasValue())
            {
                int primaryDimensionId = Convert.ToInt32(id);
                var primaryDimension = _measureService.GetMeasureDimensionById(primaryDimensionId);
                if (primaryDimension != null)
                {
                    _measureSettings.BaseDimensionId = primaryDimensionId;
                    Services.Settings.SaveSetting(_measureSettings);
                }
            }

            var dimensionsModel = _measureService.GetAllMeasureDimensions()
                .Select(x => x.ToModel())
                .ToList();

            foreach (var wm in dimensionsModel)
            {
                wm.IsPrimaryDimension = wm.Id == _measureSettings.BaseDimensionId;
            }

            var model = new GridModel<MeasureDimensionModel>
            {
                Data = dimensionsModel,
                Total = dimensionsModel.Count
            };

            return View(model);
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
        public ActionResult DimensionUpdate(MeasureDimensionModel model, GridCommand command)
        {
			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var dimension = _measureService.GetMeasureDimensionById(model.Id);
				dimension = model.ToEntity(dimension);

				_measureService.UpdateMeasureDimension(dimension);
			}
            
            return Dimensions(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DimensionAdd([Bind(Exclude="Id")] MeasureDimensionModel model, GridCommand command)
        {
			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var dimension = new MeasureDimension();
				dimension = model.ToEntity(dimension);

				_measureService.InsertMeasureDimension(dimension);
			}

            return Dimensions(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DimensionDelete(int id, GridCommand command)
        {
			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMeasures))
			{
				var dimension = _measureService.GetMeasureDimensionById(id);

				if (dimension.Id == _measureSettings.BaseDimensionId)
				{
					return Content(T("Admin.Configuration.Measures.Dimensions.CantDeletePrimary"));
				}

				_measureService.DeleteMeasureDimension(dimension);
			}

            return Dimensions(command);
        }

        #endregion
    }
}
