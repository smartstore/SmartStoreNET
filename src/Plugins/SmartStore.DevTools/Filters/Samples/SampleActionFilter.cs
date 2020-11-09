using System.Diagnostics;
using System.Web.Mvc;
using SmartStore.Core.Logging;

namespace SmartStore.DevTools.Filters
{
    public class SampleActionFilter : IActionFilter
    {
        private readonly INotifier _notifier;

        public SampleActionFilter(INotifier notifier)
        {
            this._notifier = notifier;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Debug.WriteLine("Executing: {0} - {1}".FormatInvariant(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor.ActionName));
            _notifier.Information("Yeah, my plugin action filter works. NICE!");
            // Do something meaningful here ;-)
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Debug.WriteLine("Executed: {0} - {1}".FormatInvariant(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor.ActionName));
        }
    }
}
