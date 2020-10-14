using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.Caching
{
    public partial class MemoryCacheManager : DisposableObject, ICacheManager
    {
        const string LockRecursionExceptionMessage = "Acquiring identical cache items recursively is not supported. Key: {0}";

        // We put a special string into cache if value is null,
        // otherwise our 'Contains()' would always return false,
        // which is bad if we intentionally wanted to save NULL values.
        public const string FakeNull = "__[NULL]__";

        private readonly Work<ICacheScopeAccessor> _scopeAccessor;
        private MemoryCache _cache;

        public MemoryCacheManager(Work<ICacheScopeAccessor> scopeAccessor)
        {
            _scopeAccessor = scopeAccessor;
            _cache = CreateCache();
        }

        private MemoryCache CreateCache()
        {
            return new MemoryCache("SmartStore");
        }

        public bool IsDistributedCache => false;

        private bool TryGet<T>(string key, bool independent, out T value)
        {
            value = default(T);

            object obj = _cache.Get(key);

            if (obj != null)
            {
                // Make the parent scope's entry depend on this
                if (!independent)
                {
                    _scopeAccessor.Value.PropagateKey(key);
                }

                if (obj.Equals(FakeNull))
                {
                    return true;
                }

                value = (T)obj;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(string key, bool independent = false)
        {
            TryGet(key, independent, out T value);
            return value;
        }

        public T Get<T>(string key, Func<T> acquirer, TimeSpan? duration = null, bool independent = false, bool allowRecursion = false)
        {
            if (TryGet(key, independent, out T value))
            {
                return value;
            }

            if (!allowRecursion && _scopeAccessor.Value.HasScope(key))
            {
                throw new LockRecursionException(LockRecursionExceptionMessage.FormatInvariant(key));
            }

            // Get the (semaphore) locker specific to this key
            using (KeyedLock.Lock("cache:" + key, TimeSpan.FromSeconds(5)))
            {
                // Atomic operation must be outer locked
                if (!TryGet(key, independent, out value))
                {
                    var scope = !allowRecursion ? _scopeAccessor.Value.BeginScope(key) : ActionDisposable.Empty;
                    using (scope)
                    {
                        value = acquirer();
                        var dependencies = !allowRecursion ? _scopeAccessor.Value.Current?.Dependencies : (IEnumerable<string>)null;
                        Put(key, value, duration, dependencies);
                        return value;
                    }
                }
            }

            return value;
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquirer, TimeSpan? duration = null, bool independent = false, bool allowRecursion = false)
        {
            if (TryGet(key, independent, out T value))
            {
                return value;
            }

            if (!allowRecursion && _scopeAccessor.Value.HasScope(key))
            {
                throw new LockRecursionException(LockRecursionExceptionMessage.FormatInvariant(key));
            }

            // Get the async (semaphore) locker specific to this key
            using (await KeyedLock.LockAsync("cache:" + key, TimeSpan.FromMinutes(1)))
            {
                if (!TryGet(key, independent, out value))
                {
                    var scope = !allowRecursion ? _scopeAccessor.Value.BeginScope(key) : ActionDisposable.Empty;
                    using (scope)
                    {
                        value = await acquirer();
                        var dependencies = !allowRecursion ? _scopeAccessor.Value.Current?.Dependencies : (IEnumerable<string>)null;
                        Put(key, value, duration, dependencies);
                        return value;
                    }
                }
            }

            return value;
        }

        public void Put(string key, object value, TimeSpan? duration = null, IEnumerable<string> dependencies = null)
        {
            _cache.Set(key, value ?? FakeNull, GetCacheItemPolicy(duration, dependencies));
        }

        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public IEnumerable<string> Keys(string pattern)
        {
            Guard.NotEmpty(pattern, nameof(pattern));

            var keys = _cache.AsParallel().Select(x => x.Key);

            if (pattern.IsEmpty() || pattern == "*")
            {
                return keys.ToArray();
            }

            var wildcard = new Wildcard(pattern, RegexOptions.IgnoreCase);
            return keys.Where(x => wildcard.IsMatch(x)).ToArray();
        }

        public int RemoveByPattern(string pattern)
        {
            lock (_cache)
            {
                var keysToRemove = Keys(pattern);
                int count = 0;

                // lock atomic operation
                foreach (string key in keysToRemove)
                {
                    _cache.Remove(key);
                    count++;
                }

                return count;
            }
        }

        public void Clear()
        {
            // Faster way of clearing cache: https://stackoverflow.com/questions/8043381/how-do-i-clear-a-system-runtime-caching-memorycache
            var oldCache = Interlocked.Exchange(ref _cache, CreateCache());
            oldCache.Dispose();
            GC.Collect();
        }

        public virtual ISet GetHashSet(string key, Func<IEnumerable<string>> acquirer = null)
        {
            var result = Get(key, () =>
            {
                var set = new MemorySet(this);
                var items = acquirer?.Invoke();
                if (items != null)
                {
                    set.AddRange(items);
                }

                return set;
            });

            return result;
        }

        private CacheItemPolicy GetCacheItemPolicy(TimeSpan? duration, IEnumerable<string> dependencies)
        {
            var absoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;

            if (duration.HasValue)
            {
                absoluteExpiration = DateTime.UtcNow + duration.Value;
            }

            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = ObjectCache.NoSlidingExpiration
            };

            if (dependencies != null && dependencies.Any())
            {
                // INFO: we can only depend on existing items, otherwise this entry will be removed immediately.
                dependencies = dependencies.Where(x => x != null && _cache.Contains(x));
                if (dependencies.Any())
                {
                    cacheItemPolicy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(dependencies));
                }
            }

            //cacheItemPolicy.RemovedCallback = OnRemoveEntry;

            return cacheItemPolicy;
        }

        //private void OnRemoveEntry(CacheEntryRemovedArguments args)
        //{
        //	if (args.RemovedReason == CacheEntryRemovedReason.ChangeMonitorChanged)
        //	{
        //		Debug.WriteLine("MEMCACHE: remove depending entry '{0}'.".FormatInvariant(args.CacheItem.Key));
        //	}
        //}

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
                _cache.Dispose();
        }
    }
}