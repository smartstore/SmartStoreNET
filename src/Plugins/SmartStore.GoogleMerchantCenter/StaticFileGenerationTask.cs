using Autofac;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.GoogleMerchantCenter
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