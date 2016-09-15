using System;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Logging;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Logging
{
    /// <summary>
    /// Represents a task for deleting log entries
    /// </summary>
    public partial class DeleteLogsTask : ITask
    {
        private readonly ILogService _logService;

        public DeleteLogsTask(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
		public void Execute(TaskExecutionContext ctx)
        {
            var olderThanDays = 7; // TODO: move to settings
            var toUtc = DateTime.UtcNow.AddDays(-olderThanDays);

			_logService.ClearLog(toUtc, LogLevel.Error);
        }
    }
}
