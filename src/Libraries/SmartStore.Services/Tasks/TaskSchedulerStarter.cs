using System;
using System.Web;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.Tasks
{
    public sealed class TaskSchedulerStarter : IPostApplicationStart
    {
        private readonly ITaskScheduler _taskScheduler;
        private readonly IScheduleTaskService _taskService;
        private readonly IStoreService _storeService;
        private readonly IEventPublisher _eventPublisher;

        public TaskSchedulerStarter(
            ITaskScheduler taskScheduler,
            IScheduleTaskService taskService,
            IStoreService storeService,
            IEventPublisher eventPublisher)
        {
            _taskScheduler = taskScheduler;
            _taskService = taskService;
            _storeService = storeService;
            _eventPublisher = eventPublisher;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public int Order => int.MinValue;
        public int MaxAttempts => 10;
        public bool ThrowOnError => false;

        public void Start(HttpContextBase httpContext)
        {
            var tasks = _taskService.GetAllTasks(true);
            _taskService.CalculateFutureSchedules(tasks, true /* isAppStart */);

            var baseUrl = CommonHelper.GetAppSetting<string>("sm:TaskSchedulerBaseUrl");
            if (baseUrl.IsWebUrl())
            {
                _taskScheduler.BaseUrl = baseUrl;
            }
            else
            {
                // autoresolve base url
                _taskScheduler.SetBaseUrl(_storeService, httpContext);
            }

            _taskScheduler.SweepIntervalMinutes = CommonHelper.GetAppSetting<int>("sm:TaskSchedulerSweepInterval", 1);
            _taskScheduler.Start();

            Logger.Info("Initialized TaskScheduler with base url '{0}'".FormatInvariant(_taskScheduler.BaseUrl));

            _eventPublisher.Publish(new AppInitScheduledTasksEvent { ScheduledTasks = tasks });
        }

        public void OnFail(Exception exception, bool willRetry)
        {
            if (willRetry)
            {
                Logger.Error(exception, "Error while initializing Task Scheduler");
            }
            else
            {
                Logger.Warn("Stopped trying to initialize the Task Scheduler: too many failed attempts in succession (10+). Maybe uncommenting the setting 'sm:TaskSchedulerBaseUrl' in web.config solves the problem?");
            }
        }
    }
}
