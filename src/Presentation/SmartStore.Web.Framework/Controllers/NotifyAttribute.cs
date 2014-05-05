using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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
		internal const string NotificationsKey = "sm.notifications.all";

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var notifier = EngineContext.Current.Resolve<INotifier>();

			if (notifier == null || !notifier.Entries.Any())
				return;

			if (filterContext.HttpContext.Request.IsAjaxRequest())
			{
				HandleAjaxRequest(notifier.Entries.FirstOrDefault(), filterContext.HttpContext.Response);
				return;
			}

			Persist(filterContext.Controller.ViewData, notifier.Entries.Where(x => x.Durable == false));
			Persist(filterContext.Controller.TempData, notifier.Entries.Where(x => x.Durable == true));
		}

		private void Persist(IDictionary<string, object> bag, IEnumerable<NotifyEntry> source)
		{
			if (!source.Any())
				return;

			var existing = (bag[NotificationsKey] ?? new List<NotifyEntry>()) as List<NotifyEntry>;
			
			source.Each(x => {
				if (x.Message.Text.HasValue() && !existing.Contains(x))
					existing.Add(x);
			});

			bag[NotificationsKey] = existing;
		}

		private void HandleAjaxRequest(NotifyEntry entry, HttpResponseBase response)
		{
			if (entry == null)
				return;

			response.AddHeader("X-Message-Type", entry.Type.ToString().ToLower());
			response.AddHeader("X-Message", entry.Message.Text);
		}

	}

}
