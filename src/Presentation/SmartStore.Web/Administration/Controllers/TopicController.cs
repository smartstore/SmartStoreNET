using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Topics;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class TopicController : AdminControllerBase
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
        private readonly IEventPublisher _eventPublisher;

        #endregion Fields

        #region Constructors

        public TopicController(ITopicService topicService, ILanguageService languageService,
            ILocalizedEntityService localizedEntityService, ILocalizationService localizationService,
			IPermissionService permissionService, IStoreService storeService,
            IStoreMappingService storeMappingService, IEventPublisher eventPublisher)
        {
            this._topicService = topicService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
            this._eventPublisher = eventPublisher;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateLocales(Topic topic, TopicModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(topic,
                                                               x => x.Title,
                                                               localized.Title,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.Body,
                                                           localized.Body,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(topic,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);
            }
        }

		[NonAction]
		private void PrepareStoresMappingModel(TopicModel model, Topic topic, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();
			if (!excludeProperties)
			{
				if (topic != null)
				{
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(topic);
				}
				else
				{
					model.SelectedStoreIds = new int[0];
				}
			}
		}

        #endregion
        
        #region List

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

			var model = new TopicListModel();

			foreach (var s in _storeService.GetAllStores())
			{
				model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command, TopicListModel model)
        {
			var gridModel = new GridModel<TopicModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
			{
				var topics = _topicService.GetAllTopics(model.SearchStoreId);

				gridModel.Data = topics.Select(x =>
				{
					var item = x.ToModel();
					// otherwise maxJsonLength could be exceeded
					item.Body = "";
					return item;
				});

				gridModel.Total = topics.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<TopicModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
			{
				MaxJsonLength = int.MaxValue,
				Data = gridModel
			};
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var model = new TopicModel();
			//Stores
			PrepareStoresMappingModel(model, null, false);
            //locales
            AddLocales(_languageService, model.Locales);

            model.TitleTag = "h1";

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(TopicModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (!model.IsPasswordProtected)
                {
                    model.Password = null;
                }

                var topic = model.ToEntity();
                _topicService.InsertTopic(topic);
                //locales
                UpdateLocales(topic, model);

                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.Topics.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = topic.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form
			PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                return RedirectToAction("List");

            var model = topic.ToModel();
            model.Url = Url.RouteUrl("Topic", new { SystemName = topic.SystemName }, "http");
			
			//Store
			PrepareStoresMappingModel(model, topic, false);
            
			//locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = topic.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = topic.GetLocalized(x => x.Body, languageId, false, false);
                locale.MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = topic.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = topic.GetLocalized(x => x.MetaTitle, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        public ActionResult Edit(TopicModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(model.Id);
            if (topic == null)
                return RedirectToAction("List");

            model.Url = Url.RouteUrl("Topic", new { SystemName = topic.SystemName }, "http");

            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            if (ModelState.IsValid)
            {
                topic = model.ToEntity(topic);
                _topicService.UpdateTopic(topic);
				
				//Stores
				_storeMappingService.SaveStoreMappings<Topic>(topic, model.SelectedStoreIds);
                
				//locales
                UpdateLocales(topic, model);

                _eventPublisher.Publish(new ModelBoundEvent(model, topic, form));

                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.Topics.Updated"));
                return continueEditing ? RedirectToAction("Edit", topic.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			//Store
			PrepareStoresMappingModel(model, topic, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                return RedirectToAction("List");

            if (topic.IsSystemTopic)
            {
                NotifyError(_localizationService.GetResource("Admin.ContentManagement.Topics.CannotBeDeleted"));
                return RedirectToAction("List");
            }
            
            _topicService.DeleteTopic(topic);

            NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.Topics.Deleted"));
            return RedirectToAction("List");
        }
        
        #endregion
    }
}
