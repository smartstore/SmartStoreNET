using Autofac;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.Plugin.Feed.Froogle
{
    public class StaticFileGenerationTask : ITask
    {	
		public void Execute(TaskExecutionContext context)
		{
			var scope = context.LifetimeScope as ILifetimeScope;
			var googleService = scope.Resolve<IGoogleFeedService>();
			
			googleService.CreateFeed(context);
		}
    }
}