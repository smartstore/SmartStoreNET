using System;
using System.Collections.Concurrent;
using SmartStore.Core.Logging;
using StackExchange.Profiling;

namespace SmartStore.DevTools.Services
{
	public class MiniProfilerChronometer : IChronometer
	{
		private readonly ConcurrentDictionary<string, ConcurrentStack<IDisposable>> _steps = new ConcurrentDictionary<string, ConcurrentStack<IDisposable>>();
		private MiniProfiler _profiler;

		public MiniProfilerChronometer()
		{
			_profiler = MiniProfiler.Current;
		}

		protected MiniProfiler Profiler
		{
			get
			{
				return _profiler ?? (_profiler = MiniProfiler.Current);
			}
		}

		public void StepStart(string key, string message)
		{
			if (this.Profiler == null)
			{
				return;
			}

			var stack = _steps.GetOrAdd(key, k => new ConcurrentStack<IDisposable>());
			var step = Profiler.Step(message);
			stack.Push(step);
		}

		public void StepStop(string key)
		{
			if (this.Profiler == null)
			{
				return;
			}

			IDisposable step;
			if (this._steps.ContainsKey(key))
			{
				if (this._steps[key].TryPop(out step))
				{
					step.Dispose();
				}
			}
		}

		private void StopAll()
		{
			// Dispose any orphaned steps
			foreach (var stack in this._steps.Values)
			{
				IDisposable step;
				while (stack.TryPop(out step))
				{
					step.Dispose();
				}
			}
		}

		public void Dispose()
		{
			this.StopAll();
		}
	}
}