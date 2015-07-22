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

                    var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();
                    var tasks = taskService.GetAllTasks();

                    var now = DateTime.UtcNow;
                    foreach (var task in tasks)
                    {
                        task.NextRunUtc = now.AddSeconds(task.Seconds);
                        taskService.UpdateTask(task);
                    }

                    var taskSweeper = EngineContext.Current.Resolve<ITaskSweeper>();

                    taskSweeper.SetBaseUrl(EngineContext.Current.Resolve<IStoreService>(), filterContext.HttpContext);
                    taskSweeper.Start();

                    var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
                    eventPublisher.Publish(new AppInitScheduledTasksEvent { ScheduledTasks = tasks });

                    GlobalFilters.Filters.Remove(this);
                }
            }
        }
    }
}
