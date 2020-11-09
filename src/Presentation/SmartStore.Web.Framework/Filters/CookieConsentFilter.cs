using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Filters
{
    public class CookieConsentFilter : IActionFilter, IResultFilter
    {
        private readonly IUserAgent _userAgent;
        private readonly ICommonServices _services;
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private readonly PrivacySettings _privacySettings;
        private readonly ICookieManager _cookieManager;

        private bool _isProcessableRequest;

        public CookieConsentFilter(
            IUserAgent userAgent,
            ICommonServices services,
            Lazy<IWidgetProvider> widgetProvider,
            PrivacySettings privacySettings,
            ICookieManager cookieManager)
        {
            _userAgent = userAgent;
            _services = services;
            _widgetProvider = widgetProvider;
            _privacySettings = privacySettings;
            _cookieManager = cookieManager;
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

            var isLegacy = false;
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;
            ConsentCookie cookieData = null;

            // Check if the user has a consent cookie.
            var consentCookie = request.Cookies["CookieConsent"];
            if (consentCookie == null)
            {
                // No consent cookie. We first check the Do Not Track header value, this can have the value "0" or "1"
                var doNotTrack = request.Headers.Get("DNT");

                // If we receive a DNT header, we accept its value (0 = give consent, 1 = deny) and do not ask the user anymore.
                if (doNotTrack.HasValue())
                {
                    if (doNotTrack.Equals("0"))
                    {
                        // Tracking consented.
                        _cookieManager.SetConsentCookie(response, true, true);
                    }
                    else
                    {
                        // Tracking denied.
                        _cookieManager.SetConsentCookie(response, false, false);
                    }
                }
                else
                {
                    if (_userAgent.IsBot)
                    {
                        // Don't ask consent from search engines, also don't set cookies.
                        _cookieManager.SetConsentCookie(response, true, true);
                    }
                    else
                    {
                        // First request on the site and no DNT header (we could use session cookie, which is allowed by EU cookie law).
                        // Don't set cookie!
                    }
                }
            }
            else
            {
                // We received a consent cookie
                try
                {
                    cookieData = JsonConvert.DeserializeObject<ConsentCookie>(consentCookie.Value);
                }
                catch { }

                if (cookieData == null)
                {
                    // Cookie was found but could not be converted thus it's a legacy cookie.
                    isLegacy = true;
                    var str = consentCookie.Value;

                    // 'asked' means customer has not consented.
                    // '2' was the Value of legacy enum CookieConsentStatus.Denied
                    if (str.Equals("asked") || str.Equals("2"))
                    {
                        // Remove legacy Cookie & thus show CookieManager.
                        request.Cookies.Remove("CookieConsent");
                    }
                    // 'true' means consented to all cookies.
                    // '1' was the Value of legacy enum CookieConsentStatus.Consented
                    else if (str.Equals("true") || str.Equals("1"))
                    {
                        // Set Cookie with all types allowed.
                        _cookieManager.SetConsentCookie(response, true, true);
                    }
                }
            }

            if (!isLegacy)
            {
                filterContext.HttpContext.Items["CookieConsent"] = cookieData;
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!_isProcessableRequest)
                return;

            // Should only run on a full view rendering result or HTML ContentResult.
            if (!filterContext.Result.IsHtmlViewResult())
                return;

            // Always render the child action because of output caching.
            _widgetProvider.Value.RegisterAction(
                new[] { "body_end_html_tag_before" },
                "CookieManager",
                "Common",
                new { area = "" });
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}