using System;
using System.Web.Mvc;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools.Filters
{
    public class MachineNameFilter : IResultFilter
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private readonly ProfilerSettings _profilerSettings;

        public MachineNameFilter(
            ICommonServices services,
            Lazy<IWidgetProvider> widgetProvider,
            ProfilerSettings profilerSettings)
        {
            _services = services;
            _widgetProvider = widgetProvider;
            _profilerSettings = profilerSettings;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!_profilerSettings.DisplayMachineName)
                return;

            if (filterContext.IsChildAction)
                return;

            var result = filterContext.Result;

            // should only run on a full view rendering result or HTML ContentResult
            if (!result.IsHtmlViewResult())
                return;

            if (!_services.WorkContext.CurrentCustomer.IsAdmin() && !filterContext.HttpContext.Request.IsLocal)
            {
                return;
            }

            _widgetProvider.Value.RegisterAction(
                new[] { "body_end_html_tag_before", "admin_content_after" },
                "MachineName",
                "DevTools",
                new { area = "SmartStore.DevTools" });
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

    }
}
