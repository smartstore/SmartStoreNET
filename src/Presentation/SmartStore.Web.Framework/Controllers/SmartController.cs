using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Controllers
{
    /// <remarks>
    /// <see cref="FilterAttribute.Order"/> for <see cref="JsonNetAttribute"/> must be -1, otherwise <see cref="JsonNetResult.ExecuteResult(ControllerContext)"/> is not executed.
    /// </remarks>
    [SetWorkingCulture(Order = 2)]
    [JsonNet(Order = -1)] // Run first (OnActionExecuted)
    [Notify(Order = 1000)] // Run last (OnResultExecuting)
    public abstract partial class SmartController : Controller
    {
        protected SmartController()
        {
        }

        public ILogger Logger
        {
            get;
            set;
        } = NullLogger.Instance;

        public Localizer T
        {
            get;
            set;
        } = NullLocalizer.Instance;

        public ICommonServices Services
        {
            get;
            set;
        }

        /// <summary>
        /// Pushes an info message to the notification queue
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
        protected virtual void NotifyInfo(string message, bool durable = true)
        {
            Services.Notifier.Information(message, durable);
        }

        /// <summary>
        /// Pushes a warning message to the notification queue
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
        protected virtual void NotifyWarning(string message, bool durable = true)
        {
            Services.Notifier.Warning(message, durable);
        }

        /// <summary>
        /// Pushes a success message to the notification queue
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
        protected virtual void NotifySuccess(string message, bool durable = true)
        {
            Services.Notifier.Success(message, durable);
        }

        /// <summary>
        /// Pushes an error message to the notification queue
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="durable">A value indicating whether the message should be persisted for the next request</param>
        protected virtual void NotifyError(string message, bool durable = true)
        {
            Services.Notifier.Error(message, durable);
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

            Services.Notifier.Error(HttpUtility.HtmlEncode(exception.ToAllMessages()), durable);
        }

        /// <summary>
        /// Pushes an error message to the notification queue that the access to a resource has been denied
        /// </summary>
        /// <param name="durable">A value indicating whether a message should be persisted for the next request</param>
        /// <param name="log">A value indicating whether the message should be logged</param>
        protected virtual void NotifyAccessDenied(bool durable = true, bool log = true)
        {
            var message = T("Admin.AccessDenied.Description");

            if (log)
            {
                Logger.Error(message);
            }

            Services.Notifier.Error(message, durable);
        }

        protected ActionResult RedirectToReferrer()
        {
            return RedirectToReferrer(null, () => RedirectToRoute("HomePage"));
        }

        protected ActionResult RedirectToReferrer(string referrer)
        {
            return RedirectToReferrer(referrer, () => RedirectToRoute("HomePage"));
        }

        protected ActionResult RedirectToReferrer(string referrer, string fallbackUrl)
        {
            // addressing "Open Redirection Vulnerability" (prevent cross-domain redirects / phishing)
            if (fallbackUrl.HasValue() && !Url.IsLocalUrl(fallbackUrl))
            {
                fallbackUrl = null;
            }

            return RedirectToReferrer(
                referrer,
                fallbackUrl.HasValue() ? () => Redirect(fallbackUrl) : (Func<ActionResult>)null);
        }

        protected virtual ActionResult RedirectToReferrer(string referrer, Func<ActionResult> fallbackResult)
        {
            bool skipLocalCheck = false;

            if (referrer.IsEmpty() && Request.UrlReferrer != null && Request.UrlReferrer.ToString().HasValue())
            {
                referrer = Request.UrlReferrer.ToString();
                if (referrer.HasValue())
                {
                    var domain1 = (new Uri(referrer)).GetLeftPart(UriPartial.Authority);
                    var domain2 = this.Request.Url.GetLeftPart(UriPartial.Authority);
                    if (domain1.IsCaseInsensitiveEqual(domain2))
                    {
                        // always allow fully qualified urls from local host
                        skipLocalCheck = true;
                    }
                    else
                    {
                        referrer = null;
                    }
                }
            }

            // addressing "Open Redirection Vulnerability" (prevent cross-domain redirects / phishing)
            if (referrer.HasValue() && !skipLocalCheck && !Url.IsLocalUrl(referrer))
            {
                referrer = null;
            }

            if (referrer.HasValue())
            {
                return Redirect(referrer);
            }

            if (fallbackResult != null)
            {
                return fallbackResult();
            }

            return HttpNotFound();
        }

        /// <summary>
        /// Redirects to the configuration page of a plugin or a provider.
        /// </summary>
        /// <param name="systemName">The system name of the plugin or the provider.</param>
        /// <param name="isPlugin"><c>true</c> plugin configuration, <c>false</c> provider configuration.</param>
        protected virtual ActionResult RedirectToConfiguration(string systemName, bool isPlugin = true)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            var actionName = isPlugin ? "ConfigurePlugin" : "ConfigureProvider";

            if (ControllerContext.IsChildAction)
            {
                var url = Url.Action(actionName, "Plugin", new { systemName, area = "Admin" });
                return new PermissiveRedirectResult(url);
            }

            return RedirectToAction(actionName, "Plugin", new { systemName, area = "Admin" });
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
        /// <param name="ex">Exception</param>
        private void LogException(Exception ex)
        {
            Logger.Error(ex);
        }

        ///// <summary>
        ///// Creates a <see cref="JsonResult"/> object that serializes the specified object to JavaScript Object Notation (JSON) format using the content type, 
        ///// content encoding, and the JSON request behavior.
        ///// </summary>
        ///// <param name="data">The JavaScript object graph to serialize.</param>
        ///// <param name="contentType">The content type (MIME type).</param>
        ///// <param name="contentEncoding">The content encoding.</param>
        ///// <param name="behavior">The JSON request behavior</param>
        ///// <returns>The result object that serializes the specified object to JSON format.</returns>
        ///// <remarks>
        ///// This overridden method internally uses the Json.NET library for serialization.
        ///// </remarks>
        //protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        //{
        //	return new JsonNetResult(Services.DateTimeHelper)
        //	{
        //		Data = data,
        //		ContentType = contentType,
        //		ContentEncoding = contentEncoding,
        //		JsonRequestBehavior = behavior
        //	};
        //}
    }
}
