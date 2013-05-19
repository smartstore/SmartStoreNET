using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ContentSlider;
using SmartStore.Core;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Media;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ContentSliderController :  AdminControllerBase
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IPictureService _pictureService;
        private readonly ContentSliderSettings _contentSliderSettings;
        #endregion

        #region Constructors

        public ContentSliderController( ISettingService settingService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService, 
            ILanguageService languageService,
            IPictureService pictureService,
            ContentSliderSettings contentSliderSettings)
        {
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
            this._pictureService = pictureService;
            this._contentSliderSettings = contentSliderSettings;
        }
        
        #endregion

        #region Methods

        public ActionResult Index()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var model = _contentSliderSettings.ToModel();

            foreach (ContentSliderSlideModel slide in model.Slides)
            {
                slide.LanguageName = _languageService.GetLanguageByCulture(slide.LanguageCulture).Name;
            }

            return View(model);
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

            _settingService.SaveSetting(_contentSliderSettings);

            return View(_contentSliderSettings.ToModel());
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult CreateSlide()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new ContentSliderSlideModel();
            
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
                _settingService.SaveSetting(_contentSliderSettings);
                
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Added"));
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult EditSlide(int index)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var slide = _contentSliderSettings.Slides[index].ToModel();
            slide.Id = index;

            if (slide == null)
                return RedirectToAction("Index");

            return View(slide);
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

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Updated"));

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
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.ContentSlider.Slide.Deleted"));
                return RedirectToAction("Index");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { index = id });
            }
        }

        #endregion
    }
}
