using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Tasks
{

    public class TaskExecutor : ITaskExecutor
    {
        private readonly IScheduleTaskService _scheduledTaskService;
		private readonly IDbContext _dbContext;
        private readonly Func<Type, ITask> _taskResolver;

        public TaskExecutor(IScheduleTaskService scheduledTaskService, IDbContext dbContext, Func<Type, ITask> taskResolver)
        {
            this._scheduledTaskService = scheduledTaskService;
			this._dbContext = dbContext;
            this._taskResolver = taskResolver;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Execute(ScheduleTask task, bool throwOnError = false)
        {
            if (task.IsRunning)
                return; // TODO: Really?

            bool faulted = false;
            string lastError = null;
            ITask instance = null;

            try
            {
                instance = CreateTaskInstance(task);
                
                task.LastStartUtc = DateTime.UtcNow;
                task.LastEndUtc = null;
                task.NextRunUtc = null;
                _scheduledTaskService.UpdateTask(task);

                var ctx = new TaskExecutionContext
                {
                    ScheduleTask = task.Clone()
                    // TODO: Remove obsolete properties
                };

                instance.Execute(ctx);
            }
            catch (Exception ex)
            {
                faulted = true;
                Logger.Error(string.Format("Error while running scheduled task '{0}'. {1}", task.Name, ex.Message), ex);
                lastError = ex.Message.Truncate(995, "...");
                if (throwOnError)
                {
                    throw;
                }
            }
            finally
            {
                var now = DateTime.UtcNow;
                task.LastError = lastError;
                task.LastEndUtc = now;

                if (faulted)
                {
                    if (task.StopOnError || instance == null)
                    {
                        task.Enabled = false;
                    }
                }
                else
                {
                    task.LastSuccessUtc = now;
                }

                if (task.Enabled)
                {
                    task.NextRunUtc = task.LastStartUtc.Value.AddSeconds(task.Seconds);
                }

                _scheduledTaskService.UpdateTask(task);
            }
        }

        private ITask CreateTaskInstance(ScheduleTask task)
        {
            var type = Type.GetType(task.Type);
            return _taskResolver(type);
        }
    }

}
