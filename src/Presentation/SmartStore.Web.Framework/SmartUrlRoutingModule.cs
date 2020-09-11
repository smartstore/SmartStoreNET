using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.Web.Framework
{
    /// <remarks>
    /// Request event sequence:
    /// - BeginRequest
    /// - AuthenticateRequest 
    /// - PostAuthenticateRequest 
    /// - AuthorizeRequest 
    /// - PostAuthorizeRequest 
    /// - ResolveRequestCache 
    /// - PostResolveRequestCache 
    /// - MapRequestHandler 
    /// - PostMapRequestHandler 
    /// - AcquireRequestState 
    /// - PostAcquireRequestState
    /// - PreRequestHandlerExecute 
    /// - PostRequestHandlerExecute 
    /// - ReleaseRequestState 
    /// - PostReleaseRequestState 
    /// - UpdateRequestCache 
    /// - PostUpdateRequestCache  
    /// - LogRequest 
    /// - PostLogRequest  
    /// - EndRequest  
    /// - PreSendRequestHeaders  
    /// - PreSendRequestContent
    /// </remarks>
    public class SmartUrlRoutingModule : IHttpModule
    {
        private static readonly object _contextKey = new object();

        private static readonly ICollection<Action<HttpApplication>> _actions =
            new SyncedCollection<Action<HttpApplication>>(new List<Action<HttpApplication>>()) { ReadLockFree = true };

        private static readonly ICollection<RoutablePath> _routes =
            new SyncedCollection<RoutablePath>(new List<RoutablePath>()) { ReadLockFree = true };

        static SmartUrlRoutingModule()
        {
            StopSubDirMonitoring();
        }

        private static void StopSubDirMonitoring()
        {
            try
            {
                // http://stackoverflow.com/questions/2248825/asp-net-restarts-when-a-folder-is-created-renamed-or-deleted
                var prop = typeof(HttpRuntime).GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                var o = prop.GetValue(null, null);

                var fi = o.GetType().GetField("_dirMonSubdirs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                var monitor = fi.GetValue(o);
                var mi = monitor.GetType().GetMethod("StopMonitoring", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(monitor, new object[] { });
            }
            catch { }
        }

        public void Init(HttpApplication application)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            if (application.Context.Items[_contextKey] == null)
            {
                application.Context.Items[_contextKey] = _contextKey;

                application.BeginRequest += (s, e) => FilterSameSiteNoneForIncompatibleUserAgents(application);

                if (CommonHelper.IsDevEnvironment && HttpContext.Current.IsDebuggingEnabled)
                {
                    // Handle plugin static file in DevMode
                    application.PostAuthorizeRequest += (s, e) => RewritePluginStaticFilePaths((HttpApplication)s);
                    application.PreSendRequestHeaders += (s, e) => HandlePluginStaticFileCaching((HttpApplication)s);
                }

                application.PreSendRequestHeaders += (s, e) => HandleSameSiteForAntiForgeryCookie((HttpApplication)s);
                application.PostResolveRequestCache += (s, e) => TryResolveRoutablePath((HttpApplication)s);

                // Publish event to give plugins the chance to register custom event handlers for the request lifecycle.
                foreach (var action in _actions)
                {
                    action(application);
                }

                // Set app to fully initialized state on very first request
                EngineContext.Current.IsFullyInitialized = true;
            }
        }

        #region Handlers

        private static void RewritePluginStaticFilePaths(HttpApplication app)
        {
            var context = app.Context;
            var request = context?.Request;
            if (request == null)
                return;

            if (IsExtensionPath(request) && context.IsStaticResourceRequested())
            {
                // We're in debug mode and in dev environment
                var file = HostingEnvironment.VirtualPathProvider.GetFile(request.AppRelativeCurrentExecutionFilePath) as DebugVirtualFile;
                if (file != null)
                {
                    context.Items["DebugFile"] = file;
                    context.Response.TransmitFile(file.PhysicalPath);
                    context.Response.Flush(); // TBD: optional?
                    context.Response.SuppressContent = true; // TBD: optional?
                    context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        private static void HandlePluginStaticFileCaching(HttpApplication app)
        {
            var context = app.Context;
            if (context?.Response == null)
                return;

            var file = context.Items?["DebugFile"] as DebugVirtualFile;
            if (file != null)
            {
                context.Response.AddFileDependency(file.PhysicalPath);
                context.Response.ContentType = MimeTypes.MapNameToMimeType(Path.GetFileName(file.PhysicalPath));
                context.Response.Cache.SetNoStore();
                context.Response.Cache.SetLastModifiedFromFileDependencies();
            }
        }

        private static void HandleSameSiteForAntiForgeryCookie(HttpApplication app)
        {
            var context = app.Context;
            if (context?.Request == null || context?.Response == null)
                return;

            if (context.IsStaticResourceRequested())
                return;

            if (SameSiteBrowserDetector.AllowsSameSiteNone(context.Request.UserAgent))
            {
                // Set SameSite attribute for antiforgery token.
                var privacySettings = EngineContext.Current.Resolve<PrivacySettings>();
                var antiForgeryCookie = context.Request.Cookies["__requestverificationtoken"];

                if (antiForgeryCookie != null)
                {
                    antiForgeryCookie.HttpOnly = true;
                    antiForgeryCookie.Secure = context.Request.IsHttps();
                    antiForgeryCookie.SameSite = antiForgeryCookie.Secure ? (SameSiteMode)privacySettings.SameSiteMode : SameSiteMode.Lax;

                    context.Response.Cookies.Set(antiForgeryCookie);
                }
            }
        }

        private static void TryResolveRoutablePath(HttpApplication app)
        {
            var context = app.Context;
            var request = context?.Request;
            if (request == null)
                return;

            if (_routes.Count > 0)
            {
                var path = request.AppRelativeCurrentExecutionFilePath.TrimStart('~').TrimEnd('/');
                var method = request.HttpMethod.EmptyNull();

                foreach (var route in _routes)
                {
                    if (route.PathPattern.IsMatch(path) && route.HttpMethodPattern.IsMatch(method))
                    {
                        var module = new UrlRoutingModule();
                        module.PostResolveRequestCache(new HttpContextWrapper(context));
                        return;
                    }
                }
            }
        }

        private static void FilterSameSiteNoneForIncompatibleUserAgents(HttpApplication app)
        {
            var context = app?.Context;
            if (context == null || context.IsStaticResourceRequested() || SameSiteBrowserDetector.AllowsSameSiteNone(context.Request?.UserAgent))
                return;

            app.Response.AddOnSendingHeaders(ctx =>
            {
                var cookies = ctx.Response.Cookies;
                for (var i = 0; i < cookies.Count; i++)
                {
                    var cookie = cookies[i];
                    if (cookie.SameSite == SameSiteMode.None)
                    {
                        cookie.SameSite = (SameSiteMode)(-1); // Unspecified
                    }
                }
            });
        }

        #endregion

        /// <summary>
        ///  Registers an action that is called on application init. Call this to register HTTP request lifecycle callbacks / event handlers.
        /// </summary>
        /// <param name="action">Action</param>
        public static void RegisterAction(Action<HttpApplication> action)
        {
            Guard.NotNull(action, nameof(action));

            _actions.Add(action);
        }

        /// <summary>
        /// Registers a path pattern which should be handled by the <see cref="UrlRoutingModule"/>
        /// </summary>
        /// <param name="path">The app relative path to handle (regular expression, e.g. <c>/sitemap.xml</c> or <c>/mini-profiler-resources/(.*)</c>)</param>
        /// <param name="verb">The http method constraint (regular expression, e.g. <c>GET|POST</c>)</param>
        /// <remarks>
        /// For performance reasons Smartstore is configured to serve static files (files with extensions other than those mapped to managed handlers)
        /// through the native <c>StaticFileModule</c>. This method lets you define exceptions to this default rule: every path registered as routable gets
        /// handled by the <see cref="UrlRoutingModule"/> and therefore enables dynamic processing of (physically non-existent) static files.
        /// </remarks>
        public static void RegisterRoutablePath(string path, string verb = ".*")
        {
            Guard.NotEmpty(path, nameof(path));

            if (path.IsWebUrl())
            {
                throw new ArgumentException("Only relative paths are allowed.", "path");
            }

            var rgPath = new Regex(path, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var rgVerb = new Regex(verb, RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            _routes.Add(new RoutablePath { PathPattern = rgPath, HttpMethodPattern = rgVerb });
        }

        public static bool HasMatchingPathHandler(string path, string method = "GET")
        {
            Guard.NotEmpty(path, nameof(path));
            Guard.NotEmpty(method, nameof(method));

            if (_routes.Count > 0)
            {
                foreach (var route in _routes)
                {
                    if (route.PathPattern.IsMatch(path) && route.HttpMethodPattern.IsMatch(method))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsExtensionPath(HttpRequest request)
        {
            var path = request.AppRelativeCurrentExecutionFilePath.ToLower();
            var result = path.StartsWith("~/plugins/") || path.StartsWith("~/themes/");
            return result;
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        private class RoutablePath
        {
            public Regex PathPattern { get; set; }
            public Regex HttpMethodPattern { get; set; }
        }
    }
}
