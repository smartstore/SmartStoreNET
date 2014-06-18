using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Core.Localization;

namespace SmartStore.Plugin.Developer.DevTools.Filters
{
	public class TestActionFilter : IActionFilter
	{
		private readonly INotifier _notifier;

		public TestActionFilter(INotifier notifier)
		{
			this._notifier = notifier;
		}
		
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			Debug.WriteLine("Executing: {0} - {1}".FormatInvariant(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor.ActionName));
			_notifier.Information("Yeah, my plugin action filter works. NICE!");
		}
		
		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			Debug.WriteLine("Executed: {0} - {1}".FormatInvariant(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor.ActionName));
		}
	}
}
