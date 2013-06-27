using SmartStore.Core.Infrastructure;
using SmartStore.Plugin.Feed.ElmarShopinfo.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Feed.ElmarShopinfo
{
    public class StaticFileGenerationTask : ITask
    {
        /// <summary>
        /// Execute task
        /// </summary>
        public void Execute()
        {
			var elmarShopinfoService = EngineContext.Current.Resolve<IElmarShopinfoCoreService>();
			elmarShopinfoService.CreateFeed(true);
        }
    }
}