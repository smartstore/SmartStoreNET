using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace SmartStore.Core.Async
{

	public class AsyncState
	{
		private readonly static AsyncState s_instance = new AsyncState();
		private readonly MemoryCache _cache = MemoryCache.Default;

		private AsyncState()
		{
		}

		public static AsyncState Current
		{
			get { return s_instance; }
		}

		public bool Exists<T>(string name = null)
		{
			var key = BuildKey<T>(name);
			return _cache.Contains(key) && !object.Equals(((StateInfo)_cache[key]).Progress, default(T));
		}

		public T Get<T>(string name = null)
		{
			CancellationTokenSource cancelTokenSource;
			return Get<T>(out cancelTokenSource, name);
		}

		public CancellationTokenSource GetCancelTokenSource<T>(string name = null)
		{
			CancellationTokenSource cancelTokenSource;
			Get<T>(out cancelTokenSource, name);
			return cancelTokenSource;
		}

		private T Get<T>(out CancellationTokenSource cancelTokenSource, string name = null)
		{
			cancelTokenSource = null;
			var key = BuildKey<T>(name);

			var value = _cache.Get(key) as StateInfo;

			if (value != null)
			{
				cancelTokenSource = value.CancellationTokenSource;
				return (T)(value.Progress);
			}

			return default(T);
		}

		public void Set<T>(T state, string name = null, bool neverExpires = false)
		{
			this.Set(state, null, name, neverExpires);
		}

		public void SetCancelTokenSource<T>(CancellationTokenSource cancelTokenSource, string name = null)
		{
			Guard.ArgumentNotNull(() => cancelTokenSource);

			this.Set<T>(default(T), cancelTokenSource, name);
		}

		private void Set<T>(T state, CancellationTokenSource cancelTokenSource, string name = null, bool neverExpires = false)
		{
			var key = BuildKey(typeof(T), name);

			var value = _cache.Get(key) as StateInfo;

			if (value != null)
			{
				// exists already, so update
				value.Progress = state;
				if (cancelTokenSource != null && value.CancellationTokenSource == null)
				{
					value.CancellationTokenSource = cancelTokenSource;
				}
			}

			var policy = new CacheItemPolicy { SlidingExpiration = neverExpires ? TimeSpan.Zero : TimeSpan.FromMinutes(15) };

			_cache.Set(key, value ?? new StateInfo { Progress = state, CancellationTokenSource = cancelTokenSource }, policy);
		}

		public bool Remove<T>(string name = null)
		{
			var key = BuildKey<T>(name);
			var value = _cache.Remove(key) as StateInfo;

			if (value != null)
			{
				if (value.CancellationTokenSource != null)
				{
					value.CancellationTokenSource.Dispose();
				}
				return true;
			}

			return false;
		}

		private string BuildKey<T>(string name)
		{
			return BuildKey(typeof(T), name);
		}

		private string BuildKey(Type type, string name)
		{
			return "{0}:{1}:{2}".FormatInvariant("AsyncStateKey", type.FullName, name.EmptyNull());
		}

		class StateInfo
		{
			public object Progress
			{
				get;
				set;
			}

			public CancellationTokenSource CancellationTokenSource
			{
				get;
				set;
			}
		}
	}

}
