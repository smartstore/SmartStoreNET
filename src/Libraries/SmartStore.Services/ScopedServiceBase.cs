using System;
using SmartStore.Utilities;

namespace SmartStore.Services
{
    public abstract class ScopedServiceBase : IScopedService
    {
        /// <summary>
        /// Creates a long running unit of work in which cache eviction is suppressed
        /// </summary>
        /// <param name="clearCache">Specifies whether the cache should be evicted completely on batch disposal</param>
        /// <returns>A disposable unit of work</returns>
        public IDisposable BeginScope(bool clearCache = true)
        {
            if (IsInScope)
            {
                // nested batches are not supported
                return ActionDisposable.Empty;
            }

            OnBeginScope();
            IsInScope = true;

            return new ActionDisposable(() =>
            {
                IsInScope = false;
                OnEndScope();
                if (clearCache && HasChanges)
                {
                    ClearCache();
                }
            });
        }

        /// <summary>
        /// Gets a value indicating whether data has changed during a request, making cache eviction necessary.
        /// </summary>
        /// <remarks>
        /// Cache eviction sets this member to <c>false</c>
        /// </remarks>
        public bool HasChanges
        {
            get;
            protected set;
        }

        protected bool IsInScope
        {
            get;
            private set;
        }

        public void ClearCache()
        {
            if (!IsInScope)
            {
                OnClearCache();
                HasChanges = false;
            }
        }

        protected abstract void OnClearCache();

        protected virtual void OnBeginScope() { }
        protected virtual void OnEndScope() { }
    }
}
