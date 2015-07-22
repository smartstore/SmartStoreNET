using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Tasks
{

    /// <summary>
    /// Represents task manager
    /// </summary>
    public partial class TaskManager
    {
        private static readonly TaskManager _taskManager = new TaskManager();
        private readonly List<TaskThread> _taskThreads = new List<TaskThread>();

        private TaskManager()
        {
        }
        
        /// <summary>
        /// Initializes the task manager with the property values specified in the configuration file.
        /// </summary>
        public void Initialize()
        {
            this._taskThreads.Clear();

            var taskService = EngineContext.Current.Resolve<IScheduleTaskService>();
            var scheduleTasks = taskService.GetAllTasks();

			var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
			eventPublisher.Publish(new AppInitScheduledTasksEvent {
				ScheduledTasks = scheduleTasks
			});

            // group by threads with the same seconds
            foreach (var scheduleTaskGrouped in scheduleTasks.GroupBy(x => x.Seconds))
            {
                // create a thread
                var taskThread = new TaskThread();
                taskThread.Seconds = scheduleTaskGrouped.Key;

                foreach (var scheduleTask in scheduleTaskGrouped)
                {
					var taskType = System.Type.GetType(scheduleTask.Type);
					if (taskType != null)
					{
						var isActiveModule = PluginManager.IsActivePluginAssembly(taskType.Assembly);
						if (isActiveModule)
						{
							var job = new Job(scheduleTask);
							taskThread.AddJob(job);
						}
					}
                }

				if (taskThread.HasJobs)
				{
					this._taskThreads.Add(taskThread);
				}
            }


            //one thread, one task
            //foreach (var scheduleTask in scheduleTasks)
            //{
            //    var taskThread = new TaskThread(scheduleTask);
            //    this._taskThreads.Add(taskThread);
            //    var task = new Task(scheduleTask);
            //    taskThread.AddTask(task);
            //}
        }

        /// <summary>
        /// Starts the task manager
        /// </summary>
        public void Start()
        {
            foreach (var taskThread in this._taskThreads)
            {
                taskThread.InitTimer();
            }
        }

        /// <summary>
        /// Stops the task manager
        /// </summary>
        public void Stop()
        {
            foreach (var taskThread in this._taskThreads)
            {
                taskThread.Dispose();
            }
        }

        /// <summary>
        /// Gets the task mamanger instance
        /// </summary>
        public static TaskManager Instance
        {
            get
            {
                return _taskManager;
            }
        }

        /// <summary>
        /// Gets a list of task threads of this task manager
        /// </summary>
        public IList<TaskThread> TaskThreads
        {
            get
            {
                return new ReadOnlyCollection<TaskThread>(this._taskThreads);
            }
        }
    }
}
