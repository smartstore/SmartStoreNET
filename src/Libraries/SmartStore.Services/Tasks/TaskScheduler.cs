using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using SmartStore.Core.Async;

namespace SmartStore.Services.Tasks
{

	public class DefaultTaskScheduler : DisposableObject, ITaskScheduler, IRegisteredObject
    {
        private string _baseUrl;
        private System.Timers.Timer _timer;
        private bool _shuttingDown;
        private readonly ConcurrentDictionary<string, bool> _authTokens = new ConcurrentDictionary<string, bool>();

        public DefaultTaskScheduler()
        {
			_timer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _timer.Elapsed += Elapsed;

            HostingEnvironment.RegisterObject(this);
        }

        public TimeSpan SweepInterval
        {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
        }

        public string BaseUrl
        {
            // TODO: HTTPS?
			get { return _baseUrl; }
            set
            {
                CheckUrl(value);
                _baseUrl = value.TrimEnd('/', '\\');
            }
        }

        public void Start()
        {
            lock (_timer)
            {
                CheckUrl(_baseUrl);
                _timer.Start();
            }
        }

        public void Stop()
        {
            lock (_timer)
            {
                _timer.Stop();
            }
        }

        public bool IsActive
        {
            get { return _timer.Enabled; }
        }

		public IEnumerable<TaskProgressInfo> GetAllRunningTasks()
		{
			var tasks = AsyncState.Current.GetAll<TaskProgressInfo>();
			return tasks.Select(x => x.Clone());
		}

		public bool IsTaskRunning(int scheduleTaskId)
		{
			var exists = AsyncState.Current.Exists<TaskProgressInfo>(scheduleTaskId.ToString());
			return exists;
		}

		public TaskProgressInfo GetRunningTask(int scheduleTaskId)
		{
			var info = AsyncState.Current.Get<TaskProgressInfo>(scheduleTaskId.ToString());
			if (info != null)
				return info.Clone();

			return null;
		}

		public CancellationTokenSource GetCancelTokenSourceFor(int scheduleTaskId)
		{
			var cts = AsyncState.Current.GetCancelTokenSource<TaskProgressInfo>(scheduleTaskId.ToString());
			return cts;
		}

        public bool VerifyAuthToken(string authToken)
        {
            if (authToken.IsEmpty())
                return false;

            bool val = false;
            return _authTokens.TryRemove(authToken, out val);
        }

        public void RunSingleTask(int scheduleTaskId)
        {
            CallEndpoint(_baseUrl + "/Execute/{0}".FormatInvariant(scheduleTaskId));
        }

		private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!System.Threading.Monitor.TryEnter(_timer))
                return;

            try
            {
                if (_timer.Enabled)
                {
                    CallEndpoint(_baseUrl + "/Sweep");
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_timer);
            }
        }

        protected internal virtual void CallEndpoint(string url)
        {
            if (_shuttingDown)
                return;
            
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "SmartStore.NET";
            req.Method = "POST";
            req.ContentType = "text/plain";
            req.ContentLength = 0;

            string authToken = Guid.NewGuid().ToString();
            _authTokens.TryAdd(authToken, true);
            req.Headers.Add("X-AUTH-TOKEN", authToken);

            req.GetResponseAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // TODO: Now what?! Disable timer?
                }
                t.Result.Dispose();
            });
        }

        private void CheckUrl(string url)
        {
            if (!url.IsWebUrl())
            {
                throw Error.InvalidOperation("A valid base url is required for the background task scheduler.");
            }
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            _shuttingDown = true;
            HostingEnvironment.UnregisterObject(this); 
        }
    }

}
