using System;

namespace SmartStore.Core.Caching
{
    public interface ICacheScopeAccessor
    {
        /// <summary>
        /// Returns the current cache acquire scope (for current thread)
        /// </summary>
        CacheScope Current { get; }

        /// <summary>
        /// Whether any scope with the passed <paramref name="key"/> exists already. Used to prevent lock recursions.
        /// </summary>
        /// <param name="key">Key</param>
        bool HasScope(string key);

        /// <summary>
        /// Notifies the current scope about a cache dependency.
        /// </summary>
        /// <param name="key"></param>
        void PropagateKey(string key);

        /// <summary>
        /// Begins a new cache acquire scope.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="independent">
        /// <c>true</c> means that no key propagation will be performed, making the current cache entry independent from its parent.
        /// Meaning: parent cache entries will not be auto-invalidated when this entry changes.
        /// </param>
        IDisposable BeginScope(string key, bool independent = false);
    }
}
