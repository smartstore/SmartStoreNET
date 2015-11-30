using SmartStore.Services.Tasks;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class QueuedMessagesSendTask : ITask
    {
        private readonly IQueuedEmailService _queuedEmailService;

        public QueuedMessagesSendTask(IQueuedEmailService queuedEmailService)
        {
            _queuedEmailService = queuedEmailService;
        }

		public void Execute(TaskExecutionContext ctx)
        {
			const int pageSize = 1000;
			const int maxTries = 3;

			for (int i = 0; i < 9999999; ++i)
			{
				var q = new SearchEmailsQuery
				{
					MaxSendTries = maxTries,
					PageIndex = i,
					PageSize = pageSize,
					Expand = "Attachments",
					UnsentOnly = true
				};
				var queuedEmails = _queuedEmailService.SearchEmails(q);

				foreach (var queuedEmail in queuedEmails)
				{
					_queuedEmailService.SendEmail(queuedEmail);
				}

				if (!queuedEmails.HasNextPage)
					break;
			}
        }
    }
}
