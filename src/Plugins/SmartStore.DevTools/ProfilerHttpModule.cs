using System;
using System.Web;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using StackExchange.Profiling;

namespace SmartStore.DevTools
{
	public class ProfilerHttpModule : IHttpModule
	{
		private const string MP_KEY = "sm.miniprofiler.started";
		
		public void Init(HttpApplication context)
		{
			if (DevToolsPlugin.HasPendingMigrations())
			{
				return;
			}

			context.AcquireRequestState += OnAcquireRequestState;
			context.EndRequest += OnEndRequest;
		}

		private static void OnAcquireRequestState(object sender, EventArgs e)
		{
			var app = (HttpApplication)sender;
			if (!MiniProfilerStarted(app) && ShouldProfile(app))
			{
				MiniProfiler.Start();
				if (app.Context != null && app.Context.Items != null)
				{
					app.Context.Items[MP_KEY] = true;
				}
			}
		}

		private static void OnEndRequest(object sender, EventArgs e)
		{
			var app = (HttpApplication)sender;
			if (MiniProfilerStarted(app))
			{
				MiniProfiler.Stop();
			}
		}

		private static bool MiniProfilerStarted(HttpApplication app)
		{
			return app?.Context?.Items != null && app.Context.Items.Contains(MP_KEY);
		}

		private static bool ShouldProfile(HttpApplication app)
		{
			if (app?.Context?.Request == null)
				return false;

			if (!DataSettings.DatabaseIsInstalled())
			{
				return false;
			}

			var url = app.Context.Request.AppRelativeCurrentExecutionFilePath;
			if (url.StartsWith("~/admin", StringComparison.InvariantCultureIgnoreCase) 
				|| url.StartsWith("~/mini-profiler", StringComparison.InvariantCultureIgnoreCase)
				|| url.StartsWith("~/bundles", StringComparison.InvariantCultureIgnoreCase)
				|| url.StartsWith("~/plugin/", StringComparison.InvariantCultureIgnoreCase)
				|| url.StartsWith("~/taskscheduler", StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}

			ProfilerSettings settings = null;

			if (EngineContext.Current.IsFullyInitialized)
			{
				try
				{
					settings = EngineContext.Current.Resolve<ProfilerSettings>();
				}
				catch
				{
					return true;
				}
			}

			return settings == null ? true : settings.EnableMiniProfilerInPublicStore;
		}

		public void Dispose()
		{
			// nothing to dispose
		}
	}
}