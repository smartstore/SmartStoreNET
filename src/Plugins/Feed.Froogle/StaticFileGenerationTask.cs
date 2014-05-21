using SmartStore.Core.Infrastructure;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Feed.Froogle
{
    public class StaticFileGenerationTask : ITask
    {
		private readonly IGoogleService _googService;

		public StaticFileGenerationTask(IGoogleService googService)
		{
			_googService = googService;
		}
		
		/// <summary>
		/// Execute task
		/// </summary>
		public void Execute()
		{
			_googService.CreateFeed();
		}
    }
}