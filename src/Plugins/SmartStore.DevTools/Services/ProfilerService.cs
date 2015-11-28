using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using StackExchange.Profiling;

namespace SmartStore.DevTools.Services
{
	public class ProfilerService : IProfilerService, IDisposable
	{
		private readonly ConcurrentDictionary<string, ConcurrentStack<IDisposable>> steps = new ConcurrentDictionary<string, ConcurrentStack<IDisposable>>();
		private MiniProfiler _profiler;

		public ProfilerService()
		{
			this._profiler = MiniProfiler.Current;
		}

		protected MiniProfiler Profiler
		{
			get
			{
				return this._profiler ?? (this._profiler = MiniProfiler.Current);
			}
		}

		public void StepStart(string key, string message)
		{
			if (this.Profiler == null)
			{
				return;
			}

			var stack = this.steps.GetOrAdd(key, k => new ConcurrentStack<IDisposable>());
			var step = this.Profiler.Step(message);
			stack.Push(step);
		}

		public void StepStop(string key)
		{
			if (this.Profiler == null)
			{
				return;
			}

			IDisposable step;
			if (this.steps.ContainsKey(key))
			{
				if (this.steps[key].TryPop(out step))
				{
					step.Dispose();
				}
			}
		}

		private void StopAll()
		{
			// Dispose any orphaned steps
			foreach (var stack in this.steps.Values)
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