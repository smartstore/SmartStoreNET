using System;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a task for deleting sent emails from the message queue.
    /// </summary>
    public partial class QueuedMessagesClearTask : AsyncTask
    {
        private readonly IRepository<QueuedEmail> _qeRepository;
        private readonly CommonSettings _commonSettings;

        public QueuedMessagesClearTask(
            IRepository<QueuedEmail> qeRepository,
            CommonSettings commonSettings)
        {
            _qeRepository = qeRepository;
            _commonSettings = commonSettings;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
            var olderThan = DateTime.UtcNow.AddDays(-Math.Abs(_commonSettings.MaxQueuedMessagesAgeInDays));
            await _qeRepository.DeleteAllAsync(x => x.SentOnUtc.HasValue && x.CreatedOnUtc < olderThan);

            _qeRepository.Context.ShrinkDatabase();
        }
    }
}
