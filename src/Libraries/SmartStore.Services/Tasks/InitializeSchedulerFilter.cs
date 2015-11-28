using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.Tasks
{
    public class InitializeSchedulerFilter : IAuthorizationFilter
    {
        private readonly static object s_lock = new object();
        private static bool s_initializing = false;
        
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            lock (s_lock)
            {
                if (!s_initializing)
                {
                    s_initializing = true;

					var logger = EngineContext.Current.Resolve<ILogger>();

					try
					{
						var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();
						var storeService = EngineContext.Current.Resolve<IStoreService>();
						var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
						var taskScheduler = EngineContext.Current.Resolve<ITaskScheduler>();

						var tasks = taskService.GetAllTasks(true);
						taskService.CalculateFutureSchedules(tasks, true /* isAppStart */);

						var baseUrl = CommonHelper.GetAppSetting<string>("sm:TaskSchedulerBaseUrl");
						if (baseUrl.IsWebUrl())
						{
							taskScheduler.BaseUrl = baseUrl;
						}
						else
						{
							// autoresolve base url
							taskScheduler.SetBaseUrl(storeService, filterContext.HttpContext);
						}

						taskScheduler.SweepIntervalMinutes = CommonHelper.GetAppSetting<int>("sm:TaskSchedulerSweepInterval", 1);
						taskScheduler.Start();

						logger.Information("Initialized TaskScheduler with base url '{0}'".FormatInvariant(taskScheduler.BaseUrl));

						eventPublisher.Publish(new AppInitScheduledTasksEvent { ScheduledTasks = tasks });
					}
					catch (Exception ex)
					{
						logger.Error("Error while initializing Task Scheduler", ex);
					}
					finally
					{
						GlobalFilters.Filters.Remove(this);
					}
                }
            }
        }
    }
}
