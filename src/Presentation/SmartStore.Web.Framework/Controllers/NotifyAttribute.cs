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

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			var notifier = EngineContext.Current.Resolve<INotifier>();

			if (notifier != null && !notifier.Entries.Any())
				return;

			var entries = notifier.Entries.Where(x => x.Message.ToString().HasValue()).ToList();

			string key = "sm.notifications.all";

			filterContext.Controller.TempData[key] = entries.Where(x => x.Durable).ToList();
			filterContext.Controller.ViewData[key] = entries.Where(x => !x.Durable).ToList();
		}

	}

}
