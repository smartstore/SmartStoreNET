using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    public interface ITaskExecutor
    {
        void Execute(
			ScheduleTask task, 
			IDictionary<string, string> taskParameters = null, 
			bool throwOnError = false);
    }
}
