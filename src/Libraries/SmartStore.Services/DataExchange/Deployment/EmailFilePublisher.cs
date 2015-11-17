using System;
using System.IO;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Services.Messages;

namespace SmartStore.Services.DataExchange.Deployment
{
	public class EmailFilePublisher : IFilePublisher
	{
		private IEmailAccountService _emailAccountService;
		private IQueuedEmailService _queuedEmailService;

		public EmailFilePublisher(
			IEmailAccountService emailAccountService,
			IQueuedEmailService queuedEmailService)
		{
			_emailAccountService = emailAccountService;
			_queuedEmailService = queuedEmailService;
		}

		public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			var emailAccountService = EngineContext.Current.Resolve<IEmailAccountService>();
			var queuedEmailService = EngineContext.Current.Resolve<IQueuedEmailService>();

			var emailAccount = emailAccountService.GetEmailAccountById(deployment.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var count = 0;

			foreach (var email in deployment.EmailAddresses.SplitSafe(",").Where(x => x.IsEmail()))
			{
				var queuedEmail = new QueuedEmail
				{
					From = emailAccount.Email,
					FromName = emailAccount.DisplayName,
					To = email,
					Subject = deployment.EmailSubject.NaIfEmpty(),
					CreatedOnUtc = DateTime.UtcNow,
					EmailAccountId = deployment.EmailAccountId
				};

				foreach (string path in context.DeploymentFiles)
				{
					string name = Path.GetFileName(path);

					queuedEmail.Attachments.Add(new QueuedEmailAttachment
					{
						StorageLocation = EmailAttachmentStorageLocation.Path,
						Path = path,
						Name = name,
						MimeType = MimeTypes.MapNameToMimeType(name)
					});
				}

				queuedEmailService.InsertQueuedEmail(queuedEmail);
				++count;
			}

			context.Log.Information("{0} email(s) created and queued for deployment.".FormatInvariant(count));
		}
	}
}
