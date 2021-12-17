using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStore.Utilities.Threading
{
    public sealed class KeyedLock
    {
        private static readonly TimeSpan _noTimeout = TimeSpan.FromMilliseconds(-1);
        private static readonly ConcurrentDictionary<object, KeyedLock> _locks = new ConcurrentDictionary<object, KeyedLock>();

        private int _waiterCount;

        private KeyedLock(object key)
        {
            Key = key;
            Semaphore = new SemaphoreSlim(1, 1);
        }

        private object Key { get; set; }
        private SemaphoreSlim Semaphore { get; set; }

        public bool HasWaiters => _waiterCount > 1;

        public int WaiterCount => _waiterCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementCount()
        {
            Interlocked.Increment(ref _waiterCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DecrementCount()
        {
            Interlocked.Decrement(ref _waiterCount);
        }

        public static bool IsLockHeld(object key)
        {
            return _locks.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Lock(object key)
        {
            return Lock(key, _noTimeout);
        }

        public static IDisposable Lock(object key, TimeSpan timeout)
        {
            var keyedLock = GetOrCreateLock(key);
            keyedLock.Semaphore.Wait(timeout);
            return new Releaser(keyedLock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IDisposable> LockAsync(object key)
        {
            return LockAsync(key, _noTimeout, CancellationToken.None);
        }

        public static Task<IDisposable> LockAsync(object key, TimeSpan timeout)
        {
            return LockAsync(key, timeout, CancellationToken.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IDisposable> LockAsync(object key, CancellationToken cancellationToken)
        {
            return LockAsync(key, _noTimeout, cancellationToken);
        }

        public static Task<IDisposable> LockAsync(object key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var keyedLock = GetOrCreateLock(key);
            var wait = keyedLock.Semaphore.WaitAsync(timeout, cancellationToken);

            return wait.IsCompleted
                ? Task.FromResult(new Releaser(keyedLock) as IDisposable)
                : wait.ContinueWith((t, s) => new Releaser((KeyedLock)s) as IDisposable,
                    keyedLock,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }

        private static KeyedLock GetOrCreateLock(object key)
        {
            Guard.NotNull(key, nameof(key));

            var item = _locks.GetOrAdd(key, k => new KeyedLock(key));
            item.IncrementCount();
            return item;
        }

        sealed class Releaser : IDisposable
        {
            private KeyedLock _item;

            public Releaser(KeyedLock item)
            {
                _item = item;
            }

            public void Dispose()
            {
                _item.DecrementCount();
                if (_item.WaiterCount == 0)
                {
                    // Remove from dict
                    _locks.TryRemove(_item.Key, out _);
                }

                if (_item.Semaphore.CurrentCount == 0)
                {
                    _item.Semaphore.Release();
                }

                //_item.Semaphore.Dispose();
                _item = null;
            }
        }
    }
}
