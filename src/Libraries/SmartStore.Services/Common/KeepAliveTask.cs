using System;
using System.Net;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : ITask
    {

		public void Execute(TaskExecutionContext ctx)
        {
            // do absolutely nothing
        }
    }
}
