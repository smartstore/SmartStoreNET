using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Controllers
{

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public class NotifyAttribute : ActionFilterAttribute
	{
		private ICollection<NotifyEntry> _entries;
		
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;
			
			var notifier = EngineContext.Current.Resolve<INotifier>();

			if (notifier != null && notifier.Entries.Any())
				_entries = notifier.Entries;
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;
			
			if (_entries == null || !_entries.Any())
				return;

			var viewResult = filterContext.Result;

			// if it's not a view result, a redirect for example
			if (!(viewResult is ViewResultBase || viewResult is RedirectResult || viewResult is RedirectToRouteResult))
				return;

			var entries = _entries.Where(x => x.Message.ToString().HasValue()).ToList();

			string key = "sm.notifications.all";

			filterContext.Controller.TempData[key] = entries.Where(x => x.Durable);
			filterContext.Controller.ViewData[key] = entries.Where(x => !x.Durable);
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);
		}

	}

}
