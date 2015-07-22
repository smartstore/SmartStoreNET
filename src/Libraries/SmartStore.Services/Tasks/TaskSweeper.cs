using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Hosting;

namespace SmartStore.Services.Tasks
{
    
    public class TaskSweeper : DisposableObject, ITaskSweeper
    {
        private string _baseUrl;
        private Timer _timer;
        private bool _shuttingDown;
        private readonly ConcurrentDictionary<string, bool> _authTokens = new ConcurrentDictionary<string, bool>();

        public TaskSweeper()
        {
            _timer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _timer.Elapsed += Elapsed;

            HostingEnvironment.RegisterObject(this);
        }

        public TimeSpan Interval
        {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
        }

        public string BaseUrl
        {
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

        public bool IsRunning
        {
            get { return _timer.Enabled; }
        }

        public bool VerifyAuthToken(string authToken)
        {
            if (authToken.IsEmpty())
                return false;

            bool val = false;
            return _authTokens.TryRemove(authToken, out val);
        }

        public void ExecuteSingleTask(int scheduleTaskId)
        {
            CallEndpoint(_baseUrl + "/Execute/{0}".FormatInvariant(scheduleTaskId));
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
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

        private void CallEndpoint(string url)
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
