using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Topics;

namespace SmartStore.Web.Controllers
{
    public partial class TopicController : PublicControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;
        private readonly SeoSettings _seoSettings;
        private readonly IBreadcrumb _breadcrumb;
        private readonly CatalogHelper _helper;
        private readonly ICookieManager _cookieManager;

        public TopicController(
            ITopicService topicService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            ICacheManager cacheManager,
            SeoSettings seoSettings,
            IBreadcrumb breadcrumb,
            CatalogHelper helper,
            ICookieManager cookieManager)
        {
            _topicService = topicService;
            _workContext = workContext;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _localizationService = localizationService;
            _cacheManager = cacheManager;
            _seoSettings = seoSettings;
            _breadcrumb = breadcrumb;
            _helper = helper;
            _cookieManager = cookieManager;
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
                HtmlId = topic.HtmlId,
                BodyCssClass = topic.BodyCssClass,
                IsPasswordProtected = topic.IsPasswordProtected,
                ShortTitle = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.ShortTitle),
                Title = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.Title),
                Intro = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.Intro),
                Body = topic.IsPasswordProtected ? null : topic.GetLocalized(x => x.Body, detectEmptyHtml: true),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle),
                SeName = topic.GetSeName(),
                TitleTag = titleTag,
                RenderAsWidget = topic.RenderAsWidget
            };

            return model;
        }

        /// <summary>
        /// Redirects old (prior V3.1.5) topic URL pattern "t/[SystemName]" to "[SeName]"
        /// </summary>
        public ActionResult TopicDetailsLegacy(string systemName, bool popup = false)
        {
            if (!_seoSettings.RedirectLegacyTopicUrls)
                return HttpNotFound();

            var topic = _topicService.GetTopicBySystemName(systemName, 0, false);
            if (topic == null || !topic.IsPublished)
                return HttpNotFound();

            var routeValues = new RouteValueDictionary { ["SeName"] = topic.GetSeName() };
            if (popup)
                routeValues["popup"] = true;

            return RedirectToRoutePermanent("Topic", routeValues);
        }

        public ActionResult TopicDetails(int topicId, bool popup = false)
        {
            _helper.GetBreadcrumb(_breadcrumb, ControllerContext);

            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_BY_ID_KEY, topicId, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, _workContext.CurrentCustomer.GetRolesIdent());
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var topic = _topicService.GetTopicById(topicId);
                if (topic == null || !topic.IsPublished)
                    return null;

                if (!_storeMappingService.Authorize(topic))
                    return null;

                if (!_aclService.Authorize(topic))
                    return null;

                return PrepareTopicModel(topic);
            });

            if (cacheModel == null || (!popup && cacheModel.RenderAsWidget))
                return HttpNotFound();

            ViewBag.IsPopup = popup;

            if (!cacheModel.RenderAsWidget)
            {
                Services.DisplayControl.Announce(new Topic { Id = cacheModel.Id });
            }

            return View("TopicDetails", cacheModel);
        }

        [ChildActionOnly]
        public ActionResult TopicBlock(string systemName, bool bodyOnly = false, bool isLead = false)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_BY_SYSTEMNAME_KEY, systemName.ToLower(), _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id, _workContext.CurrentCustomer.GetRolesIdent());
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id, true);
                if (topic == null || !topic.IsPublished)
                    return null;

                return PrepareTopicModel(topic);
            });

            if (cacheModel == null)
                return Content("");

            ViewBag.BodyOnly = bodyOnly;
            ViewBag.IsLead = isLead;

            if (!cacheModel.RenderAsWidget)
            {
                Services.DisplayControl.Announce(new Topic { Id = cacheModel.Id });
            }

            return PartialView(cacheModel);
        }

        [ChildActionOnly]
        public ActionResult TopicWidget(TopicWidgetModel model)
        {
            // Check for Cookie Consent
            if (model.CookieType != null)
            {
                var cookiesAllowed = _cookieManager.IsCookieAllowed(this.ControllerContext, (CookieType)model.CookieType);
                if (!cookiesAllowed)
                    return new EmptyResult();
            }
            
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Authenticate(int id, string password)
        {
            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = _topicService.GetTopicById(id);

            if (!_aclService.Authorize(topic))
            {
                topic = null;
            }

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
