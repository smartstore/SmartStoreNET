using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.UI;
using System;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Filters
{
	public class CookieConsentFilter : IActionFilter, IResultFilter
	{
		private readonly IUserAgent _userAgent;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly PrivacySettings _privacySettings;

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
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!_privacySettings.EnableCookieConsent)
				return;

			if (filterContext?.ActionDescriptor == null || filterContext?.HttpContext?.Request == null)
				return;

			string actionName = filterContext.ActionDescriptor.ActionName;
			string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

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
					}
					else
					{
						// first request on the site and no DNT header. 
						consentCookie = new HttpCookie(CookieConsent.CONSENT_COOKIE_NAME);
						consentCookie.Value = "asked";
						filterContext.HttpContext.Response.Cookies.Add(consentCookie);
					}
				}
			}
			else
			{
				// we received a consent cookie
				viewBag.AskCookieConsent = false;
				if (consentCookie.Value == "asked")
				{
					// consent is implicitly given
					consentCookie.Expires = DateTime.UtcNow.AddYears(1);
					filterContext.HttpContext.Response.Cookies.Set(consentCookie);
					viewBag.HasCookieConsent = true;
				}
				else if (consentCookie.Value == "true")
				{
					viewBag.HasCookieConsent = true;
				}
				else
				{
					// assume consent denied
					viewBag.HasCookieConsent = false;
				}
			}

			// supress action of widgets which are setting cookies
			// TODO: What to do if cookies are set from scripts within topics???
			if (consentCookie != null && consentCookie.Value != "true")
			{
				switch (controllerName)
				{
					case "AmazonPay":
					case "AmazonPayCheckout":
					case "AmazonPayShoppingCart":
					case "WidgetsGoogleAnalytics":
					case "ExternalAuthFacebook":
					case "WidgetsETracker":
						filterContext.Result = new EmptyResult();
						break;
					case "Product":
						if (actionName == "ShareButton")
						{
							filterContext.Result = new EmptyResult();
						}
						break;

					default:
						//nothing to do here
						break;
				}
			}
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{

		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_privacySettings.EnableCookieConsent)
				return;

			if (filterContext.IsChildAction)
				return;

			var result = filterContext.Result;

			// should only run on a full view rendering result or HTML ContentResult
			if (!result.IsHtmlViewResult())
				return;
			
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