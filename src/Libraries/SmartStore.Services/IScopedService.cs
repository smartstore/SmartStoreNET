using System;

namespace SmartStore.Services
{
    public interface IScopedService
    {
        /// <summary>
        /// Creates a long running unit of work in which cache eviction is suppressed
        /// </summary>
        /// <param name="clearCache">Specifies whether the cache should be evicted completely on batch disposal</param>
        /// <returns>A disposable unit of work</returns>
        IDisposable BeginScope(bool clearCache = true);

        /// <summary>
        /// Gets a value indicating whether data has changed during a request, making cache eviction necessary.
        /// </summary>
        /// <remarks>
        /// Cache eviction sets this member to <c>false</c>
        /// </remarks>
        bool HasChanges { get; }

        void ClearCache();
    }
}
