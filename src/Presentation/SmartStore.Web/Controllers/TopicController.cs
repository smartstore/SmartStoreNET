using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Services.Localization;
using SmartStore.Services.Topics;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Topics;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
    public partial class TopicController : PublicControllerBase
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Constructors

        public TopicController(ITopicService topicService,
            ILocalizationService localizationService,
            IWorkContext workContext,
			IStoreContext storeContext,
			ICacheManager cacheManager)
        {
            this._topicService = topicService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected TopicModel PrepareTopicModel(string systemName)
        {
			//load by store
			var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
            if (topic == null)
                return null;

            var titleTag = "h3";
            if(topic.TitleTag != null)
                titleTag = topic.TitleTag;
            else if (!topic.RenderAsWidget) 
                 titleTag = "h1";

            var model = new TopicModel()
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Title),
                Body = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Body),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle),
                TitleTag = titleTag,
				RenderAsWidget = topic.RenderAsWidget
			};

			if (!topic.RenderAsWidget)
			{
				Services.DisplayControl.Announce(topic);
			}

            return model;
        }

        #endregion

        #region Methods

        public ActionResult TopicDetails(string systemName)
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_KEY, systemName, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () => PrepareTopicModel(systemName));

			if (cacheModel == null || (cacheModel.RenderAsWidget))
				return HttpNotFound();

            return View("TopicDetails", cacheModel);
        }

        public ActionResult TopicDetailsPopup(string systemName)
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_KEY, systemName, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () => PrepareTopicModel(systemName));

            if (cacheModel == null)
				return HttpNotFound();

            ViewBag.IsPopup = true;
            return View("TopicDetails", cacheModel);
        }

        [ChildActionOnly]
        public ActionResult TopicBlock(string systemName, bool bodyOnly = false, bool isLead = false)
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_KEY, systemName, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () => PrepareTopicModel(systemName));

            if (cacheModel == null)
                return Content("");

            ViewBag.BodyOnly = bodyOnly;
			ViewBag.IsLead = isLead;

			return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult TopicWidget(TopicWidgetModel model)
        {
            return PartialView(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Authenticate(int id, string password)
        {
            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = _topicService.GetTopicById(id);

            if (topic != null)
            {
                if (topic.Password != null && topic.Password.Equals(password))
                {
                    authResult = true;
                    title = topic.GetLocalized(x => x.Title);
                    body = topic.GetLocalized(x => x.Body);
                }
                else
                {
                    error = _localizationService.GetResource("Topic.WrongPassword");
                }
            }
            return Json(new { Authenticated = authResult, Title = title, Body = body, Error = error });
        }

        #endregion
    }
}
