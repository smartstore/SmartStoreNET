using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Forums;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ForumController : AdminControllerBase
    {
        private readonly IForumService _forumService;
		private readonly ICommonServices _commonServices;
        private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IStoreMappingService _storeMappingService;

        public ForumController(IForumService forumService,
			ICommonServices commonServices,
            IDateTimeHelper dateTimeHelper,
			IStoreMappingService storeMappingService)
        {
            _forumService = forumService;
			_commonServices = commonServices;
            _dateTimeHelper = dateTimeHelper;
			_storeMappingService = storeMappingService;
        }

		#region Utilities

		[NonAction]
		private void PrepareForumGroupModel(ForumGroupModel model, ForumGroup forumGroup, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var allStores = _commonServices.StoreService.GetAllStores();

			model.AvailableStores = allStores.Select(s => s.ToModel()).ToList();

			if (forumGroup != null)
			{
				model.CreatedOn = _dateTimeHelper.ConvertToUserTime(forumGroup.CreatedOnUtc, DateTimeKind.Utc);
			}

			if (!excludeProperties)
			{
				if (forumGroup != null)
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(forumGroup);
				else
					model.SelectedStoreIds = new int[0];
			}

			ViewBag.StoreCount = allStores.Count;
		}

		#endregion

        #region List

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroupsModel = _forumService.GetAllForumGroups(true)
                .Select(fg =>
                {
                    var forumGroupModel = fg.ToModel();

					PrepareForumGroupModel(forumGroupModel, fg, false);

                    foreach (var f in fg.Forums.OrderBy(x=>x.DisplayOrder))
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

        #endregion

        #region Create

        public ActionResult CreateForumGroup()
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

			var model = new ForumGroupModel { DisplayOrder = 1 };

			PrepareForumGroupModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult CreateForumGroup(ForumGroupModel model, bool continueEditing)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
				var utcNow = DateTime.UtcNow;
				var forumGroup = model.ToEntity();
                forumGroup.CreatedOnUtc = utcNow;
                forumGroup.UpdatedOnUtc = utcNow;

                _forumService.InsertForumGroup(forumGroup);

				_storeMappingService.SaveStoreMappings<ForumGroup>(forumGroup, model.SelectedStoreIds);

                NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.ForumGroup.Added"));

                return continueEditing ? RedirectToAction("EditForumGroup", new { forumGroup.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

			PrepareForumGroupModel(model, null, true);

            return View(model);
        }

        public ActionResult CreateForum()
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var model = new ForumModel();
            foreach (var forumGroup in _forumService.GetAllForumGroups(true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            model.DisplayOrder = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult CreateForum(ForumModel model, bool continueEditing)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var forum = model.ToEntity();
                forum.CreatedOnUtc = DateTime.UtcNow;
                forum.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.InsertForum(forum);

                NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Added"));
                return continueEditing ? RedirectToAction("EditForum", new { forum.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups(true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        #endregion

        #region Edit

        public ActionResult EditForumGroup(int id)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToAction("List");

            var model = forumGroup.ToModel();

			PrepareForumGroupModel(model, forumGroup, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult EditForumGroup(ForumGroupModel model, bool continueEditing)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(model.Id);
            if (forumGroup == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                forumGroup = model.ToEntity(forumGroup);
                forumGroup.UpdatedOnUtc = DateTime.UtcNow;

                _forumService.UpdateForumGroup(forumGroup);

				_storeMappingService.SaveStoreMappings<ForumGroup>(forumGroup, model.SelectedStoreIds);

                NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.ForumGroup.Updated"));

                return continueEditing ? RedirectToAction("EditForumGroup", forumGroup.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

			PrepareForumGroupModel(model, forumGroup, true);

            return View(model);
        }

        public ActionResult EditForum(int id)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(id);
            if (forum == null)
                return RedirectToAction("List");

            var model = forum.ToModel();
            foreach (var forumGroup in _forumService.GetAllForumGroups(true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult EditForum(ForumModel model, bool continueEditing)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(model.Id);
            if (forum == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                forum = model.ToEntity(forum);
                forum.UpdatedOnUtc = DateTime.UtcNow;
                _forumService.UpdateForum(forum);

                NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Updated"));
                return continueEditing ? RedirectToAction("EditForum", forum.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            foreach (var forumGroup in _forumService.GetAllForumGroups(true))
            {
                var forumGroupModel = forumGroup.ToModel();
                model.ForumGroups.Add(forumGroupModel);
            }
            return View(model);
        }

        #endregion

        #region Delete

        [HttpPost]
        public ActionResult DeleteForumGroup(int id)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToAction("List");

            _forumService.DeleteForumGroup(forumGroup);

            NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.ForumGroup.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult DeleteForum(int id)
        {
            if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManageForums))
                return AccessDeniedView();

            var forum = _forumService.GetForumById(id);
            if (forum == null)
                return RedirectToAction("List");

            _forumService.DeleteForum(forum);

            NotifySuccess(_commonServices.Localization.GetResource("Admin.ContentManagement.Forums.Forum.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}
