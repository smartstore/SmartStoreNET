using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
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
    public class DeliveryTimeController :  AdminControllerBase
    {
        #region Fields

        private readonly IDeliveryTimeService _deliveryTimeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;

        #endregion

        #region Constructors

        public DeliveryTimeController(
			IDeliveryTimeService deliveryTimeService,
            ILocalizedEntityService localizedEntityService, 
            ILanguageService languageService)
        {
            _deliveryTimeService = deliveryTimeService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        public void UpdateLocales(DeliveryTime deliveryTime, DeliveryTimeModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(deliveryTime, x => x.Name, localized.Name, localized.LanguageId);
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
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            return View();
        }

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			var gridModel = new GridModel<DeliveryTimeModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
			{
				var deliveryTimeModels = _deliveryTimeService.GetAllDeliveryTimes()
					.Select(x => x.ToModel())
					.ToList();

				gridModel.Data = deliveryTimeModels;
				gridModel.Total = deliveryTimeModels.Count();
			}
			else
			{
				gridModel.Data = Enumerable.Empty<DeliveryTimeModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = gridModel
			};
		}

		//ajax
		public ActionResult AllDeliveryTimes(string label, int selectedId)
        {
            var deliveryTimes = _deliveryTimeService.GetAllDeliveryTimes();
            if (label.HasValue())
            {
                deliveryTimes.Insert(0, new DeliveryTime { Name = label, Id = 0 });
            }

            var list = from m in deliveryTimes
                       select new
                       {
                           id = m.Id.ToString(),
                           text = m.Name,
                           selected = m.Id == selectedId
                       };

            return new JsonResult
			{
				Data = list.ToList(),
				JsonRequestBehavior = JsonRequestBehavior.AllowGet
			};
        }

        #endregion

        #region Create / Edit / Delete / Save

        public ActionResult Create()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            var model = new DeliveryTimeModel();

            AddLocales(_languageService, model.Locales);
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(DeliveryTimeModel model, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var deliveryTime = model.ToEntity();
                
                _deliveryTimeService.InsertDeliveryTime(deliveryTime);

                UpdateLocales(deliveryTime, model);

                NotifySuccess(T("Admin.Configuration.DeliveryTime.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = deliveryTime.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            var deliveryTime = _deliveryTimeService.GetDeliveryTimeById(id);
            if (deliveryTime == null)
                return RedirectToAction("List");

            var model = deliveryTime.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = deliveryTime.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(DeliveryTimeModel model, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            var deliveryTime = _deliveryTimeService.GetDeliveryTimeById(model.Id);
            if (deliveryTime == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                deliveryTime = model.ToEntity(deliveryTime);

                // if this is the default delivery time set all other delivery times to non default
                if (model.IsDefault)
                {
                    _deliveryTimeService.SetToDefault(deliveryTime);
                }

                UpdateLocales(deliveryTime, model);
				_deliveryTimeService.UpdateDeliveryTime(deliveryTime);
                
                NotifySuccess(T("Admin.Configuration.DeliveryTimes.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = deliveryTime.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            var deliveryTime = _deliveryTimeService.GetDeliveryTimeById(id);
            if (deliveryTime == null)
                return RedirectToAction("List");

            try
            {
                _deliveryTimeService.DeleteDeliveryTime(deliveryTime);

                NotifySuccess(T("Admin.Configuration.DeliveryTimes.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { id = deliveryTime.Id });
            }
        }

        [HttpPost]
        public ActionResult Save(FormCollection formValues)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageDeliveryTimes))
                return AccessDeniedView();

            return RedirectToAction("List", "DeliveryTime");
        }

        #endregion
    }
}
