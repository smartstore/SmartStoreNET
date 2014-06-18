using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Plugins;
using StackExchange.Profiling;

namespace SmartStore.Plugin.Developer.DevTools
{

	public class ProfilerStarter : IPreApplicationStart
	{
		public void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(ProfilerHttpModule));
		}
	}

	public class ProfilerHttpModule : IHttpModule
	{
		public void Init(HttpApplication context)
		{
			context.BeginRequest += OnBeginRequest;
			context.EndRequest += OnEndRequest;
		}

		public static void OnBeginRequest(object sender, EventArgs e)
		{
			MiniProfiler.Start();
		}

		public static void OnEndRequest(object sender, EventArgs e)
		{
			MiniProfiler.Stop();
		}

		public void Dispose()
		{
			// nothing to dispose
		}
	}

}