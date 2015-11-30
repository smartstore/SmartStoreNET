using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ContentSlider;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Media;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Stores;
using SmartStore.Core;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ContentSliderController :  AdminControllerBase
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ContentSliderSettings _contentSliderSettings;
		private readonly IStoreService _storeService;

        #endregion

        #region Constructors

        public ContentSliderController( ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService, 
            ILanguageService languageService,
            IPictureService pictureService,
            ContentSliderSettings contentSliderSettings,
			IStoreService storeService,
            IWorkContext workContext)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
            this._pictureService = pictureService;
            this._contentSliderSettings = contentSliderSettings;
			this._storeService = storeService;
            this._workContext = workContext;
        }
        
        #endregion

        #region Methods

		private ContentSliderSettingsModel PrepareContentSliderSettingsModel(ContentSliderSettingsModel modelIn = null)
		{
			int rowIndex = 0;
			var allStores = _storeService.GetAllStores();
			var model = _contentSliderSettings.ToModel();

			model.StoreCount = allStores.Count;

			model.AvailableStores.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
			foreach (var s in allStores)
			{
				model.AvailableStores.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString() });
			}

			foreach (var slide in model.Slides)
			{
				slide.SlideIndex = rowIndex++;

				var language = _languageService.GetLanguageByCulture(slide.LanguageCulture);
				if (language != null)
				{
					slide.LanguageName = language.Name;
				}
				else
				{
					var seoCode = _languageService.GetDefaultLanguageSeoCode();
					slide.LanguageName = _languageService.GetLanguageBySeoCode(seoCode).Name;
				}
			}

			// note order: first SlideIndex then search filter.
			if (modelIn != null && modelIn.SearchStoreId > 0)
			{
				model.Slides = model.Slides
					.Where(x => x.LimitedToStores && x.SelectedStoreIds != null && x.SelectedStoreIds.Contains(modelIn.SearchStoreId))
					.ToList();
			}

			return model;
		}

        public ActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

			var model = PrepareContentSliderSettingsModel();

            return View(model);
        }

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult SlideList(GridCommand command, ContentSliderSettingsModel model)
		{
			var gridModel = new GridModel();
			var viewModel = PrepareContentSliderSettingsModel(model);

			gridModel.Data = viewModel.Slides.OrderBy(s => s.LanguageName).ThenBy(s => s.DisplayOrder);

            return new JsonResult { Data = gridModel };
		}
        
        [HttpPost]
        public ActionResult Index(ContentSliderSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            _contentSliderSettings.IsActive = model.IsActive;
            _contentSliderSettings.ContentSliderHeight = model.ContentSliderHeight;
            _contentSliderSettings.BackgroundPictureId = model.BackgroundPictureId;
            _contentSliderSettings.BackgroundPictureUrl = _pictureService.GetPictureUrl(model.BackgroundPictureId);
            _contentSliderSettings.AutoPlay = model.AutoPlay;
            _contentSliderSettings.AutoPlayDelay = model.AutoPlayDelay;

			MediaHelper.UpdatePictureTransientState(0, model.BackgroundPictureId, true);

            _settingService.SaveSetting(_contentSliderSettings);

			var viewModel = PrepareContentSliderSettingsModel(model);

            return View(viewModel);
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult CreateSlide()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new ContentSliderSlideModel();
			model.AvailableStores = _storeService.GetAllStores().Select(s => s.ToModel()).ToList();

			var lastSlide = _contentSliderSettings.Slides.OrderByDescending(x => x.DisplayOrder).FirstOrDefault();
			if (lastSlide != null)
				model.DisplayOrder = lastSlide.DisplayOrder + 1;
            
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateSlide(ContentSliderSlideModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            if (ModelState.IsValid)
            {
                model.PictureUrl = _pictureService.GetPictureUrl(model.PictureId);
                model.LanguageName = _languageService.GetLanguageByCulture(model.LanguageCulture).Name;
                _contentSliderSettings.Slides.Add(model.ToEntity());
				MediaHelper.UpdatePictureTransientState(0, model.PictureId, true);
                _settingService.SaveSetting(_contentSliderSettings);
                
                NotifySuccess(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Added"));
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult EditSlide(int index)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = _contentSliderSettings.Slides[index].ToModel();

			if (model == null)
				return RedirectToAction("Index");

            model.Id = index;
			model.AvailableStores = _storeService.GetAllStores().Select(s => s.ToModel()).ToList();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult EditSlide(ContentSliderSlideModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            if (model == null)
                //No slide found
                return RedirectToAction("Index");

            var index = Request.QueryString["index"].ToInt();

            if (ModelState.IsValid)
            {
                Language lang = null;
                int langId = model.LanguageCulture.ToInt();

                if (langId > 0)
                {
                    lang = _languageService.GetLanguageById(langId);
                }
                else
                {
                    lang = _languageService.GetLanguageByCulture(model.LanguageCulture);
                }

                if (lang != null)
                {
                    model.LanguageName = lang.Name;
                }

                model.PictureUrl = _pictureService.GetPictureUrl(model.PictureId);            

                //delete an old picture (if deleted or updated)
                int prevPictureId = _contentSliderSettings.Slides[index].PictureId;
                if (prevPictureId > 0 && prevPictureId != model.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }

                _contentSliderSettings.Slides[index] = model.ToEntity();
                _settingService.SaveSetting(_contentSliderSettings);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Updated"));

                return continueEditing ? RedirectToAction("EditSlide", new { index = index }) : RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost, ActionName("DeleteSlide")]
        public ActionResult DeleteSlideConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            try
            {
                var slidePicture = _pictureService.GetPictureById(_contentSliderSettings.Slides[id].PictureId);
                if (slidePicture != null)
                    _pictureService.DeletePicture(slidePicture);

                _contentSliderSettings.Slides.RemoveAt(id);
                _settingService.SaveSetting(_contentSliderSettings);
                NotifySuccess(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Deleted"));
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { index = id });
            }
        }

        #endregion
    }
}
