using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
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

        private readonly Lazy<IEnumerable<IWidget>> _simpleWidgets;
        private static ConcurrentDictionary<string, IEnumerable<string>> s_simpleWidgetsMap;
        private static bool? s_hasSimpleWidgets;
        private static readonly object _lock = new object();

        #endregion

        public DefaultWidgetSelector(
            IWidgetService widgetService, 
            ITopicService topicService, 
            IStoreContext storeContext, 
            ICacheManager cacheManager, 
            IWorkContext workContext, 
            IDbContext dbContext,
            Lazy<IEnumerable<IWidget>> simpleWidgets)
        {
            this._widgetService = widgetService;
            this._topicService = topicService;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._workContext = workContext;
            this._dbContext = dbContext;
            this._simpleWidgets = simpleWidgets;
        }

        public virtual IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone)
        {		
			string actionName;
            string controllerName;
            RouteValueDictionary routeValues;

            #region Plugin Widgets

            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, _storeContext.CurrentStore.Id);
            foreach (var widget in widgets)
            {
                widget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);

                yield return new WidgetRouteInfo 
                { 
                    ActionName = actionName, 
                    ControllerName = controllerName, 
                    RouteValues = routeValues 
                };
            }

            #endregion

            #region Topic Widgets

            // add special "topic widgets" to the list
			var allTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_ALL_MODEL_KEY, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
            var topicWidgets = _cacheManager.Get(allTopicsCacheKey, () =>
            {
				var allTopicWidgets = _topicService.GetAllTopics(_storeContext.CurrentStore.Id).Where(x => x.RenderAsWidget).ToList();
				var stubs = allTopicWidgets
					.Select(t => new TopicWidgetStub 
					{
 						Id = t.Id,
						Bordered = t.WidgetBordered,
						ShowTitle = t.WidgetShowTitle,
						SystemName = t.SystemName.SanitizeHtmlId(),
						Title = t.GetLocalized(x => t.Title),
						Body = t.GetLocalized(x => t.Body),
						WidgetZones = t.GetWidgetZones().ToArray(),
						Priority = t.Priority
					})
					.ToList();
                return stubs;
            });

            var byZoneTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_BYZONE_MODEL_KEY, widgetZone, _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
            var topicsByZone = _cacheManager.Get(byZoneTopicsCacheKey, () =>
            {
                var result = from t in topicWidgets
                             where t.WidgetZones.Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase)
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
                                        SystemName = t.SystemName,
                                        ShowTitle = t.ShowTitle,
                                        IsBordered = t.Bordered,
                                        Title = t.Title,
                                        Html = t.Body
                                    } }
                                 }
                             };

                return result.ToList(); 
            });

            foreach (var topicWidget in topicsByZone)
            {
                yield return topicWidget;
            }

            #endregion

            #region Simple Widgets

            if (s_hasSimpleWidgets.HasValue && s_hasSimpleWidgets.Value == false)
            {
                yield break;
            }

            if (!s_hasSimpleWidgets.HasValue)
            {
                InitSimpleWidgetsMap(_simpleWidgets.Value);
                if (s_hasSimpleWidgets.Value == false)
                {
                    yield break;
                }
            }

            if (s_simpleWidgetsMap.ContainsKey(widgetZone))
            {
                var widgetNames = s_simpleWidgetsMap[widgetZone];
                foreach (var widgetName in widgetNames)
                {
                    // Simple widgets has been registered with their type full names before
                    var simpleWidget = EngineContext.Current.Resolve<IWidget>(widgetName);
                    if (simpleWidget != null)
                    {
                        simpleWidget.GetDisplayWidgetRoute(widgetZone, out actionName, out controllerName, out routeValues);

                        yield return new WidgetRouteInfo
                        {
                            ActionName = actionName,
                            ControllerName = controllerName,
                            RouteValues = routeValues
                        };
                    }
                }
            }

            #endregion
        }

        #region Simple Widgets

        private static void InitSimpleWidgetsMap(IEnumerable<IWidget> widgets)
        {
            lock (_lock)
            {
                if (!s_hasSimpleWidgets.HasValue) // double check
                {
                    if (!widgets.Any())
                    {
                        s_hasSimpleWidgets = false;
                        return;
                    }
                    
                    var map = new Multimap<string, SimpleWidgetStub>();

                    foreach (var widget in widgets)
                    {
                        var zones = widget.GetWidgetZones();
                        if (zones != null & zones.Any())
                        {
                            foreach (var zone in zones.Select(x => x.ToLower()))
                            {
                                int ordinal = 0;
                                var ordered = widget as IOrdered;
                                if (ordered != null)
                                {
                                    ordinal = ordered.Ordinal;
                                }
                                map.Add(zone, new SimpleWidgetStub { Widget = widget, Ordinal = ordinal });
                            }
                        }
                    }

                    var orderedMap = from x in map
                                     select new KeyValuePair<string, IEnumerable<string>>(
                                         x.Key, 
                                         x.Value.OrderBy(w => w.Ordinal).Select(w => w.Widget.GetType().FullName));

                    s_simpleWidgetsMap = new ConcurrentDictionary<string, IEnumerable<string>>(orderedMap.ToList(), StringComparer.InvariantCultureIgnoreCase);

                    s_hasSimpleWidgets = s_simpleWidgetsMap.Count > 0;
                }
            }
        }

        class SimpleWidgetStub
        {
            public IWidget Widget { get; set; }
            public int Ordinal { get; set; }
        }

		class TopicWidgetStub
		{
			public int Id { get; set; }
			public string[] WidgetZones { get; set; }
			public string SystemName { get; set; }
			public bool ShowTitle { get; set; }
			public bool Bordered { get; set; }
			public string Title { get; set; }
			public string Body { get; set; }
			public int Priority { get; set; }
		}

        #endregion

    }

}