using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Controllers
{
    
    public class TaskSchedulerController : Controller
    {
        private readonly DefaultTaskManager _taskManager;
        private readonly IRepository<ScheduleTask> _tasksRepository;
        private readonly Func<Type, ITask> _taskResolver;
        private readonly ICommonServices _services;

        public TaskSchedulerController(
            DefaultTaskManager taskManager,
            IRepository<ScheduleTask> tasksRepository,
            Func<Type, ITask> taskResolver,
            ICommonServices services)
        {
            this._taskManager = taskManager;
            this._tasksRepository = tasksRepository;
            this._taskResolver = taskResolver;
            this._services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }

        public ILogger Logger { get; set; }

        [HttpPost]
        public ActionResult Sweep()
        {
            var authToken = Request.Headers["X-AUTH-TOKEN"];
            if (authToken.IsEmpty())
                return new HttpUnauthorizedResult();

            if (!_taskManager.VerifyAuthToken(authToken))
                return new HttpUnauthorizedResult();

            var pendingTasks = GetPendingTasks();

            foreach (var task in pendingTasks)
            {
                var instance = CreateTaskInstance(task);
                ExecuteTask(instance, task);
            }
            
            return Content("{0} tasks executed".FormatInvariant(pendingTasks.Count));
        }

        private ICollection<ScheduleTask> GetPendingTasks()
        {
            var now = DateTime.UtcNow;
            
            var query = from t in _tasksRepository.Table
                        where t.Enabled && t.NextRunUtc.HasValue && t.NextRunUtc <= now
                        orderby t.Seconds
                        select t;

            return query.ToList();
        }

        private ITask CreateTaskInstance(ScheduleTask task)
        {
            var type = Type.GetType(task.Type);
            return _taskResolver(type);
        }

        private void ExecuteTask(ITask instance, ScheduleTask task)
        {
            bool faulted = false;
            string lastError = null;
            
            try
            {
                task.NextRunUtc = null;
                task.LastStartUtc = DateTime.UtcNow;
                _tasksRepository.Update(task);

                // EXECUTE
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error while running the '{0}' schedule task. {1}", task.Name, ex.Message), ex);
                faulted = true;
                lastError = ex.Message.Truncate(995, "...");
            }
            finally
            {
                var now = DateTime.UtcNow;
                task.LastError = lastError;
                task.LastEndUtc = now;
                
                if (faulted)
                {
                    task.Enabled = task.StopOnError;
                }
                else
                {
                    task.LastSuccessUtc = now;
                }

                if (task.Enabled)
                {
                    task.NextRunUtc = task.LastStartUtc.Value.AddSeconds(task.Seconds);
                }

                _tasksRepository.Update(task);
            }
        }

    }
}