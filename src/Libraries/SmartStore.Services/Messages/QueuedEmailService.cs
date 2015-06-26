using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Messages
{
    public partial class QueuedEmailService : IQueuedEmailService
    {
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
        private readonly IEventPublisher _eventPublisher;
		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;
		private readonly ILocalizationService _localizationService;

        public QueuedEmailService(
			IRepository<QueuedEmail> queuedEmailRepository,
			IEventPublisher eventPublisher,
			IEmailSender emailSender,
			ILogger logger,
			ILocalizationService localizationService)
        {
            _queuedEmailRepository = queuedEmailRepository;
            _eventPublisher = eventPublisher;
			_emailSender = emailSender;
			_logger = logger;
			_localizationService = localizationService;
        }
     
        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Insert(queuedEmail);

            //event notification
            _eventPublisher.EntityInserted(queuedEmail);
        }

        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Update(queuedEmail);

            //event notification
            _eventPublisher.EntityUpdated(queuedEmail);
        }

        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Delete(queuedEmail);

            //event notification
            _eventPublisher.EntityDeleted(queuedEmail);
        }

		public virtual int DeleteAllQueuedEmails()
		{
			return _queuedEmailRepository.DeleteAll();
		}

        public virtual QueuedEmail GetQueuedEmailById(int queuedEmailId)
        {
            if (queuedEmailId == 0)
                return null;

            var queuedEmail = _queuedEmailRepository.GetById(queuedEmailId);
            return queuedEmail;

        }

        public virtual IList<QueuedEmail> GetQueuedEmailsByIds(int[] queuedEmailIds)
        {
            if (queuedEmailIds == null || queuedEmailIds.Length == 0)
                return new List<QueuedEmail>();

            var query = from qe in _queuedEmailRepository.Table.Expand(x => x.EmailAccount)
                        where queuedEmailIds.Contains(qe.Id)
                        select qe;

            var queuedEmails = query.ToList();

            // sort by passed identifiers
            var sortedQueuedEmails = new List<QueuedEmail>();

            foreach (int id in queuedEmailIds)
            {
                var queuedEmail = queuedEmails.Find(x => x.Id == id);
                if (queuedEmail != null)
                    sortedQueuedEmails.Add(queuedEmail);
            }
            return sortedQueuedEmails;
        }

        public virtual IPagedList<QueuedEmail> SearchEmails(string fromEmail, 
            string toEmail, DateTime? startTime, DateTime? endTime, 
            bool loadUnsentItemsOnly, int maxSendTries,
            bool loadNewest, int pageIndex, int pageSize,
			bool? sendManually = null)
        {
            fromEmail = (fromEmail ?? String.Empty).Trim();
            toEmail = (toEmail ?? String.Empty).Trim();
            
            var query = _queuedEmailRepository.Table.Expand(x => x.EmailAccount);

            if (!String.IsNullOrEmpty(fromEmail))
                query = query.Where(qe => qe.From.Contains(fromEmail));

            if (!String.IsNullOrEmpty(toEmail))
                query = query.Where(qe => qe.To.Contains(toEmail));

            if (startTime.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc >= startTime);

            if (endTime.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc <= endTime);

            if (loadUnsentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

			if (sendManually.HasValue)
				query = query.Where(qe => qe.SendManually == sendManually.Value);

            query = query.Where(qe => qe.SentTries < maxSendTries);
            
			query = query.OrderByDescending(qe => qe.Priority);

            query = loadNewest ? 
                ((IOrderedQueryable<QueuedEmail>)query).ThenByDescending(qe => qe.CreatedOnUtc) :
                ((IOrderedQueryable<QueuedEmail>)query).ThenBy(qe => qe.CreatedOnUtc);

            var queuedEmails = new PagedList<QueuedEmail>(query, pageIndex, pageSize);
            return queuedEmails;
        }

		public virtual bool SendEmail(QueuedEmail queuedEmail)
		{
			var result = false;

			try
			{
				var bcc = String.IsNullOrWhiteSpace(queuedEmail.Bcc) ? null : queuedEmail.Bcc.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				var cc = String.IsNullOrWhiteSpace(queuedEmail.CC) ? null : queuedEmail.CC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

				var smtpContext = new SmtpContext(queuedEmail.EmailAccount);

				var msg = new EmailMessage(
					new EmailAddress(queuedEmail.To, queuedEmail.ToName),
					queuedEmail.Subject,
					queuedEmail.Body,
					new EmailAddress(queuedEmail.From, queuedEmail.FromName));

				if (queuedEmail.ReplyTo.HasValue())
				{
					msg.ReplyTo.Add(new EmailAddress(queuedEmail.ReplyTo, queuedEmail.ReplyToName));
				}

				if (cc != null)
				{
					msg.Cc.AddRange(cc.Where(x => x.HasValue()).Select(x => new EmailAddress(x)));
				}

				if (bcc != null)
				{
					msg.Bcc.AddRange(bcc.Where(x => x.HasValue()).Select(x => new EmailAddress(x)));
				}

				_emailSender.SendEmail(smtpContext, msg);

				queuedEmail.SentOnUtc = DateTime.UtcNow;
				result = true;
			}
			catch (Exception exc)
			{
				_logger.Error(string.Concat(_localizationService.GetResource("Admin.Common.ErrorSendingEmail"), ": ", exc.Message), exc);
			}
			finally
			{
				queuedEmail.SentTries = queuedEmail.SentTries + 1;
				UpdateQueuedEmail(queuedEmail);
			}
			return result;
		}
    }
}
