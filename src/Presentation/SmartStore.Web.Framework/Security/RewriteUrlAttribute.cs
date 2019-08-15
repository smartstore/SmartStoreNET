using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Web.Framework.Security
{
    public class RewriteUrlAttribute : FilterAttribute, IAuthorizationFilter
    {
		public RewriteUrlAttribute(SslRequirement sslRequirement)
        {
            this.SslRequirement = sslRequirement;
			this.Order = 100;
        }

		public Lazy<SeoSettings> SeoSettings { get; set; }
		public Lazy<SecuritySettings> SecuritySettings { get; set; }
		public Lazy<IStoreContext> StoreContext { get; set; }
		public Lazy<IWebHelper> WebHelper { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

			// only redirect for GET requests, 
			// otherwise the browser might not propagate the verb and request body correctly.
			if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

			var securitySettings = SecuritySettings.Value;
			var isLocalRequest = filterContext.HttpContext.Request.IsLocal;

			if (!securitySettings.UseSslOnLocalhost && isLocalRequest)
				return;

			var webHelper = WebHelper.Value;

			var currentConnectionSecured = webHelper.IsCurrentConnectionSecured();
			var currentStore = StoreContext.Value.CurrentStore;

			if (currentStore.ForceSslForAllPages)
            {
                // all pages are forced to be SSL no matter of the specified value
                this.SslRequirement = SslRequirement.Yes;
            }

			var securityMode = currentStore.GetSecurityMode();

			switch (this.SslRequirement)
            {
                case SslRequirement.Yes:
                    {
                        if (!currentConnectionSecured && securityMode > HttpSecurityMode.Unsecured)
                        {
							// Redirect to HTTPS version of page
							string url = webHelper.GetThisPageUrl(true, true);
							filterContext.Result = new RedirectResult(url, !isLocalRequest);
                        }
                    }
                    break;
                case SslRequirement.No:
                    {
                        if (currentConnectionSecured)
                        {
							// Redirect to HTTP version of page
							string url = webHelper.GetThisPageUrl(true, false);
							filterContext.Result = new RedirectResult(url, !isLocalRequest);
						}
                    }
                    break;
                case SslRequirement.Retain:
                    {
                        // Do nothing
                    }
                    break;
                default:
					throw new SmartException("Unsupported SslRequirement parameter");
            }

			ApplyCanonicalHostNameRule(filterContext, isLocalRequest, securityMode);
        }

		private void ApplyCanonicalHostNameRule(AuthorizationContext filterContext, bool isLocalRequest, HttpSecurityMode securityMode)
		{
			var rule = SeoSettings.Value.CanonicalHostNameRule;
			if (rule == CanonicalHostNameRule.NoRule)
				return;

			var uri = filterContext.Result != null
				? new Uri(((RedirectResult)filterContext.Result).Url)
				: filterContext.HttpContext.Request.Url;

			if (uri.IsLoopback)
			{
				// allows testing of "localtest.me"
				return;
			}

			var url = uri.ToString();
			var protocol = uri.Scheme.ToLower();
			var isHttps = protocol.IsCaseInsensitiveEqual("https");
			var startsWith = "{0}://www.".FormatInvariant(protocol);
			var hasPrefix = url.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);

			if (isHttps)
			{
				if (securityMode == HttpSecurityMode.SharedSsl)
				{
					// Don't attempt to redirect when shared SSL is being used and current request is secured.
					// Redirecting from http://ssl.bla.com to https://www.ssl.bla.com will most probably fail.
					return;
				}
			}

			if (hasPrefix && rule == CanonicalHostNameRule.OmitWww)
			{
				url = url.Replace("://www.", "://");
				filterContext.Result = new RedirectResult(url, !isLocalRequest);
			}

			if (!hasPrefix && rule == CanonicalHostNameRule.RequireWww)
			{
				url = url.Replace("{0}://".FormatInvariant(protocol), startsWith);
				filterContext.Result = new RedirectResult(url, !isLocalRequest);
			}
		}

		public SslRequirement SslRequirement { get; set; }
    }
}
