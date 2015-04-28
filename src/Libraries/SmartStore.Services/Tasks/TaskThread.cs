using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Async;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Represents task thread
    /// </summary>
    public partial class TaskThread : IDisposable
    {
        private Timer _timer;
        private bool _disposed;
        private DateTime _startedUtc;
        private bool _isRunning;
        private readonly Dictionary<string, Job> _jobs;
        private int _seconds;

        internal TaskThread()
        {
            this._jobs = new Dictionary<string, Job>();
            this._seconds = 10 * 60;
        }

        internal TaskThread(ScheduleTask scheduleTask)
        {
            this._jobs = new Dictionary<string, Job>();
            this._seconds = scheduleTask.Seconds;
            this._isRunning = false;
        }

        private void Run()
        {
            if (_seconds <=0)
                return;

            this._startedUtc = DateTime.UtcNow;
            this._isRunning = true;

			var jobs = _jobs.Values
				.Select(job => AsyncRunner.Run((c, ct) => job.Execute(ct, c)))
				.ToArray();

			try
			{
				Task.WaitAll(jobs);
			}
			catch { }

            this._isRunning = false;
        }

        private void TimerHandler(object state)
        {
            this._timer.Change(-1, -1);
            this.Run();
            this._timer.Change(this.Interval, this.Interval);
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
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

        /// <summary>
        /// Inits a timer
        /// </summary>
        public void InitTimer()
        {
            if (this._timer == null)
            {
                this._timer = new Timer(new TimerCallback(this.TimerHandler), null, this.Interval, this.Interval);
            }
        }

        /// <summary>
        /// Adds a job to the thread
        /// </summary>
        /// <param name="job">The task to be added</param>
        public void AddJob(Job job)
        {
            if (!this._jobs.ContainsKey(job.Name))
            {
                this._jobs.Add(job.Name, job);
            }
        }


        /// <summary>
        /// Gets or sets the interval in seconds at which to run the jobs
        /// </summary>
        public int Seconds
        {
            get
            {
                return this._seconds;
            }
            internal set
            {
                this._seconds = value;
            }
        }

        /// <summary>
        /// Get a datetime when thread has been started
        /// </summary>
        public DateTime Started
        {
            get
            {
                return this._startedUtc;
            }
        }

        /// <summary>
        /// Get a value indicating whether thread is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this._isRunning;
            }
        }

        /// <summary>
        /// Get a list of jobs
        /// </summary>
        public IList<Job> Jobs
        {
            get
            {
                var list = new List<Job>();
                foreach (var jobs in this._jobs.Values)
                {
                    list.Add(jobs);
                }
                return new ReadOnlyCollection<Job>(list);
            }
        }

		public bool HasJobs
		{
			get { return _jobs.Count > 0; }
		}

        /// <summary>
        /// Gets the interval at which to run the jobs
        /// </summary>
        public int Interval
        {
            get
            {
				if (_seconds > (Int32.MaxValue / 1000))
					return Int32.MaxValue;

                return this._seconds * 1000;
            }
        }
    }
}
