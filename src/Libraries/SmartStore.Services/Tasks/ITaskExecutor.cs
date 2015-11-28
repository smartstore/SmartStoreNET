using System;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    public interface ITaskExecutor
    {
        void Execute(ScheduleTask task, bool throwOnError = false);
    }
}
