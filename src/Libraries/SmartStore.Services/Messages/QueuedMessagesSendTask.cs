using System.Threading.Tasks;
using System.Data.Entity;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class QueuedMessagesSendTask : AsyncTask
    {
        private readonly IQueuedEmailService _queuedEmailService;

        public QueuedMessagesSendTask(IQueuedEmailService queuedEmailService)
        {
            _queuedEmailService = queuedEmailService;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
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
                    UnsentOnly = true,
                    SendManually = false
                };

                var queuedEmails = await _queuedEmailService.SearchEmails(q).LoadAsync();

                foreach (var queuedEmail in queuedEmails)
                {
                    await _queuedEmailService.SendEmailAsync(queuedEmail);
                }

                if (!queuedEmails.HasNextPage)
                    break;
            }
        }
    }
}
