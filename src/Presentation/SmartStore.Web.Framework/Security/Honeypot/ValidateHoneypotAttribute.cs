using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Security
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class ValidateHoneypotAttribute : FilterAttribute, IAuthorizationFilter
    {
		public ValidateHoneypotAttribute()
		{
			Logger = NullLogger.Instance;
		}

		public SecuritySettings SecuritySettings { get; set; }
		public ILogger Logger { get; set; }
		public Localizer T { get; set; }
		public Lazy<IWebHelper> WebHelper { get; set; }

		public void OnAuthorization(AuthorizationContext filterContext)
		{
			if (!SecuritySettings.EnableHoneypotProtection)
				return;

			var isBot = Honeypot.IsBot(filterContext.HttpContext);
			if (!isBot)
				return;
			
			Logger.Warn("Honeypot detected a bot and rejected the request.");

			var redirectUrl = WebHelper.Value.GetThisPageUrl(true);
			filterContext.Result = new RedirectResult(redirectUrl);
		}
	}
}
