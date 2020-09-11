using SmartStore.AmazonPay.Services;
using SmartStore.Services.Tasks;

namespace SmartStore.AmazonPay
{
    public class DataPollingTask : ITask
    {
        private readonly IAmazonPayService _apiService;

        public DataPollingTask(IAmazonPayService apiService)
        {
            _apiService = apiService;
        }

        public void Execute(TaskExecutionContext ctx)
        {
            _apiService.StartDataPolling();
        }
    }
}