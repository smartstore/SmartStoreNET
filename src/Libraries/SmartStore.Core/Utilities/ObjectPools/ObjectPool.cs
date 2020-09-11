using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SmartStore.Utilities.ObjectPools
{
    /// <summary>
    /// A pool of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool.</typeparam>
    /// <remarks>
    /// This implementation keeps a cache of retained objects. This means that if objects are returned
    /// when the pool has already reached "poolSize" objects they will be available to be Garbage Collected.
    /// </remarks>
    public class ObjectPool<T> : IDisposable where T : class, IPooledObject
    {
        private volatile bool _isDisposed;
        private readonly Func<T> _activator;

        // Storage for the pool objects. The first item is stored in a dedicated field because we
        // expect to be able to satisfy most requests from it.
        internal protected T _firstItem;
        internal protected readonly ObjectHolder[] _items;

        /// <summary>
        /// Creates an instance of <see cref="ObjectPool{T}"/>.
        /// </summary>
        /// <param name="activator">The factory that creates an instance of <typeparamref name="T"/></param>
        /// <param name="poolSize">The maximum number of objects to retain in the pool. Default: ProcessorCount * 4.</param>
        public ObjectPool(Func<T> activator = null, int? poolSize = null)
        {
            // -1 due to _firstItem
            _items = new ObjectHolder[(poolSize ?? Environment.ProcessorCount * 4) - 1];
            _activator = activator ?? (() => Activator.CreateInstance<T>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected virtual T Create()
        {
            if (_isDisposed)
            {
                throw Error.ObjectDisposed(GetType().Name);
            }

            return _activator();
        }

        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns>A <typeparamref name="T"/>.</returns>
        public virtual T Rent()
        {
            // PERF: Examine the first element. If that fails, RentSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. In a worst case we may miss some
            // recently returned objects. Not a big deal.
            var item = _firstItem;

            if (item == null || item != Interlocked.CompareExchange(ref _firstItem, null, item))
            {
                item = RentSlow();
            }

            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T RentSlow()
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i].Value;
                if (item != null && item == Interlocked.CompareExchange(ref items[i].Value, null, item))
                {
                    return item;
                }
            }

            return Create();
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        /// <returns>Whether free slots were available and the object has been returned to the pool.</returns>
        public virtual bool Return(T obj)
        {
            if (_isDisposed)
            {
                DisposeItem(obj);
                return false;
            }

            bool returned;
            if (_firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _firstItem = obj;
                returned = true;
            }
            else
            {
                returned = ReturnSlow(obj);
            }

            if (!returned)
            {
                DisposeItem(obj);
            }

            return returned;
        }

        private bool ReturnSlow(T obj)
        {
            var items = _items;
            for (var i = 0; i < items.Length; ++i)
            {
                if (items[i].Value == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i].Value = obj;
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            _isDisposed = true;

            DisposeItem(_firstItem);
            _firstItem = null;

            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                DisposeItem(items[i].Value);
                items[i].Value = null;
            }
        }

        private void DisposeItem(T item)
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // PERF: the struct wrapper avoids array-covariance-checks from the runtime when assigning to elements of the array.
        [DebuggerDisplay("{Value}")]
        internal protected struct ObjectHolder
        {
            public T Value;
        }
    }
}
