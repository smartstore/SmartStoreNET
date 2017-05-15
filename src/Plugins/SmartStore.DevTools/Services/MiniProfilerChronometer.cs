using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SmartStore.Core.Logging;
using StackExchange.Profiling;

namespace SmartStore.DevTools.Services
{
	public class MiniProfilerChronometer : IChronometer
	{
		private readonly Dictionary<string, Stack<IDisposable>> _steps = new Dictionary<string, Stack<IDisposable>>();
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

			var stack = _steps.Get(key);
			if (stack == null)
			{
				_steps[key] = stack = new Stack<IDisposable>();
			}

			var step = Profiler.Step(message);
			stack.Push(step);
		}

		public void StepStop(string key)
		{
			if (_steps.ContainsKey(key) && _steps[key].Count > 0)
			{
				var step = _steps[key].Pop();
				step.Dispose();
				if (_steps[key].Count == 0)
				{
					_steps.Remove(key);
				}
			}
		}

		private void StopAll()
		{
			// Dispose any orphaned steps
			foreach (var stack in _steps.Values)
			{
				stack.Each(x => x.Dispose());
			}

			_steps.Clear();
		}

		public void Dispose()
		{
			this.StopAll();
		}
	}
}