using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartStore.Core.Async
{
	
	public class AsyncState
	{
		private readonly static AsyncState s_instance = new AsyncState();
		private readonly ConcurrentDictionary<StateKey, StateInfo> _states = new ConcurrentDictionary<StateKey, StateInfo>();

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
			return _states.ContainsKey(key) && !object.Equals(_states[key].Progress, default(T));
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
			StateInfo value;
			if (_states.TryGetValue(key, out value))
			{
				cancelTokenSource = value.CancellationTokenSource;
				return (T)(value.Progress);
			}

			return default(T);
		}

		public void Set<T>(T state, string name = null)
		{
			this.Set(state, null, name);
		}

		public void SetCancelTokenSource<T>(CancellationTokenSource cancelTokenSource, string name = null)
		{
			Guard.ArgumentNotNull(() => cancelTokenSource);

			this.Set<T>(default(T), cancelTokenSource, name);
		}

		private void Set<T>(T state, CancellationTokenSource cancelTokenSource, string name = null)
		{
			_states.AddOrUpdate(
				BuildKey(typeof(T), name),
				(key) => new StateInfo { Progress = state, CancellationTokenSource = cancelTokenSource },
				(key, old) => 
				{ 
					old.Progress = state;
					if (cancelTokenSource != null && old.CancellationTokenSource == null)
					{
						old.CancellationTokenSource = cancelTokenSource;
					}
					return old; 
				});
		}

		public bool Remove<T>(string name = null)
		{
			var key = BuildKey<T>(name);
			StateInfo value;
			if (_states.TryRemove(key, out value)) 
			{
				if (value.CancellationTokenSource != null)
				{
					value.CancellationTokenSource.Dispose();
				}
				return true;
			}

			return false;
		}

		private StateKey BuildKey<T>(string name)
		{
			return BuildKey(typeof(T), name);
		}

		private StateKey BuildKey(Type type, string name)
		{
			return new StateKey(type, name);
		}

		class StateKey : Tuple<Type, string>
		{
			public StateKey(Type type, string token) : base(type, token)
			{
				Guard.ArgumentNotNull(() => type);
			}
			
			public Type StateType 
			{
				get { return base.Item1; }
			}

			public string Token
			{
				get { return base.Item2; }
			}
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
