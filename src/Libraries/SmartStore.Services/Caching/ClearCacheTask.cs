using System;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Caching
{
    /// <summary>
    /// Clear cache scheduled task implementation
    /// </summary>
    public partial class ClearCacheTask : ITask
    {
        private readonly ICacheManager _cacheManager;

        public ClearCacheTask(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute(TaskExecutionContext ctx)
        {
            _cacheManager.Clear();
        }
    }
}
