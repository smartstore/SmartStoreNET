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
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var cacheManager = EngineContext.Current.Resolve<ICacheManager>("static");
            cacheManager.Clear();
        }
    }
}
