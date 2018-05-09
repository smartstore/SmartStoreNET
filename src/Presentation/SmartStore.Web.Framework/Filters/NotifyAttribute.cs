using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Filters
{
	public class NotifyAttribute : FilterAttribute, IResultFilter
	{
		public const string NotificationsKey = "sm.notifications.all";

		public INotifier Notifier { get; set; }

		public virtual void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (Notifier == null || Notifier.Entries.Count == 0)
				return;

			if (!filterContext.IsChildAction && filterContext.HttpContext.Request.IsAjaxRequest())
			{
				HandleAjaxRequest(Notifier.Entries.FirstOrDefault(), filterContext.HttpContext.Response);
				return;
			}

			Persist(filterContext.Controller.ViewData, Notifier.Entries.Where(x => x.Durable == false));
			Persist(filterContext.Controller.TempData, Notifier.Entries.Where(x => x.Durable == true));

			Notifier.Entries.Clear();
		}

		public virtual void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}

		private void Persist(IDictionary<string, object> bag, IEnumerable<NotifyEntry> source)
		{
			if (!source.Any())
				return;

			var existing = (bag[NotificationsKey] ?? new List<NotifyEntry>()) as List<NotifyEntry>;
			
			source.Each(x => 
			{
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
