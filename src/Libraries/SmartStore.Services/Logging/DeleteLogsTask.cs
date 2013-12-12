using System;
using System.Linq;
using SmartStore.Core.Domain.Logging;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Logging
{
    /// <summary>
    /// Represents a task for deleting log entries
    /// </summary>
    public partial class DeleteLogsTask : ITask
    {
        private readonly ILogger _logger;

        public DeleteLogsTask(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var olderThanDays = 7; // TODO: move to settings
            var toUtc = DateTime.UtcNow.AddDays(-olderThanDays);
            // do not delete error and fatal logs
            var logsToDelete = _logger.GetAllLogs(null, toUtc, null, null, 0, int.MaxValue, 1).Where(x => x.LogLevel < LogLevel.Error);

            foreach (var log in logsToDelete)
            {
                _logger.DeleteLog(log);
            }
        }
    }
}
