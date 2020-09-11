using System.Collections.Generic;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    public interface ITaskExecutor
    {
        Task ExecuteAsync(
            ScheduleTask entity,
            IDictionary<string, string> taskParameters = null,
            bool throwOnError = false);
    }
}
