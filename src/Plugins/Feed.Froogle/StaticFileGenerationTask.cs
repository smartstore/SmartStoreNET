using SmartStore.Core.Infrastructure;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Feed.Froogle
{
    public class StaticFileGenerationTask : ITask
    {
		/// <summary>
		/// Execute task
		/// </summary>
		public void Execute()
		{
			var googleService = EngineContext.Current.Resolve<IGoogleService>();
			googleService.CreateFeed();
		}
    }
}