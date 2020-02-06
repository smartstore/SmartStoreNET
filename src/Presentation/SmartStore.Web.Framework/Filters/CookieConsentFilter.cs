using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Filters
{
	public enum CookieConsentStatus
	{
		Unset = -1,
		Asked = 0,
		Consented = 1,
		Denied = 2
	}

	public class CookieConsentFilter : IActionFilter, IResultFilter
	{
		private readonly IUserAgent _userAgent;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly PrivacySettings _privacySettings;

		private bool _isProcessableRequest;
		private CookieConsentStatus _consentStatus;

		public CookieConsentFilter(
			IUserAgent userAgent,
			ICommonServices services,
			Lazy<IWidgetProvider> widgetProvider,
			PrivacySettings privacySettings)
		{
			_userAgent = userAgent;
			_services = services;
			_widgetProvider = widgetProvider;
			_privacySettings = privacySettings;

			_consentStatus = CookieConsentStatus.Unset;
		}

		private bool IsProcessableRequest(ControllerContext controllerContext)
		{
			if (!_privacySettings.EnableCookieConsent)
				return false;

			if (controllerContext.IsChildAction)
				return false;

			var request = controllerContext.HttpContext?.Request;

			if (request == null)
				return false;

			if (request.IsAjaxRequest())
				return false;

			if (!String.Equals(request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
				return false;

			return true;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_isProcessableRequest = IsProcessableRequest(filterContext);

			if (!_isProcessableRequest)
				return;

			var request = filterContext.HttpContext.Request;
			var response = filterContext.HttpContext.Response;

			// Check if the user has a consent cookie
			var consentCookie = request.Cookies[CookieConsent.ConsentCookieName];
			if (consentCookie == null)
			{
				// No consent cookie. We first check the Do Not Track header value, this can have the value "0" or "1"
				var doNotTrack = request.Headers.Get("DNT");

				// If we receive a DNT header, we accept its value (0 = give consent, 1 = deny) and do not ask the user anymore...
				if (doNotTrack.HasValue())
				{
					_consentStatus = doNotTrack == "0" 
						? CookieConsentStatus.Consented 
						: CookieConsentStatus.Denied;
                }
				else
				{
					if (_userAgent.IsBot)
					{
						// Don't ask consent from search engines, also don't set cookies
						_consentStatus = CookieConsentStatus.Consented;
					}
					else
					{
						// First request on the site and no DNT header (we use session cookie, which is allowed by EU cookie law).
						_consentStatus = CookieConsentStatus.Asked;
						CookieConsent.SetCookie(response, _consentStatus);
					}
				}
			}
			else
			{
				// We received a consent cookie

				var cookieSet = false;

				if (int.TryParse(consentCookie.Value, out var i)) 
				{
					_consentStatus = (CookieConsentStatus)i;
				}
				else
				{
					// Legacy
					var str = consentCookie.Value;

					if (str == "asked")
						_consentStatus = CookieConsentStatus.Asked;
					else if (str == "true")
						_consentStatus = CookieConsentStatus.Consented;
					else
						_consentStatus = CookieConsentStatus.Denied;

					// Fix legacy value
					CookieConsent.SetCookie(response, _consentStatus);
					cookieSet = true;
				}
				
				if (_consentStatus == CookieConsentStatus.Asked && !cookieSet)
				{
					// Consent has been asked for
					CookieConsent.SetCookie(response, _consentStatus);
				}
			}

			filterContext.HttpContext.Items[CookieConsent.ConsentCookieName] = _consentStatus;
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_isProcessableRequest)
				return;

			// Should only run on a full view rendering result or HTML ContentResult
			if (!filterContext.Result.IsHtmlViewResult())
				return;

			// Always render the child action because of output caching
			_widgetProvider.Value.RegisterAction(
				new[] { "body_end_html_tag_before" },
				"CookieConsentBadge",
				"Common",
				new { area = "" });
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}

	public static class CookieConsent
	{
		public const string ConsentCookieName = "CookieConsent";

		public static void SetCookie(HttpResponseBase response, CookieConsentStatus status)
		{
			if (status == CookieConsentStatus.Unset)
				return;

			var expiry = status == CookieConsentStatus.Asked ? TimeSpan.FromMinutes(10) : TimeSpan.FromDays(365);

			var consentCookie = new HttpCookie(CookieConsent.ConsentCookieName)
			{
				Value = ((int)status).ToString(),
				Expires = DateTime.UtcNow + expiry
			};

			response.Cookies.Set(consentCookie);
		}

		public static CookieConsentStatus GetStatus(ControllerContext context)
		{
			if (context.HttpContext.Items.Contains(ConsentCookieName))
			{
				return context.HttpContext.GetItem<CookieConsentStatus>(ConsentCookieName);
			}

			var cookie = context.HttpContext.Request.Cookies[ConsentCookieName];
			if (cookie != null && int.TryParse(cookie.Value, out var i))
			{
				return (CookieConsentStatus)i;
			}

			return CookieConsentStatus.Unset;
		}
	}
}