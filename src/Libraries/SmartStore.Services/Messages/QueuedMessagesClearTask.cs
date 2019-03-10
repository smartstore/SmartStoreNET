using System;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a task for deleting sent emails from the message queue.
    /// </summary>
    public partial class QueuedMessagesClearTask : ITask
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

		public void Execute(TaskExecutionContext ctx)
        {
			var olderThan = DateTime.UtcNow.AddDays(-Math.Abs(_commonSettings.MaxQueuedMessagesAgeInDays));
			_qeRepository.DeleteAll(x => x.SentOnUtc.HasValue && x.CreatedOnUtc < olderThan);

			_qeRepository.Context.ShrinkDatabase();
        }
    }
}
