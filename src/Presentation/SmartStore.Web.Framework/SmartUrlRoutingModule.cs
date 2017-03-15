﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Web;
using System.Web.Routing;
using System.Text.RegularExpressions;
using SmartStore.Utilities;
using SmartStore.Core;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Core.IO;
using System.IO;
using System.Web.Hosting;
using System.Reflection;

namespace SmartStore.Web.Framework
{
	public class SmartUrlRoutingModule : IHttpModule
	{
		private static readonly object _contextKey = new object();
		private static readonly ConcurrentBag<RoutablePath> _routes = new ConcurrentBag<RoutablePath>();

		public void Init(HttpApplication application)
		{
			if (application.Context.Items[_contextKey] == null)
			{
				application.Context.Items[_contextKey] = _contextKey;

				if (CommonHelper.IsDevEnvironment && HttpContext.Current.IsDebuggingEnabled)
				{
					// Handle plugin static file in DevMode
					application.PostAuthorizeRequest += (s, e) => PostAuthorizeRequest(new HttpContextWrapper(((HttpApplication)s).Context));
					application.PreSendRequestHeaders += (s, e) => PreSendRequestHeaders(new HttpContextWrapper(((HttpApplication)s).Context));
				}
				
				application.PostResolveRequestCache += (s, e) => PostResolveRequestCache(new HttpContextWrapper(((HttpApplication)s).Context));

				StopSubDirMonitoring();
			}			
		}

		private void StopSubDirMonitoring()
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

		/// <summary>
		/// Registers a path pattern which should be handled by the <see cref="UrlRoutingModule"/>
		/// </summary>
		/// <param name="path">The app relative path to handle (regular expression, e.g. <c>/sitemap.xml</c> or <c>/mini-profiler-resources/(.*)</c>)</param>
		/// <param name="verb">The http method constraint (regular expression, e.g. <c>GET|POST</c>)</param>
		/// <remarks>
		/// For performance reasons SmartStore.NET is configured to serve static files (files with extensions other than those mapped to managed handlers)
		/// through the native <c>StaticFileModule</c>. This method lets you define exceptions to this default rule: every path registered as routable gets
		/// handled by the <see cref="UrlRoutingModule"/> and therefore enables dynamic processing of (physically non-existant) static files.
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

		public virtual void PostAuthorizeRequest(HttpContextBase context)
		{
			var request = context?.Request;
			if (request == null)
				return;

			if (IsPluginPath(request) && WebHelper.IsStaticResourceRequested(request))
			{
				// We're in debug mode and in dev environment, so we can be sure that 'PluginDebugViewVirtualPathProvider' is running
				var file = HostingEnvironment.VirtualPathProvider.GetFile(request.AppRelativeCurrentExecutionFilePath) as DebugPluginVirtualFile;
				if (file != null)
				{
					context.Items["DebugFile"] = file;
					context.Response.WriteFile(file.PhysicalPath);
					context.Response.End();
				}
			}
		}

		public virtual void PreSendRequestHeaders(HttpContextBase context)
		{
			if (context?.Response == null)
				return;

			var file = context.Items?["DebugFile"] as DebugPluginVirtualFile;
			if (file != null)
			{
				context.Response.AddFileDependency(file.PhysicalPath);
				context.Response.ContentType = MimeTypes.MapNameToMimeType(Path.GetFileName(file.PhysicalPath));
				context.Response.Cache.SetNoStore();
				context.Response.Cache.SetLastModifiedFromFileDependencies();
			}
		}

		public virtual void PostResolveRequestCache(HttpContextBase context)
		{
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
						module.PostResolveRequestCache(context);
						return;
					}
				}
			}	
		}

		private bool IsPluginPath(HttpRequestBase request)
		{
			var result = request.AppRelativeCurrentExecutionFilePath.StartsWith("~/Plugins/", StringComparison.InvariantCultureIgnoreCase);
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
