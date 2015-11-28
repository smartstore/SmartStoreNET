using Autofac;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.GoogleMerchantCenter
{
    public class StaticFileGenerationTask : ITask
    {
        private readonly IGoogleFeedService _feedService;
        
        public StaticFileGenerationTask(IGoogleFeedService feedService)
        {
            this._feedService = feedService;
        }
        
        public void Execute(TaskExecutionContext context)
		{
            _feedService.CreateFeed(context);
			context.CancellationToken.ThrowIfCancellationRequested();
		}
    }
}