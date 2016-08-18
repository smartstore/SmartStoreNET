using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Core.Async
{
	public sealed class AsyncLock : DisposableObject
	{
		private static readonly TimeSpan _noTimeout = TimeSpan.FromMilliseconds(-1);
		private static readonly ConcurrentDictionary<object, AsyncLock> _keyLocks = new ConcurrentDictionary<object, AsyncLock>();

		private readonly SemaphoreSlim _semaphore;
		private readonly int _initialCount;

		public AsyncLock()
			: this(1, int.MaxValue)
		{
		}

		public AsyncLock(int initialCount)
			: this(initialCount, int.MaxValue)
		{
		}

		public AsyncLock(int initialCount, int maxCount)
		{
			_semaphore = new SemaphoreSlim(initialCount, maxCount);
			_initialCount = initialCount;
		}

		public static AsyncLock Acquire(object key)
		{
			Guard.NotNull(key, nameof(key));

			var asyncLock = _keyLocks.GetOrAdd(key, x => new AsyncLock());

			// Perf: once we have more than 100 lock objects in the storage, remove the idle ones.
			if (_keyLocks.Count > 100)
			{
				// never remove the current one and the locked ones
				var idles = _keyLocks.Where(kvp => kvp.Key != key && !kvp.Value.IsLockHeld).ToArray();
				foreach (var kvp in idles)
				{
					var value = kvp.Value;
					// double check for held lock
					if (!value.IsLockHeld)
					{
						_keyLocks.TryRemove(kvp.Key, out value);
					}
				}
			}

			return asyncLock;
		}

		public bool IsLockHeld
		{
			get { return _semaphore.CurrentCount < _initialCount; }
		}

		public int Release()
		{
			return _semaphore.Release();
		}

		public int Release(int releaseCount)
		{
			return _semaphore.Release(releaseCount);
		}

		public Task<IDisposable> LockAsync()
		{
			return LockAsync(_noTimeout, CancellationToken.None);
		}

		public Task<IDisposable> LockAsync(TimeSpan timeout)
		{
			return LockAsync(timeout, CancellationToken.None);
		}

		public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
		{
			return LockAsync(_noTimeout, cancellationToken);
		}

		public Task<IDisposable> LockAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			var wait = _semaphore.WaitAsync(timeout, cancellationToken);

			return wait.IsCompleted
				? Task.FromResult(new ActionDisposable(() => _semaphore.Release()) as IDisposable)
				: wait.ContinueWith((t, s) => new ActionDisposable(() => ((SemaphoreSlim)s).Release()) as IDisposable,
					_semaphore,
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
		}

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
				_semaphore.Dispose();
		}
	}
}
