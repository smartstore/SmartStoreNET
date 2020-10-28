using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Forums;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ForumController : AdminControllerBase
    {
        #region Fields

        private readonly IForumService _forumService;
        private readonly ICommonServices _services;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructor

        public ForumController(
            IForumService forumService,
            ICommonServices services,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            ICustomerService customerService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            IDateTimeHelper dateTimeHelper)
        {
            _forumService = forumService;
            _services = services;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _customerService = customerService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareForumGroupModel(ForumGroupModel model, ForumGroup forumGroup, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            var allStores = _services.StoreService.GetAllStores();

            if (forumGroup != null)
            {
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(forumGroup.CreatedOnUtc, DateTimeKind.Utc);
            }

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(forumGroup);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(forumGroup);
            }

            ViewBag.StoreCount = allStores.Count;
        }

        [NonAction]
        private void UpdateLocales(ForumGroupModel model, ForumGroup forumGroup)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(forumGroup, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(forumGroup, x => x.Description, localized.Description, localized.LanguageId);

                var seName = forumGroup.ValidateSeName(localized.SeName, localized.Name, false, localized.LanguageId);
                _urlRecordService.SaveSlug(forumGroup, seName, localized.LanguageId);
            }
        }

        [NonAction]
        private void UpdateLocales(ForumModel model, Forum forum)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(forum, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(forum, x => x.Description, localized.Description, localized.LanguageId);

                var seName = forum.ValidateSeName(localized.SeName, localized.Name, false, localized.LanguageId);
                _urlRecordService.SaveSlug(forum, seName, localized.LanguageId);
            }
        }

        #endregion

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Forum.Read)]
        public ActionResult List()
        {
            var forumGroupsModel = _forumService.GetAllForumGroups(0, true)
                .Select(fg =>
                {
                    var forumGroupModel = fg.ToModel();

                    PrepareForumGroupModel(forumGroupModel, fg, false);

                    foreach (var f in fg.Forums.OrderBy(x => x.DisplayOrder))
                    {
                        var forumModel = f.ToModel();
                        forumModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(f.CreatedOnUtc, DateTimeKind.Utc);

                        forumGroupModel.ForumModels.Add(forumModel);
                    }
                    return forumGroupModel;
                })
                .ToList();

            return View(forumGroupsModel);
        }

        #region Forum

        [Permission(Permissions.Cms.Forum.Create)]
        public ActionResult CreateForum()
        {
            var model = new ForumModel { DisplayOrder = 1 };

            AddLocales(_languageService, model.Locales);

            foreach (var forumGroup in _forumService.GetAllForumGroups(0, true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Create)]
        public ActionResult CreateForum(ForumModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var forum = model.ToEntity();

                _forumService.InsertForum(forum);

                model.SeName = forum.ValidateSeName(model.SeName, forum.Name, true);
                _urlRecordService.SaveSlug(forum, model.SeName, 0);

                NotifySuccess(_services.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Added"));

                return continueEditing ? RedirectToAction("EditForum", new { forum.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups(0, true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        [Permission(Permissions.Cms.Forum.Read)]
        public ActionResult EditForum(int id)
        {
            var forum = _forumService.GetForumById(id);
            if (forum == null)
                return RedirectToAction("List");

            var model = forum.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = forum.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = forum.GetLocalized(x => x.Description, languageId, false, false);
                locale.SeName = forum.GetSeName(languageId, false, false);
            });

            foreach (var forumGroup in _forumService.GetAllForumGroups(0, true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Update)]
        public ActionResult EditForum(ForumModel model, bool continueEditing)
        {
            var forum = _forumService.GetForumById(model.Id);
            if (forum == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                forum = model.ToEntity(forum);

                _forumService.UpdateForum(forum);

                model.SeName = forum.ValidateSeName(model.SeName, forum.Name, true);
                _urlRecordService.SaveSlug(forum, model.SeName, 0);

                UpdateLocales(model, forum);

                NotifySuccess(_services.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Updated"));

                return continueEditing ? RedirectToAction("EditForum", forum.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups(0, true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Delete)]
        public ActionResult DeleteForum(int id)
        {
            var forum = _forumService.GetForumById(id);
            if (forum == null)
                return RedirectToAction("List");

            _forumService.DeleteForum(forum);

            NotifySuccess(_services.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Forum group

        [Permission(Permissions.Cms.Forum.Create)]
        public ActionResult CreateForumGroup()
        {
            var model = new ForumGroupModel { DisplayOrder = 1 };

            AddLocales(_languageService, model.Locales);

            PrepareForumGroupModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Create)]
        public ActionResult CreateForumGroup(ForumGroupModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var forumGroup = model.ToEntity();

                _forumService.InsertForumGroup(forumGroup);

                model.SeName = forumGroup.ValidateSeName(model.SeName, forumGroup.Name, true);
                _urlRecordService.SaveSlug(forumGroup, model.SeName, 0);

                UpdateLocales(model, forumGroup);

                SaveStoreMappings(forumGroup, model.SelectedStoreIds);
                SaveAclMappings(forumGroup, model.SelectedCustomerRoleIds);

                NotifySuccess(T("Admin.ContentManagement.Forums.ForumGroup.Added"));

                return continueEditing ? RedirectToAction("EditForumGroup", new { forumGroup.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareForumGroupModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Cms.Forum.Read)]
        public ActionResult EditForumGroup(int id)
        {
            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToAction("List");

            var model = forumGroup.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = forumGroup.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = forumGroup.GetLocalized(x => x.Description, languageId, false, false);
                locale.SeName = forumGroup.GetSeName(languageId, false, false);
            });

            PrepareForumGroupModel(model, forumGroup, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Update)]
        public ActionResult EditForumGroup(ForumGroupModel model, bool continueEditing)
        {
            var forumGroup = _forumService.GetForumGroupById(model.Id);
            if (forumGroup == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                forumGroup = model.ToEntity(forumGroup);

                _forumService.UpdateForumGroup(forumGroup);

                model.SeName = forumGroup.ValidateSeName(model.SeName, forumGroup.Name, true);
                _urlRecordService.SaveSlug(forumGroup, model.SeName, 0);

                UpdateLocales(model, forumGroup);

                SaveStoreMappings(forumGroup, model.SelectedStoreIds);
                SaveAclMappings(forumGroup, model.SelectedCustomerRoleIds);

                NotifySuccess(T("Admin.ContentManagement.Forums.ForumGroup.Updated"));

                return continueEditing ? RedirectToAction("EditForumGroup", forumGroup.Id) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareForumGroupModel(model, forumGroup, true);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Forum.Delete)]
        public ActionResult DeleteForumGroup(int id)
        {
            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToAction("List");

            _forumService.DeleteForumGroup(forumGroup);

            NotifySuccess(_services.Localization.GetResource("Admin.ContentManagement.Forums.ForumGroup.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
