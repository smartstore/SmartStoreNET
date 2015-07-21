using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using System.Threading;
using System.Net;
using System;

namespace SmartStore.Services.Tasks
{
    
    public class DefaultTaskManager : DisposableObject
    {
        private bool _isInitialized;
        private bool _isSuspended;
        private string _url;
        private Timer _timer;
        private bool _disposed;

        public void Start(string url, int interval)
        {
            Guard.ArgumentNotEmpty(() => url);
            Guard.ArgumentNotZero(interval, "interval");

            this._url = url;
            this._timer = new Timer(new TimerCallback(this.CallEndpoint), null, interval * 1000, interval * 1000);
        }

        public void ChangeUrl(string url)
        {
            Guard.ArgumentNotEmpty(() => url);

            this._url = url;
        }

        public void ChangeInterval(int interval)
        {
            this._timer.Change(interval * 1000, interval * 1000);
        }

        public void Suspend()
        {
            _isSuspended = true;
        }

        public void Resume()
        {
            _isSuspended = false;
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public bool IsRunning
        {
            get { return _isInitialized && !_isSuspended; }
        }

        public bool VerifyAuthToken(string authToken)
        {
            // TODO: Verify!
            return true;
        }

        private void CallEndpoint(object state)
        {
            var req = (HttpWebRequest)WebRequest.Create(_url);
            req.UserAgent = "SmartStore.NET";
            req.Headers.Add("X-AUTH-TOKEN", Guid.NewGuid().ToString());
            req.Method = "POST";
            req.ContentType = "text/plain";
            req.ContentLength = 0;

            req.GetResponseAsync().ContinueWith(t => t.Result.Dispose());

            //using (var resp = (HttpWebResponse)req.GetResponse())
            //{
            //    //using (var stream = resp.GetResponseStream())
            //    //{
            //    //}
            //}
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if ((this._timer != null) && !this._disposed)
                {
                    lock (this)
                    {
                        this._timer.Dispose();
                        this._timer = null;
                        this._disposed = true;
                    }
                }
            }
        }
    }
    
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
