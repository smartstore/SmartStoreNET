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

                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    var path = urlHelper.Action("Sweep", "TaskScheduler");

                    var request = filterContext.HttpContext.Request;

                    var taskManager = EngineContext.Current.Resolve<DefaultTaskManager>();
                    var url = WebHelper.GetAbsoluteUrl(path, request);

                    if (!request.IsLocal)
                    {
                        // TODO: get default store url
                    }

                    taskManager.Start(url, 15 /* seconds */);

                    var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
                    eventPublisher.Publish(new AppInitScheduledTasksEvent { ScheduledTasks = tasks });

                    GlobalFilters.Filters.Remove(this);
                }
            }
        }
    }
}
