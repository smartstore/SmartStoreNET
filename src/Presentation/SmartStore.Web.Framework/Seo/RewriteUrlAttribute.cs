using System;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Web.Framework.Seo
{
    public class RewriteUrlAttribute : FilterAttribute, IAuthorizationFilter
    {
        public RewriteUrlAttribute(SslRequirement sslRequirement)
        {
            SslRequirement = sslRequirement;
            AppendTrailingSlash = RouteTable.Routes.AppendTrailingSlash;
            LowercaseUrls = RouteTable.Routes.LowercaseUrls;
            Order = 100;
        }

        public Lazy<SeoSettings> SeoSettings { get; set; }
        public Lazy<SecuritySettings> SecuritySettings { get; set; }
        public Lazy<IStoreContext> StoreContext { get; set; }
        public Lazy<IWebHelper> WebHelper { get; set; }
        public Lazy<IWorkContext> WorkContext { get; set; }

        public SslRequirement SslRequirement { get; set; }
        public bool AppendTrailingSlash { get; set; }
        public bool LowercaseUrls { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            // don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

            // only redirect for GET requests, 
            // otherwise the browser might not propagate the verb and request body correctly.
            if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            if (HostingEnvironment.IsHosted && !DataSettings.DatabaseIsInstalled())
                return;

            var currentStore = StoreContext.Value.CurrentStore;
            var uri = filterContext.HttpContext.Request.Url;
            var originalUrl = new Uri(uri.GetLeftPart(UriPartial.Authority) + filterContext.HttpContext.Request.RawUrl);

            var rewriteContext = new RewriteContext
            {
                ControllerContext = filterContext,
                OriginalUrl = originalUrl,
                IsLoopback = uri.IsLoopback || filterContext.HttpContext.Request.IsLocal,
                CurrentStore = currentStore,
                SecurityMode = currentStore.GetSecurityMode(),
                IsAdmin = WorkContext.Value.IsAdmin
            };

            // Applies HTTP protocol rule
            TryRewriteScheme(rewriteContext);

            // Applies canonical host name rule
            TryRewriteHostName(rewriteContext);

            // Applies trailing slash and lowercase rules
            TryRewritePath(rewriteContext);

            if (rewriteContext.Url.HasValue())
            {
                filterContext.Result = new RedirectResult(rewriteContext.Url, rewriteContext.Permanent ?? !rewriteContext.IsLoopback);
            }
        }

        /// <summary>
        /// Applies HTTP protocol rule
        /// </summary>
        private bool TryRewriteScheme(RewriteContext context)
        {
            if (context.IsLoopback && !SecuritySettings.Value.UseSslOnLocalhost)
                return false;

            var webHelper = WebHelper.Value;

            var currentConnectionSecured = webHelper.IsCurrentConnectionSecured();
            var currentStore = context.CurrentStore;

            if (currentStore.ForceSslForAllPages)
            {
                // all pages are forced to be SSL no matter of the specified value
                this.SslRequirement = SslRequirement.Yes;
            }

            switch (this.SslRequirement)
            {
                case SslRequirement.Yes:
                    {
                        if (!currentConnectionSecured && context.SecurityMode > HttpSecurityMode.Unsecured)
                        {
                            // Redirect to HTTPS version of page
                            context.Url = webHelper.GetThisPageUrl(true, true);
                            return true;
                        }
                    }
                    break;
                case SslRequirement.No:
                    {
                        if (currentConnectionSecured)
                        {
                            // Redirect to HTTP version of page
                            context.Url = webHelper.GetThisPageUrl(true, false);
                            return true;
                        }
                    }
                    break;
                default:
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Applies canonical host name rule
        /// </summary>
        private bool TryRewriteHostName(RewriteContext context)
        {
            var rule = SeoSettings.Value.CanonicalHostNameRule;
            if (rule == CanonicalHostNameRule.NoRule)
                return false;

            if (context.IsLoopback)
            {
                // Allows testing of "localtest.me"
                return false;
            }

            var uri = context.Url == null ? context.OriginalUrl : new Uri(context.Url);
            var url = context.Url ?? uri.ToString();
            var protocol = uri.Scheme.ToLower();
            var isHttps = protocol.IsCaseInsensitiveEqual("https");
            var startsWith = "{0}://www.".FormatInvariant(protocol);
            var hasPrefix = url.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);

            if (isHttps)
            {
                if (context.SecurityMode == HttpSecurityMode.SharedSsl)
                {
                    // Don't attempt to redirect when shared SSL is being used and current request is secured.
                    // Redirecting from http://ssl.bla.com to https://www.ssl.bla.com will most probably fail.
                    return false;
                }
            }

            if (hasPrefix && rule == CanonicalHostNameRule.OmitWww)
            {
                context.Url = url.Replace("://www.", "://");
                return true;
            }

            if (!hasPrefix && rule == CanonicalHostNameRule.RequireWww)
            {
                context.Url = url.Replace("{0}://".FormatInvariant(protocol), startsWith);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies trailing slash and lowercase rules
        /// </summary>
        private bool TryRewritePath(RewriteContext context)
        {
            // Be tolerant in backend
            if (context.IsAdmin)
                return false;

            // Don't try to fix root path
            if (context.ControllerContext.HttpContext.Request.Path == "/")
                return false;

            bool rewritten = false;

            var url = context.Url ?? context.OriginalUrl.ToString();
            var queryIndex = url.IndexOf('?');

            if (queryIndex == -1)
            {
                bool hasTrailingSlash = url[url.Length - 1] == '/';

                if (this.AppendTrailingSlash)
                {
                    // Append a trailing slash to the end of the URL.
                    if (!hasTrailingSlash)
                    {
                        url += "/";
                        rewritten = true;
                    }
                }
                else
                {
                    // Trim a trailing slash from the end of the URL.
                    if (hasTrailingSlash)
                    {
                        url = url.TrimEnd('/');
                        rewritten = true;
                    }
                }
            }
            else
            {
                bool hasTrailingSlash = url[queryIndex - 1] == '/';

                if (this.AppendTrailingSlash)
                {
                    // Append a trailing slash to the end of the URL but before the query string.
                    if (!hasTrailingSlash)
                    {
                        url = url.Insert(queryIndex, "/");
                        rewritten = true;
                    }
                }
                else
                {
                    // Trim a trailing slash to the end of the URL but before the query string.
                    if (hasTrailingSlash)
                    {
                        url = url.Remove(queryIndex - 1, 1);
                        rewritten = true;
                    }
                }
            }

            if (this.LowercaseUrls)
            {
                foreach (char c in url)
                {
                    if (char.IsUpper(c))
                    {
                        var qIndex = url.IndexOf('?');
                        url = qIndex == -1
                            ? url.ToLower()
                            : url.Substring(0, qIndex).ToLower() + url.Substring(qIndex);

                        rewritten = true;
                        break;
                    }
                    if (c == '?')
                    {
                        break;
                    }
                }
            }

            if (rewritten)
            {
                context.Url = url;
            }

            return rewritten;
        }

        class RewriteContext
        {
            public Uri OriginalUrl { get; set; }
            public bool IsLoopback { get; set; }
            public HttpSecurityMode SecurityMode { get; set; }
            public ControllerContext ControllerContext { get; set; }
            public Store CurrentStore { get; set; }
            public bool IsAdmin { get; set; }

            public string Url { get; set; }
            public bool? Permanent { get; set; }
        }
    }
}
