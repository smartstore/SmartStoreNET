using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Controllers
{
	[SetWorkingCulture]
	[Notify]
	public abstract partial class SmartController : Controller
	{
		private readonly Lazy<INotifier> _notifier;

		protected SmartController()
		{
			this.Logger = NullLogger.Instance;
			this.T = NullLocalizer.Instance;

			this._notifier = EngineContext.Current.Resolve<Lazy<INotifier>>();
		}

		public ILogger Logger { get; set; }
		public Localizer T { get; set; }
		
		/// <summary>
		/// Pushes an info message to the notification queue
		/// </summary>
		/// <param name="message">Message</param>
		/// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
		protected virtual void NotifyInfo(string message, bool durable = true)
		{
			_notifier.Value.Information(message, durable);
		}

		/// <summary>
		/// Pushes a warning message to the notification queue
		/// </summary>
		/// <param name="message">Message</param>
		/// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
		protected virtual void NotifyWarning(string message, bool durable = true)
		{
			_notifier.Value.Warning(message, durable);
		}

		/// <summary>
		/// Pushes a success message to the notification queue
		/// </summary>
		/// <param name="message">Message</param>
		/// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
		protected virtual void NotifySuccess(string message, bool durable = true)
		{
			_notifier.Value.Success(message, durable);
		}

		/// <summary>
		/// Pushes an error message to the notification queue
		/// </summary>
		/// <param name="message">Message</param>
		/// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
		protected virtual void NotifyError(string message, bool durable = true)
		{
			_notifier.Value.Error(message, durable);
		}

		/// <summary>
		/// Pushes an error message to the notification queue
		/// </summary>
		/// <param name="exception">Exception</param>
		/// <param name="durable">A value indicating whether a message should be persisted for the next request</param>
		/// <param name="logException">A value indicating whether the exception should be logged</param>
		protected virtual void NotifyError(Exception exception, bool durable = true, bool logException = true)
		{
			if (logException)
			{
				LogException(exception);
			}

			_notifier.Value.Error(exception.ToAllMessages(), durable);
		}

		protected virtual ActionResult RedirectToReferrer()
		{
			if (Request.UrlReferrer != null && Request.UrlReferrer.ToString().HasValue())
			{
				return Redirect(Request.UrlReferrer.ToString());
			}

			return RedirectToRoute("HomePage");
		}

		protected virtual ActionResult RedirectToHomePageWithError(string reason, bool durable = true)
		{
			string message = T("Common.RequestProcessingFailed", this.RouteData.Values["controller"], this.RouteData.Values["action"], reason.NaIfEmpty());

			_notifier.Value.Error(message, durable);

			return RedirectToRoute("HomePage");
		}

		/// <summary>
		/// On exception
		/// </summary>
		/// <param name="filterContext">Filter context</param>
		protected override void OnException(ExceptionContext filterContext)
		{
			if (filterContext.Exception != null)
			{
				LogException(filterContext.Exception);
			}

			base.OnException(filterContext);
		}

		/// <summary>
		/// Log exception
		/// </summary>
		/// <param name="exc">Exception</param>
		private void LogException(Exception exc)
		{
			var workContext = EngineContext.Current.Resolve<IWorkContext>();

			var customer = workContext.CurrentCustomer;
			Logger.Error(exc.Message, exc, customer);
		}
	}
}
