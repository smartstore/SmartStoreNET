using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;

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
        }

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

        public virtual IList<ScheduleTask> GetAllTasks(bool showHidden = false)
        {
            var query = _taskRepository.Table;
            if (!showHidden)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderBy(t => t.Seconds);

            var tasks = query.ToList();
            return tasks;
        }

        public virtual IList<ScheduleTask> GetPendingTasks()
        {
            var now = DateTime.UtcNow;

            var query = from t in _taskRepository.Table
                        where t.Enabled && t.NextRunUtc.HasValue && t.NextRunUtc <= now
                        orderby t.Seconds
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

		public virtual void EnsureTaskIsNotRunning(int taskId)
		{
			try
			{
				if (taskId != 0)
				{
					_taskRepository.Context.ExecuteSqlCommand("Update [dbo].[ScheduleTask] Set [LastEndUtc] = [LastStartUtc] Where Id = {0} And [LastEndUtc] < [LastStartUtc]",
						true, null, taskId);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
		}

		public void CalculateNextRunTimes(IEnumerable<ScheduleTask> tasks)
		{
			Guard.ArgumentNotNull(() => tasks);
			
			using (var scope = new DbContextScope(autoCommit: false))
			{
				var now = DateTime.UtcNow;
				foreach (var task in tasks)
				{
					task.NextRunUtc = now.AddSeconds(task.Seconds);
					this.UpdateTask(task);
				}

				scope.Commit();
			}
		}

        #endregion

	}
}
