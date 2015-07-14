using System;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Data;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a task for deleting sent emails from the message queue
    /// </summary>
    public partial class QueuedMessagesClearTask : ITask
    {
        private readonly IRepository<QueuedEmail> _qeRepository;

		public QueuedMessagesClearTask(IRepository<QueuedEmail> qeRepository)
        {
			this._qeRepository = qeRepository;
        }

		public void Execute(TaskExecutionContext ctx)
        {
			var olderThan = DateTime.UtcNow.AddDays(-14);
			_qeRepository.DeleteAll(x => x.SentOnUtc.HasValue && x.CreatedOnUtc < olderThan);

			_qeRepository.Context.ShrinkDatabase();
        }
    }
}
