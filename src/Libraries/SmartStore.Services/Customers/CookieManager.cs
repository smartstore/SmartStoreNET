using Newtonsoft.Json;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Services.Customers
{

    /// <summary>
    /// Class that will be serialized and stored as string in a cookie.
    /// </summary>
    public class ConsentCookie
    {
        public bool AllowAnalytics { get; set; }
        public bool AllowThirdParty { get; set; }
    }

    public partial class CookieManager : ICookieManager
    {
        private readonly ICommonServices _services;
        private readonly ITypeFinder _typeFinder;
        private readonly HttpContextBase _httpContext;
        private readonly PrivacySettings _privacySettings;

        public const string ConsentCookieName = "CookieConsent";

        private readonly static object _lock = new object();
        private static IList<Type> _cookiePublisherTypes = null;

        public CookieManager(
            ICommonServices services,
            ITypeFinder typeFinder,
            HttpContextBase httpContext,
            PrivacySettings privacySettings)
        {
            _services = services;
            _typeFinder = typeFinder;
            _httpContext = httpContext;
            _privacySettings = privacySettings;
        }

        public LocalizerEx T { get; set; } = NullLocalizer.InstanceEx;

        public ILogger Logger { get; set; } = NullLogger.Instance;

        protected virtual IEnumerable<ICookiePublisher> GetAllCookiePublishers()
        {
            if (_cookiePublisherTypes == null)
            {
                lock (_lock)
                {
                    if (_cookiePublisherTypes == null)
                    {
                        _cookiePublisherTypes = _typeFinder.FindClassesOfType<ICookiePublisher>(ignoreInactivePlugins: true).ToList();
                    }
                }
            }

            var cookiePublishers = _cookiePublisherTypes
                .Select(x => EngineContext.Current.ContainerManager.ResolveUnregistered(x) as ICookiePublisher)
                .ToArray();

            return cookiePublishers;
        }

        public virtual List<CookieInfo> GetAllCookieInfos(bool addSettingCookies = false)
        {
            var cookieInfos = new List<CookieInfo>();
            var plugins = GetAllCookiePublishers();

            foreach (var plugin in plugins)
            {
                var typedInstance = plugin as ICookiePublisher;
                var cookieInfo = typedInstance.GetCookieInfo();
                if (cookieInfo != null)
                    cookieInfos.AddRange(cookieInfo);
            }

            // Add user defined cookies from privacy settings.
            if (addSettingCookies && _privacySettings.CookieInfos.HasValue())
            {
                var ciList = JsonConvert.DeserializeObject<List<CookieInfo>>(_privacySettings.CookieInfos);

                if (ciList != null && ciList.Count > 0)
                {
                    foreach (var ci in ciList)
                    {
                        ci.Name = ci.GetLocalized(x => x.Name);
                        ci.Description = ci.GetLocalized(x => x.Description);
                        cookieInfos.Add(ci);
                    }
                }
            }

            return cookieInfos;
        }

        public virtual bool IsCookieAllowed(ControllerContext context, CookieType cookieType)
        {
            // Ask whether cookie type is allowed.
            var cookie = context.HttpContext.Request.Cookies[ConsentCookieName];
            if (cookie != null)
            {
                try
                {
                    var cookieData = JsonConvert.DeserializeObject<ConsentCookie>(cookie.Value);
                    if ((cookieData.AllowAnalytics && cookieType == CookieType.Analytics) || (cookieData.AllowThirdParty && cookieType == CookieType.ThirdParty) || cookieType == CookieType.Required)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Lets be tolerant in case of error.
                    return true;
                }
            }

            // If no cookie was set return false
            return false;
        }

        public virtual void SetConsentCookie(HttpResponseBase response, bool allowAnalytics = false, bool allowThirdParty = false)
        {
            var expiry = TimeSpan.FromDays(365);

            var cookieData = new ConsentCookie
            {
                AllowAnalytics = allowAnalytics,
                AllowThirdParty = allowThirdParty
            };

            var secure = _services.WebHelper.IsCurrentConnectionSecured();
            var consentCookie = new HttpCookie(ConsentCookieName)
            {
                // Store JSON serialized object which contains current allowed types (Analytics & ThirdParty) in cookie.
                Value = JsonConvert.SerializeObject(cookieData),
                Expires = DateTime.UtcNow + expiry,
                Secure = secure,
                SameSite = secure ? (SameSiteMode)_privacySettings.SameSiteMode : SameSiteMode.Lax
            };

            response.Cookies.Set(consentCookie);
        }

        public virtual ConsentCookie GetCookieData(ControllerContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.Items.Contains(ConsentCookieName))
            {
                return httpContext.Items[ConsentCookieName] as ConsentCookie;
            }

            var cookie = httpContext.Request.Cookies[ConsentCookieName];

            if (cookie != null)
            {
                try
                {
                    var cookieData = JsonConvert.DeserializeObject<ConsentCookie>(cookie.Value);
                    httpContext.Items[ConsentCookieName] = cookieData;
                    return cookieData;
                }
                catch { }
            }

            return null;
        }
    }
}
