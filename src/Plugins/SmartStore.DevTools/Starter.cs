using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStore.DevTools
{
	public class ProfilerPreApplicationStart : IPreApplicationStart
	{
		public void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(ProfilerHttpModule));
			SmartUrlRoutingModule.RegisterRoutablePath("/mini-profiler-resources/(.*)");
		}
	}

	public class ProfilerStartupTask : IApplicationStart
	{
		public void Start()
		{
			StackExchange.Profiling.MiniProfiler.Configure(new MiniProfilerOptions 
			{
				MaxUnviewedProfiles = 5,
				Storage = new MemoryCacheStorage(TimeSpan.FromMinutes(1)),
				//ResultsAuthorize = req => true,
			});

            StackExchange.Profiling.EntityFramework6.MiniProfilerEF6.Initialize();
			
			// output cache invidation example 
			//OutputCacheInvalidationObserver.Execute();
		}
		
		public int Order
		{
			get { return int.MinValue; }
		}
	}
}