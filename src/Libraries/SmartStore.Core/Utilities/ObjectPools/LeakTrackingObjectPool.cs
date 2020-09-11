using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SmartStore.Utilities.ObjectPools
{
    public class LeakTrackingObjectPool<T> : ObjectPool<T> where T : class, IPooledObject
    {
        private readonly ConditionalWeakTable<T, Tracker> _trackers = new ConditionalWeakTable<T, Tracker>();
        private readonly ObjectPool<T> _inner;

        public LeakTrackingObjectPool(ObjectPool<T> inner)
        {
            Guard.NotNull(inner, nameof(inner));

            _inner = inner;
        }

        public override T Rent()
        {
            var value = _inner.Rent();
            _trackers.Add(value, new Tracker());
            return value;
        }

        public override bool Return(T obj)
        {
            if (_trackers.TryGetValue(obj, out var tracker))
            {
                _trackers.Remove(obj);
                tracker.Dispose();
            }

            return _inner.Return(obj);
        }

        private class Tracker : IDisposable
        {
            private readonly string _stack;
            private bool _disposed;

            public Tracker()
            {
                _stack = Environment.StackTrace;
            }

            public void Dispose()
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }

            ~Tracker()
            {
                if (!_disposed && !Environment.HasShutdownStarted)
                {
                    Debug.Fail($"{typeof(T).Name} was leaked. Created at: {Environment.NewLine}{_stack}");
                }
            }
        }
    }
}
