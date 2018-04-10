using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System;
using System.Collections.Generic;

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

	public class ProfilerStartupTask : IStartupTask
	{
		public void Execute()
		{
			StackExchange.Profiling.MiniProfiler.Settings.MaxUnviewedProfiles = 5;
			//StackExchange.Profiling.MiniProfiler.Settings.Results_List_Authorize = (req) => true;
			//StackExchange.Profiling.MiniProfiler.Settings.Storage = new NullProfilerStorage();

			StackExchange.Profiling.EntityFramework6.MiniProfilerEF6.Initialize();
			
			// output cache invidation example 
			//OutputCacheInvalidationObserver.Execute();
		}
		
		public int Order
		{
			get { return int.MinValue; }
		}
	}

	internal class NullProfilerStorage : IStorage
	{
		public List<Guid> GetUnviewedIds(string user)
		{
			return new List<Guid>();
		}

		public IEnumerable<Guid> List(int maxResults, DateTime? start = default(DateTime?), DateTime? finish = default(DateTime?), ListResultsOrder orderBy = ListResultsOrder.Descending)
		{
			return new List<Guid>();
		}

		public MiniProfiler Load(Guid id)
		{
			return null;
		}

		public void Save(MiniProfiler profiler)
		{
		}

		public void SetUnviewed(string user, Guid id)
		{
		}

		public void SetViewed(string user, Guid id)
		{
		}
	}
}