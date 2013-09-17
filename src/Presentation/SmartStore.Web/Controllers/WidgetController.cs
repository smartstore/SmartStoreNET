using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Cms;
using SmartStore.Web.Models.Cms;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core;
using SmartStore.Utilities;
using SmartStore.Web.Models.Topics;
using SmartStore.Services.Topics;
using SmartStore.Services.Localization;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Core.Caching;
using System;

namespace SmartStore.Web.Controllers
{
    public partial class WidgetController : SmartController
    {
		#region Fields

        private readonly IWidgetService _widgetService;
        private readonly ITopicService _topicService;
		private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;

        #endregion

		#region Constructors

        public WidgetController(IWidgetService widgetService, ITopicService topicService, IStoreContext storeContext, ICacheManager cacheManager, IWorkContext workContext)
        {
            this._widgetService = widgetService;
            this._topicService = topicService;
			this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult WidgetsByZone(string widgetZone)
        {
            //model
            var model = new List<RenderWidgetModel>();

			var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, _storeContext.CurrentStore.Id);
            foreach (var widget in widgets)
            {
                var widgetModel = new RenderWidgetModel();

                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                widget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);
                widgetModel.ActionName = actionName;
                widgetModel.ControllerName = controllerName;
                widgetModel.RouteValues = routeValues;

                model.Add(widgetModel);
            }

            // add special "topic widgets" to the list
            var allTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_ALL_MODEL_KEY, _storeContext.CurrentStore.Id);
            var topicWidgets = _cacheManager.Get(allTopicsCacheKey, () =>
            {
                var result = _topicService.GetAllTopics(_storeContext.CurrentStore.Id);
                return result.Where(x => x.RenderAsWidget).ToList();
            });

            var byZoneTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_BYZONE_MODEL_KEY, widgetZone, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
            var topicsByZone = _cacheManager.Get(byZoneTopicsCacheKey, () => 
            {
                var result = from t in topicWidgets
                             where t.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase)
                             select new RenderWidgetModel
                             {
                                 ControllerName = "Topic",
                                 ActionName = "TopicWidget",
                                 RouteValues = new RouteValueDictionary()
                                 {
                                    {"Namespaces", "SmartStore.Web.Controllers"},
                                    {"area", null},
                                    {"widgetZone", widgetZone},
                                    {"model", new TopicWidgetModel 
                                    { 
                                        Id = t.Id,
                                        SystemName = t.SystemName.SanitizeHtmlId(),
                                        ShowTitle = t.WidgetShowTitle,
                                        IsBordered = t.WidgetBordered,
                                        Title = t.GetLocalized(x => t.Title),
                                        Html = t.GetLocalized(x => t.Body)
                                    } }
                                 }
                             };

                return result.ToList();
            });

            model.AddRange(topicsByZone);

            return PartialView(model);
        }

        #endregion
    }
}
