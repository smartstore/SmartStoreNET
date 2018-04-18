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
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Topics;
using SmartStore.Core.Domain.Topics;

namespace SmartStore.Web.Infrastructure
{  
    public partial class DefaultWidgetSelector : IWidgetSelector
    {
        #region Fields

        private readonly IWidgetService _widgetService;
        private readonly ITopicService _topicService;
        private readonly IStoreContext _storeContext;
        private readonly IRequestCache _requestCache;
        private readonly IWorkContext _workContext;
        private readonly IDbContext _dbContext;
		private readonly IWidgetProvider _widgetProvider;
		private readonly ICommonServices _services;

        #endregion

        public DefaultWidgetSelector(
            IWidgetService widgetService, 
            ITopicService topicService, 
            IStoreContext storeContext,
			IRequestCache requestCache, 
            IWorkContext workContext, 
            IDbContext dbContext,
			IWidgetProvider widgetProvider,
			ICommonServices services)
        {
            this._widgetService = widgetService;
            this._topicService = topicService;
            this._storeContext = storeContext;
            this._requestCache = requestCache;
            this._workContext = workContext;
            this._dbContext = dbContext;
			this._widgetProvider = widgetProvider;
			this._services = services;
        }

        public virtual IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone, object model)
        {
			string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
			var storeId = _storeContext.CurrentStore.Id;

            #region Plugin Widgets

			var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone, storeId);
            foreach (var widget in widgets)
            {
                widget.Value.GetDisplayWidgetRoute(widgetZone, model, storeId, out actionName, out controllerName, out routeValues);

				if (actionName.HasValue() && controllerName.HasValue())
				{
					yield return new WidgetRouteInfo
					{
						ActionName = actionName,
						ControllerName = controllerName,
						RouteValues = routeValues
					};
				}
            }

            #endregion

            #region Topic Widgets

            // add special "topic widgets" to the list
			var allTopicsCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_WIDGET_ALL_MODEL_KEY, storeId, _workContext.WorkingLanguage.Id);
            // get topic widgets from STATIC cache
			var topicWidgets = _services.Cache.Get(allTopicsCacheKey, () =>
            {
				using (var scope = new DbContextScope(forceNoTracking: true))
				{
					var allTopicWidgets = _topicService.GetAllTopics(storeId).Where(x => x.RenderAsWidget).ToList();
					var stubs = allTopicWidgets
						.Select(t => 
						{
							var locTitle = t.GetLocalized(x => t.Title);
							var locBody = t.GetLocalized(x => t.Body, detectEmptyHtml: true);
							return new TopicWidgetStub
							{
								Id = t.Id,
								Bordered = t.WidgetBordered,
								WrapContent = !t.WidgetWrapContent.HasValue || t.WidgetWrapContent.Value,
								ShowTitle = t.WidgetShowTitle,
								SystemName = t.SystemName.SanitizeHtmlId(),
								Title = locTitle,
								TitleRtl = locTitle.CurrentLanguage.Rtl,
								Body = locBody,
								BodyRtl = locBody.CurrentLanguage.Rtl,
								TitleTag = t.TitleTag,
								WidgetZones = t.GetWidgetZones().ToArray(),
								Priority = t.Priority
							};
						})
						.OrderBy(t => t.Priority)
						.ToList();
					return stubs;
				}
            });

            var byZoneTopicsCacheKey = "SmartStore.TopicWidgets.ZoneMapped";
            // save widgets to zones map in request cache
			var topicsByZone = _requestCache.Get(byZoneTopicsCacheKey, () =>
            {
				var map = new Multimap<string, WidgetRouteInfo>();

				foreach (var widget in topicWidgets)
				{
					var zones = widget.WidgetZones;
					if (zones != null && zones.Any())
					{
						foreach (var zone in zones.Select(x => x.ToLower()))
						{
							var routeInfo = new WidgetRouteInfo
							{
								ControllerName = "Topic",
								ActionName = "TopicWidget",
								RouteValues = new RouteValueDictionary()
								{
									{"Namespaces", "SmartStore.Web.Controllers"},
									{"area", null},
									{"widgetZone", zone},
									{"model", new TopicWidgetModel 
										{ 
											Id = widget.Id,
											SystemName = widget.SystemName,
											WrapContent = widget.WrapContent,
											ShowTitle = widget.ShowTitle,
											IsBordered = widget.Bordered,
											Title = !widget.Title.HasValue() ? null : widget.Title,
											TitleTag = widget.TitleTag ?? "h3",
											Html = widget.Body,
											HtmlRtl = widget.BodyRtl,
											TitleRtl = widget.TitleRtl
										}
									}
								}
							};

							map.Add(zone, routeInfo);
						}
					}
				}

				return map;
			});

			if (topicsByZone.ContainsKey(widgetZone.ToLower()))
			{
				var zoneWidgets = topicsByZone[widgetZone.ToLower()];
				foreach (var topicWidget in zoneWidgets)
				{
					// Handle OC announcement
					var topicWidgetModel = topicWidget.RouteValues["model"] as TopicWidgetModel;
					if (topicWidgetModel != null)
					{
						_services.DisplayControl.Announce(new Topic { Id = topicWidgetModel.Id });
					}

					yield return topicWidget;
				}
			}

            #endregion


			#region Request scoped widgets (provided by IWidgetProvider)

			var requestScopedWidgets = _widgetProvider.GetWidgets(widgetZone);
			if (requestScopedWidgets != null)
			{
				foreach (var widget in requestScopedWidgets)
				{
					yield return widget;
				}
			}

			#endregion
        }
    }

	public class TopicWidgetStub
	{
		public int Id { get; set; }
		public string[] WidgetZones { get; set; }
		public string SystemName { get; set; }
		public bool WrapContent { get; set; }
		public bool ShowTitle { get; set; }
		public bool Bordered { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public bool TitleRtl { get; set; }
		public bool BodyRtl { get; set; }

		public string TitleTag { get; set; }
		public int Priority { get; set; }
	}
}