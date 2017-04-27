using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework;

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
			StackExchange.Profiling.EntityFramework6.MiniProfilerEF6.Initialize();
			//StackExchange.Profiling.MiniProfiler.Settings.MaxUnviewedProfiles = 2;
		}

		public int Order
		{
			get { return int.MinValue; }
		}
	}
}