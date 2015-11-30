using System;
using System.Web;
using SmartStore.Core.Infrastructure;
using StackExchange.Profiling;

namespace SmartStore.DevTools
{

	public class ProfilerHttpModule : IHttpModule
	{
		private const string MP_KEY = "sm.miniprofiler.started";
		
		public void Init(HttpApplication context)
		{
			context.BeginRequest += OnBeginRequest;
			context.EndRequest += OnEndRequest;
		}

		public static void OnBeginRequest(object sender, EventArgs e)
		{
			var app = (HttpApplication)sender;
			if (ShouldProfile(app))
			{
				MiniProfiler.Start();
				if (app.Context != null && app.Context.Items != null)
				{
					app.Context.Items[MP_KEY] = true;
				}
			}
		}

		public static void OnEndRequest(object sender, EventArgs e)
		{
			var app = (HttpApplication)sender;
			if (app.Context != null && app.Context.Items != null && app.Context.Items.Contains(MP_KEY))
			{
				MiniProfiler.Stop();
			}
		}

		private static bool ShouldProfile(HttpApplication app)
		{
			if (app.Context == null || app.Context.Request == null)
				return false;

			var url = app.Context.Request.AppRelativeCurrentExecutionFilePath;
			if (url.StartsWith("~/admin", StringComparison.InvariantCultureIgnoreCase) || url.StartsWith("~/mini-profiler", StringComparison.InvariantCultureIgnoreCase) || url.StartsWith("~/bundles", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			ProfilerSettings settings;
			if (!EngineContext.Current.ContainerManager.TryResolve<ProfilerSettings>(null, out settings))
			{
				return false;
			}

			if (!settings.EnableMiniProfilerInPublicStore)
			{
				return false;
			}

			return true;
		}

		public void Dispose()
		{
			// nothing to dispose
		}
	}

}