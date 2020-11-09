using System;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Logging;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Logging
{
    /// <summary>
    /// Represents a task for deleting log entries.
    /// </summary>
    public partial class DeleteLogsTask : ITask
    {
        private readonly ILogService _logService;
        private readonly CommonSettings _commonSettings;

        public DeleteLogsTask(
            ILogService logService,
            CommonSettings commonSettings)
        {
            _logService = logService;
            _commonSettings = commonSettings;
        }

        public void Execute(TaskExecutionContext ctx)
        {
            var toUtc = DateTime.UtcNow.AddDays(-_commonSettings.MaxLogAgeInDays);

            _logService.ClearLog(toUtc, LogLevel.Error);
        }
    }
}
