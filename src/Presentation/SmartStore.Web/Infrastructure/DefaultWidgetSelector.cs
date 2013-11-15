using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Topics;

namespace SmartStore.Web.Infrastructure
{
    
    public partial class DefaultWidgetSelector : IWidgetSelector
    {

        #region Fields

        private readonly IWidgetService _widgetService;
        private readonly ITopicService _topicService;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;

        #endregion

        public DefaultWidgetSelector(
            IWidgetService widgetService, 
            ITopicService topicService, 
            IStoreContext storeContext, 
            ICacheManager cacheManager, 
            IWorkContext workContext, 
            IDbContext dbContext)
        {
            this._widgetService = widgetService;
            this._topicService = topicService;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._workContext = workContext;
            this._dbContext = dbContext;
        }

        public virtual IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone)
        {
            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, _storeContext.CurrentStore.Id);
            foreach (var widget in widgets)
            {
                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                widget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);

                yield return new WidgetRouteInfo 
                { 
                    ActionName = actionName, 
                    ControllerName = controllerName, 
                    RouteValues = routeValues 
                };
            }

            // add special "topic widgets" to the list
            var allTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_ALL_MODEL_KEY, _storeContext.CurrentStore.Id);
            var topicWidgets = _cacheManager.Get(allTopicsCacheKey, () =>
            {
                var result = _topicService.GetAllTopics(_storeContext.CurrentStore.Id);
                var list = result.Where(x => x.RenderAsWidget).ToList();
                list.Each(x => _dbContext.DetachEntity(x));
                return list;
            });

            var byZoneTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_BYZONE_MODEL_KEY, widgetZone, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
            var topicsByZone = _cacheManager.Get(byZoneTopicsCacheKey, () =>
            {
                var result = from t in topicWidgets
                             where t.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase)
                             orderby t.Priority
                             select new WidgetRouteInfo
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

                return result; 
            });

            foreach (var topicWidget in topicsByZone)
            {
                yield return topicWidget;
            }
        }

    }

}