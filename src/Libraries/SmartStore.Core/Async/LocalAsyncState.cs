using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Core.Async
{
	public partial class LocalAsyncState : IAsyncState
	{
		private readonly MemoryCache _states = new MemoryCache("SmartStore.AsyncState.Progress");
		private readonly MemoryCache _cancelTokens = new MemoryCache("SmartStore.AsyncState.CancelTokenSources");

		public virtual bool Exists<T>(string name = null)
		{
			var value = GetStateInfo<T>(name);
			return value != null && !object.Equals(value.Progress, default(T));
		}

		public virtual T Get<T>(string name = null)
		{
			var value = GetStateInfo<T>(name);

			if (value != null)
			{
				return (T)value.Progress;
			}

			return default(T);
		}

		public virtual IEnumerable<T> GetAll<T>()
		{
			var keyPrefix = BuildKey<T>(null);
			return _states
				.Where(x => x.Key.StartsWith(keyPrefix))
				.Select(x => x.Value)
				.OfType<AsyncStateInfo>()
				.Select(x => x.Progress)
				.OfType<T>();
		}


		public virtual void Set<T>(T state, string name = null, bool neverExpires = false)
		{
			Guard.NotNull(state, nameof(state));

			var value = GetStateInfo<T>(name);

			if (value != null)
			{
				// exists already, so update
				if (state != null)
				{
					value.Progress = state;
				}
			}
			else
			{
				// add new entry
				var duration = neverExpires ? TimeSpan.Zero : TimeSpan.FromMinutes(15);
				var policy = new CacheItemPolicy { SlidingExpiration = duration };
				var key = BuildKey<T>(name);

				// On expiration or removal: remove corresponding cancel token also.
				policy.RemovedCallback = (x) => OnRemoveCancelTokenSource(key);

				_states.Set(key, new AsyncStateInfo { Progress = state, Duration = duration }, policy);
			}
		}

		public virtual void Update<T>(Action<T> update, string name = null)
		{
			Guard.NotNull(update, nameof(update));

			var value = GetStateInfo<T>(name);

			if (value != null)
			{
				var state = (T)value.Progress;
				if (state != null)
				{
					update(state);
				}
			}
		}

		public virtual bool Remove<T>(string name = null)
		{
			var key = BuildKey<T>(name);

			if (OnRemoveStateInfo(key))
			{
				OnRemoveCancelTokenSource(key);
				return true;
			}

			return false;
		}

		protected virtual bool OnRemoveStateInfo(string key)
		{
			return _states.Remove(key) != null;
		}

		protected virtual bool OnRemoveCancelTokenSource(string key, bool successive = false)
		{
			Guard.NotEmpty(key, nameof(key));

			var token = _cancelTokens.Remove(key) as CancellationTokenSource;

			if (token != null)
			{
				token.Dispose();
				return true;
			}

			return false;
		}


		public CancellationTokenSource GetCancelTokenSource<T>(string name = null)
		{
			return OnGetCancelTokenSource(BuildKey<T>(name));
		}

		protected virtual CancellationTokenSource OnGetCancelTokenSource(string key, bool successive = false)
		{
			Guard.NotEmpty(key, nameof(key));

			var token = _cancelTokens.Get(key);

			if (token != null)
			{
				return (CancellationTokenSource)token;
			}

			return null;
		}

		public virtual void SetCancelTokenSource<T>(CancellationTokenSource cancelTokenSource, string name = null)
		{
			Guard.NotNull(cancelTokenSource, nameof(cancelTokenSource));

			var key = BuildKey<T>(name);

			if (Exists<T>(name))
			{
				OnRemoveCancelTokenSource(key);
			}		

			var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(15) };

			_cancelTokens.Set(key, cancelTokenSource, policy);
		}

		public bool Cancel<T>(string name = null)
		{
			return OnCancel(BuildKey<T>(name));
		}

		protected virtual bool OnCancel(string key, bool successive = false)
		{
			Guard.NotEmpty(key, nameof(key));

			var cts = OnGetCancelTokenSource(key);

			if (cts != null)
			{
				cts.Cancel();
				return true;
			}

			return false;
		}


		protected virtual AsyncStateInfo GetStateInfo<T>(string name = null)
		{
			var key = BuildKey<T>(name);
			var value = _states.Get(key) as AsyncStateInfo;

			if (value != null)
			{
				// ensures that sliding expiration gets updated on cancel token
				_cancelTokens.Get(key);
			}

			return value;
		}

		protected string BuildKey<T>(string name)
		{
			return BuildKey(typeof(T), name);
		}

		protected virtual string BuildKey(Type type, string name)
		{
			return "{0}{1}".FormatInvariant(type.FullName, name.HasValue() ? ":" + name : "");
		}
	}
}
