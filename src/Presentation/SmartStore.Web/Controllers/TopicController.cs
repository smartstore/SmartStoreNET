using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Services.Localization;
using SmartStore.Services.Topics;
using SmartStore.Services.Seo;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core.Domain.Seo;
using System.Web.Routing;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Topics;

namespace SmartStore.Web.Controllers
{
    public partial class TopicController : PublicControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;
		private readonly SeoSettings _seoSettings;

		public TopicController(
			ITopicService topicService,
            ILocalizationService localizationService,
            IWorkContext workContext,
			IStoreContext storeContext,
			IStoreMappingService storeMappingService,
			ICacheManager cacheManager,
			SeoSettings seoSettings)
        {
            _topicService = topicService;
            _workContext = workContext;
			_storeContext = storeContext;
			_storeMappingService = storeMappingService;
			_localizationService = localizationService;
            _cacheManager = cacheManager;
			_seoSettings = seoSettings;
        }

        [NonAction]
        protected TopicModel PrepareTopicModel(Topic topic)
        {
			Guard.NotNull(topic, nameof(topic));

            var titleTag = "h3";
            if (topic.TitleTag != null)
			{
				titleTag = topic.TitleTag;
			}   
            else if (!topic.RenderAsWidget)
			{
				titleTag = "h1";
			}          

            var model = new TopicModel
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.Title),
                Body = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.Body, detectEmptyHtml: true),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle),
				SeName = topic.GetSeName(),
                TitleTag = titleTag,
				RenderAsWidget = topic.RenderAsWidget
			};

			if (!topic.RenderAsWidget)
			{
				Services.DisplayControl.Announce(topic);
			}

            return model;
        }

		/// <summary>
		/// Redirects old (prior V3.1.5) topic URL pattern "t/[SystemName]" to "[SeName]"
		/// </summary>
		public ActionResult TopicDetailsLegacy(string systemName, bool popup = false)
		{
			if (!_seoSettings.RedirectLegacyTopicUrls)
				return HttpNotFound();

			var topic = _topicService.GetTopicBySystemName(systemName);
			if (topic == null)
				return HttpNotFound();

			var routeValues = new RouteValueDictionary { ["SeName"] = topic.GetSeName() };
			if (popup)
				routeValues["popup"] = true;

			return RedirectToRoutePermanent("Topic", routeValues);
		}

		public ActionResult TopicDetails(int topicId, bool popup = false)
		{
			var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_BY_ID_KEY, topicId, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
			var cacheModel = _cacheManager.Get(cacheKey, () => 
			{
				var topic = _topicService.GetTopicById(topicId);
				if (topic == null)
					return null;

				if (!_storeMappingService.Authorize(topic))
					return null;

				return PrepareTopicModel(topic);
			});

			if (cacheModel == null || (!popup && cacheModel.RenderAsWidget))
				return HttpNotFound();

			ViewBag.IsPopup = popup;

			return View("TopicDetails", cacheModel);
		}

        [ChildActionOnly]
        public ActionResult TopicBlock(string systemName, bool bodyOnly = false, bool isLead = false)
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_BY_SYSTEMNAME_KEY, systemName.ToLower(), _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () => 
			{
				var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
				if (topic == null)
					return null;

				if (!_storeMappingService.Authorize(topic))
					return null;

				return PrepareTopicModel(topic);
			});

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
                    body = topic.GetLocalized(x => x.Body, detectEmptyHtml: true);
                }
                else
                {
                    error = _localizationService.GetResource("Topic.WrongPassword");
                }
            }
            return Json(new { Authenticated = authResult, Title = title, Body = body, Error = error });
        }
    }
}
