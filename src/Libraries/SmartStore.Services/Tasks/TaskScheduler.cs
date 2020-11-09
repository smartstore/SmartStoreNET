using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using SmartStore.Core.Logging;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core;

namespace SmartStore.Services.Tasks
{
    public class DefaultTaskScheduler : DisposableObject, ITaskScheduler, IRegisteredObject
    {
        private readonly ICacheManager _cache;

        private bool _intervalFixed;
        private int _sweepInterval;
        private string _baseUrl;
        private System.Timers.Timer _timer;
        private bool _shuttingDown;
        private int _errCount;

        public DefaultTaskScheduler(ICacheManager cache)
        {
            _cache = cache;

            _sweepInterval = 1;
            _timer = new System.Timers.Timer();
            _timer.Elapsed += Elapsed;

            Logger = NullLogger.Instance;
            HostingEnvironment.RegisterObject(this);
        }

        public ILogger Logger
        {
            get;
            set;
        }

        public int SweepIntervalMinutes
        {
            get => _sweepInterval;
            set => _sweepInterval = value;
        }

        public string BaseUrl
        {
            // TODO: HTTPS?
            get => _baseUrl;
            set
            {
                CheckUrl(value);
                _baseUrl = value.TrimEnd('/', '\\');
            }
        }

        public void Start()
        {
            if (_timer.Enabled)
                return;

            lock (_timer)
            {
                CheckUrl(_baseUrl);
                _timer.Interval = GetFixedInterval();
                _timer.Start();
            }
        }

        private double GetFixedInterval()
        {
            // Gets seconds to next sweep minute
            int seconds = (_sweepInterval * 60) - DateTime.Now.Second;
            return seconds * 1000;
        }

        public void Stop()
        {
            if (!_timer.Enabled)
                return;

            lock (_timer)
            {
                _timer.Stop();
            }
        }

        public bool IsActive => _timer.Enabled;

        public string GetAsyncStateKey(int scheduleTaskId)
        {
            return scheduleTaskId.ToString();
        }

        private string CreateAuthToken()
        {
            string authToken = Guid.NewGuid().ToString();

            _cache.Put(GenerateAuthTokenCacheKey(authToken), true, TimeSpan.FromMinutes(1));

            return authToken;
        }

        private string GenerateAuthTokenCacheKey(string authToken)
        {
            return "Scheduler:AuthToken:" + authToken;
        }

        public bool VerifyAuthToken(string authToken)
        {
            if (authToken.IsEmpty())
                return false;

            var cacheKey = GenerateAuthTokenCacheKey(authToken);
            if (_cache.Contains(cacheKey))
            {
                _cache.Remove(cacheKey);
                return true;
            }

            return false;
        }

        public void RunSingleTask(int scheduleTaskId, IDictionary<string, string> taskParameters = null)
        {
            taskParameters = taskParameters ?? new Dictionary<string, string>();

            // User executes task in backend explicitly
            taskParameters["Explicit"] = "true";

            var qs = new QueryString();
            taskParameters.Each(x => qs.Add(x.Key, x.Value));
            var query = qs.ToString();

            CallEndpoint(new Uri("{0}/Execute/{1}{2}".FormatInvariant(_baseUrl, scheduleTaskId, query)));
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!System.Threading.Monitor.TryEnter(_timer))
                return;

            try
            {
                if (_timer.Enabled)
                {
                    if (!_intervalFixed)
                    {
                        _timer.Interval = TimeSpan.FromMinutes(_sweepInterval).TotalMilliseconds;
                        _intervalFixed = true;
                    }

                    CallEndpoint(new Uri(_baseUrl + "/Sweep"));
                }
            }
            finally
            {
                System.Threading.Monitor.Exit(_timer);
            }
        }

        protected internal virtual void CallEndpoint(Uri uri)
        {
            if (_shuttingDown)
                return;

            var req = WebHelper.CreateHttpRequestForSafeLocalCall(uri);
            req.Method = "POST";
            req.ContentType = "text/plain";
            req.ContentLength = 0;
            req.Timeout = 10000; // 10 sec.

            string authToken = CreateAuthToken();
            req.Headers.Add("X-AUTH-TOKEN", authToken);

            req.GetResponseAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    HandleException(t.Exception, uri);
                    _errCount++;
                    if (_errCount >= 10)
                    {
                        // 10 failed attempts in succession. Stop the timer!
                        this.Stop();
                        Logger.Info("Stopping TaskScheduler sweep timer. Too many failed requests in succession.");
                    }
                }
                else
                {
                    _errCount = 0;
                    var response = t.Result;

                    //_logger.DebugFormat("TaskScheduler Sweep called successfully: {0}", response.GetResponseStream().AsString());

                    response.Dispose();
                }
            });
        }

        private void HandleException(AggregateException exception, Uri uri)
        {
            string msg = "Error while calling TaskScheduler endpoint '{0}'.".FormatInvariant(uri.OriginalString);
            var wex = exception.InnerExceptions.OfType<WebException>().FirstOrDefault();

            if (wex == null)
            {
                Logger.Error(exception.InnerException, msg);
            }
            else if (wex.Response == null)
            {
                Logger.Error(wex, msg);
            }
            else
            {
                using (var response = wex.Response as HttpWebResponse)
                {
                    if (response != null)
                    {
                        var statusCode = (int)response.StatusCode;
                        if (statusCode < 500)
                        {
                            // Any internal server error (>= 500) already handled by TaskSchedulerController's exception filter
                            msg += " HTTP {0}, {1}".FormatCurrent(statusCode, response.StatusDescription);
                            Logger.Error(msg);
                        }
                    }

                }
            }
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
