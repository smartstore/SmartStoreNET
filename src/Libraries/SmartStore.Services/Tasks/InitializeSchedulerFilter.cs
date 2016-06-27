﻿using SmartStore.Core;
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
		private static int s_errCount;
        private static bool s_initializing = false;
        
        public void OnAuthorization(AuthorizationContext filterContext)
        {
			if (filterContext == null || filterContext.HttpContext == null)
				return;

			var request = filterContext.HttpContext.Request;
			if (request == null)
				return;

			if (filterContext.IsChildAction)
				return;

			lock (s_lock)
            {
                if (!s_initializing)
                {
                    s_initializing = true;

					ILogger logger = EngineContext.Current.Resolve<ILogger>();
					ITaskScheduler taskScheduler = EngineContext.Current.Resolve<ITaskScheduler>();

					try
					{
						var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();
						var storeService = EngineContext.Current.Resolve<IStoreService>();
						var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();

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
						s_errCount++;
						s_initializing = false;
						logger.Error("Error while initializing Task Scheduler", ex);
					}
					finally
					{
						var tooManyFailures = s_errCount >= 10;

						if (tooManyFailures || (taskScheduler != null && taskScheduler.IsActive))
						{
							GlobalFilters.Filters.Remove(this);
						}

						if (tooManyFailures && logger != null)
						{
							logger.Warning("Stopped trying to initialize the Task Scheduler: too many failed attempts in succession (10+). Maybe uncommenting the setting 'sm:TaskSchedulerBaseUrl' in web.config solves the problem?");
						}
					}
                }
            }
        }
    }
}
