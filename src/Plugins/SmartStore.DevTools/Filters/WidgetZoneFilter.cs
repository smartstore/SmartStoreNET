using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Caching;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Utilities;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools.Filters
{
    public class WidgetZoneFilter : IActionFilter, IResultFilter
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private readonly ProfilerSettings _profilerSettings;
        private readonly IDisplayControl _displayControl;

        public WidgetZoneFilter(
            ICommonServices services,
            Lazy<IWidgetProvider> widgetProvider,
            ProfilerSettings profilerSettings,
            IDisplayControl displayControl)
        {
            _services = services;
            _widgetProvider = widgetProvider;
            _profilerSettings = profilerSettings;
            _displayControl = displayControl;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            if (!_profilerSettings.DisplayWidgetZones)
                return;

            _displayControl.MarkRequestAsUncacheable();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!_profilerSettings.DisplayWidgetZones)
                return;

            // should only run on a full view rendering result
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
            {
                return;
            }

            if (!this.ShouldRender(filterContext.HttpContext))
            {
                return;
            }

            if (!filterContext.IsChildAction)
            {
                _widgetProvider.Value.RegisterAction(
                    new Wildcard("*"),
                    "WidgetZone",
                    "DevTools",
                    new { area = "SmartStore.DevTools" });
            }

            var viewName = result.ViewName;

            if (viewName.IsEmpty())
            {
                string action = (filterContext.RouteData.Values["action"] as string).EmptyNull();
                viewName = action;

                if (action == "WidgetsByZone")
                {
                    var model = result.Model as WidgetZoneModel;

                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Plugins/SmartStore.DevTools/Views/DevTools/WidgetZone.cshtml",
                    };

                    if (filterContext.RouteData.Values["widgetZone"] == null)
                    {
                        filterContext.RouteData.Values.Add("widgetZone", model.WidgetZone);
                    }
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        private bool ShouldRender(HttpContextBase ctx)
        {
            if (!_services.WorkContext.CurrentCustomer.IsAdmin())
            {
                if (ctx.Request.Url.AbsolutePath == "/common/pdfreceiptfooter" || ctx.Request.Url.AbsolutePath == "/common/pdfreceiptheader")
                {
                    return false;
                }
                return ctx.Request.IsLocal;
            }

            return true;
        }

    }
}
