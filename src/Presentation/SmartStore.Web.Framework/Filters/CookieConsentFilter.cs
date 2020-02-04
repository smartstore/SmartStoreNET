using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Filters
{
    public class CookieConsentFilter : IActionFilter, IResultFilter
	{
		enum ConsentLevel
		{
			Unasked = -1,
			Asked = 0,
			Consented = 1
		}
		
		private readonly IUserAgent _userAgent;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly PrivacySettings _privacySettings;

		private ConsentLevel _consentLevel;

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

			_consentLevel = ConsentLevel.Unasked;
		}

		private bool IsProcessableRequest(ControllerContext controllerContext)
		{
			if (!_privacySettings.EnableCookieConsent)
				return false;

			if (controllerContext.IsChildAction)
				return false;

			if (controllerContext.HttpContext?.Request == null)
				return false;

			if (!String.Equals(controllerContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
				return false;

			return true;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!IsProcessableRequest(filterContext))
				return;

			var viewBag = filterContext.Controller.ViewBag;
			viewBag.AskCookieConsent = true;
			viewBag.HasCookieConsent = false;

			var request = filterContext.HttpContext.Request;

			// Check if the user has a consent cookie
			var consentCookie = request.Cookies[CookieConsent.CONSENT_COOKIE_NAME];
			if (consentCookie == null)
			{
				// No consent cookie. We first check the Do Not Track header value, this can have the value "0" or "1"
				string dnt = request.Headers.Get("DNT");

				// If we receive a DNT header, we accept its value and do not ask the user anymore
				if (!String.IsNullOrEmpty(dnt))
				{
					viewBag.AskCookieConsent = false;
					viewBag.HasCookieConsent = dnt == "0";
				}
				else
				{
					if (_userAgent.IsBot)
					{
						// don't ask consent from search engines, also don't set cookies
						viewBag.AskCookieConsent = false;
						_consentLevel = ConsentLevel.Consented;
					}
					else
					{
						// first request on the site and no DNT header. 
						consentCookie = new HttpCookie(CookieConsent.CONSENT_COOKIE_NAME);
						consentCookie.Value = "asked";
						filterContext.HttpContext.Response.Cookies.Add(consentCookie);
						_consentLevel = ConsentLevel.Asked;
					}
				}
			}
			else
			{
				// we received a consent cookie
				viewBag.AskCookieConsent = false;
				if (consentCookie.Value == "asked")
				{
					// consent has been asked for
					consentCookie.Expires = DateTime.UtcNow.AddYears(1);
					filterContext.HttpContext.Response.Cookies.Set(consentCookie);
					_consentLevel = ConsentLevel.Asked;
					//viewBag.HasCookieConsent = true;
				}
				else if (consentCookie.Value == "true")
				{
					// Consent has been explicitly given
					viewBag.HasCookieConsent = true;
					_consentLevel = ConsentLevel.Consented;
				}
				else
				{
					// assume consent denied
					viewBag.HasCookieConsent = false;
					_consentLevel = ConsentLevel.Asked;
				}
			}
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!IsProcessableRequest(filterContext))
				return;

			// Should only run on a full view rendering result or HTML ContentResult
			if (!filterContext.Result.IsHtmlViewResult())
				return;

			if (_consentLevel < ConsentLevel.Consented)
			{
				_widgetProvider.Value.RegisterAction(
					new[] { "body_end_html_tag_before" },
					"CookieConsentBadge",
					"Common",
					new { area = "" });
			}
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}

	public static class CookieConsent
	{
		public const string CONSENT_COOKIE_NAME = "CookieConsent";

		public static void SetCookieConsent(HttpResponseBase response, bool consent)
		{
			var consentCookie = new HttpCookie(CookieConsent.CONSENT_COOKIE_NAME);
			consentCookie.Value = consent ? "true" : "false";
			consentCookie.Expires = DateTime.UtcNow.AddYears(1);
			response.Cookies.Set(consentCookie);
		}

		public static bool AskCookieConsent(ViewContext context)
		{
			return context.ViewBag.AskCookieConsent ?? false;
		}

		public static bool HasCookieConsent(ViewContext context)
		{
			return context.ViewBag.HasCookieConsent ?? false;
		}
	}
}