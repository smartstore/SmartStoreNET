using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Task service
    /// </summary>
    public partial class ScheduleTaskService : IScheduleTaskService
    {
        #region Fields

        private readonly IRepository<ScheduleTask> _taskRepository;

        #endregion

        #region Ctor

        public ScheduleTaskService(IRepository<ScheduleTask> taskRepository)
        {
            this._taskRepository = taskRepository;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }

        #endregion

        #region Methods

        public virtual void DeleteTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Delete(task);
        }

        public virtual ScheduleTask GetTaskById(int taskId)
        {
            if (taskId == 0)
                return null;

            return _taskRepository.GetById(taskId);
        }

        public virtual ScheduleTask GetTaskByType(string type)
        {
			try
			{
				if (type.HasValue())
				{
					var query = _taskRepository.Table
						.Where(t => t.Type == type)
						.OrderByDescending(t => t.Id);

					var task = query.FirstOrDefault();
					return task;
				}
			}
			catch (Exception exc)
			{
				// do not throw an exception if the underlying provider failed on Open.
				exc.Dump();
			}
			return null;
        }

		public virtual IList<ScheduleTask> GetAllTasks(bool includeDisabled = false)
        {
            var query = _taskRepository.Table;
			if (!includeDisabled)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderByDescending(t => t.Enabled).ThenBy(t => t.Seconds);

            var tasks = query.ToList();
            return tasks;
        }

        public virtual IList<ScheduleTask> GetPendingTasks()
        {
            var now = DateTime.UtcNow;

            var query = from t in _taskRepository.Table
                        where t.Enabled && t.NextRunUtc.HasValue && t.NextRunUtc <= now
                        orderby t.NextRunUtc, t.Seconds
                        select t;

            return query.ToList();
        }

        public virtual void InsertTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Insert(task);
        }

        public virtual void UpdateTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Update(task);
        }

		public void CalculateNextRunTimes(IEnumerable<ScheduleTask> tasks, bool isAppStart = false)
		{
			Guard.ArgumentNotNull(() => tasks);
			
			using (var scope = new DbContextScope(autoCommit: false))
			{
				var now = DateTime.UtcNow;
				foreach (var task in tasks)
				{
					task.NextRunUtc = task.Enabled ? now.AddSeconds(task.Seconds) : (DateTime?)null;
					if (isAppStart && task.LastEndUtc.GetValueOrDefault() < task.LastStartUtc)
					{
						task.LastEndUtc = task.LastStartUtc;
						task.LastError = T("Admin.System.ScheduleTasks.AbnormalAbort");
					}
					this.UpdateTask(task);
				}

				scope.Commit();
			}
		}

        #endregion

	}
}
