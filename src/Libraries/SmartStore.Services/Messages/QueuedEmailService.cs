using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Utilities;
using System.Web;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Messages
{
    public partial class QueuedEmailService : IQueuedEmailService
    {
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
		private readonly IRepository<QueuedEmailAttachment> _queuedEmailAttachmentRepository;
		private readonly IEmailSender _emailSender;
		private readonly ICommonServices _services;

        public QueuedEmailService(
			IRepository<QueuedEmail> queuedEmailRepository,
 			IRepository<QueuedEmailAttachment> queuedEmailAttachmentRepository,
			IEmailSender emailSender, 
			ICommonServices services)
        {
            this._queuedEmailRepository = queuedEmailRepository;
			this._queuedEmailAttachmentRepository = queuedEmailAttachmentRepository;
			this._emailSender = emailSender;
			this._services = services;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
        }

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }
     
        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Insert(queuedEmail);

            //event notification
            _services.EventPublisher.EntityInserted(queuedEmail);
        }

        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Update(queuedEmail);

            //event notification
			_services.EventPublisher.EntityUpdated(queuedEmail);
        }

        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            if (queuedEmail == null)
                throw new ArgumentNullException("queuedEmail");

            _queuedEmailRepository.Delete(queuedEmail);

            //event notification
			_services.EventPublisher.EntityDeleted(queuedEmail);
        }

		public virtual int DeleteAllQueuedEmails()
		{
			// do not delete e-mails which are about to be sent
			return _queuedEmailRepository.DeleteAll(x => x.SentOnUtc.HasValue || x.SentTries >= 3);
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

            var query = from qe in _queuedEmailRepository.Table
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

        public virtual IPagedList<QueuedEmail> SearchEmails(SearchEmailsQuery query)
        {
			Guard.ArgumentNotNull(() => query);
            
            var q = _queuedEmailRepository.Table;

			if (query.Expand.HasValue())
			{
				var expands = query.Expand.Split(',');
				foreach (var expand in expands)
				{
					q = q.Expand(expand.Trim());
				}
			}

            if (query.From.HasValue())
				q = q.Where(qe => qe.From.Contains(query.From.Trim()));

			if (query.To.HasValue())
                q = q.Where(qe => qe.To.Contains(query.To.Trim()));

            if (query.StartTime.HasValue)
                q = q.Where(qe => qe.CreatedOnUtc >= query.StartTime);

            if (query.EndTime.HasValue)
                q = q.Where(qe => qe.CreatedOnUtc <= query.EndTime);

            if (query.UnsentOnly)
                q = q.Where(qe => !qe.SentOnUtc.HasValue);

			if (query.SendManually.HasValue)
				q = q.Where(qe => qe.SendManually == query.SendManually.Value);

            q = q.Where(qe => qe.SentTries < query.MaxSendTries);
            
			q = q.OrderByDescending(qe => qe.Priority);

            q = query.OrderByLatest ? 
                ((IOrderedQueryable<QueuedEmail>)q).ThenByDescending(qe => qe.CreatedOnUtc) :
                ((IOrderedQueryable<QueuedEmail>)q).ThenBy(qe => qe.CreatedOnUtc);

            var queuedEmails = new PagedList<QueuedEmail>(q, query.PageIndex, query.PageSize);
            return queuedEmails;
        }

		public virtual bool SendEmail(QueuedEmail queuedEmail)
		{
			var result = false;

			try
			{
				var smtpContext = new SmtpContext(queuedEmail.EmailAccount);
				var msg = ConvertEmail(queuedEmail);

				_emailSender.SendEmail(smtpContext, msg);

				queuedEmail.SentOnUtc = DateTime.UtcNow;
				result = true;
			}
			catch (Exception exc)
			{
				Logger.Error(string.Concat(T("Admin.Common.ErrorSendingEmail"), ": ", exc.Message), exc);
			}
			finally
			{
				queuedEmail.SentTries = queuedEmail.SentTries + 1;
				UpdateQueuedEmail(queuedEmail);
			}

			return result;
		}

		private void AddEmailAddresses(string addresses, ICollection<EmailAddress> target)
		{
			var arr = addresses.IsEmpty() ? null : addresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (arr != null && arr.Length > 0)
			{
				target.AddRange(arr.Where(x => x.Trim().HasValue()).Select(x => new EmailAddress(x)));
			}
		}

		internal EmailMessage ConvertEmail(QueuedEmail qe)
		{
			// 'internal' for testing purposes

			var msg = new EmailMessage(
				new EmailAddress(qe.To, qe.ToName),
				qe.Subject,
				qe.Body,
				new EmailAddress(qe.From, qe.FromName));

			if (qe.ReplyTo.HasValue())
			{
				msg.ReplyTo.Add(new EmailAddress(qe.ReplyTo, qe.ReplyToName));
			}

			AddEmailAddresses(qe.CC, msg.Cc);
			AddEmailAddresses(qe.Bcc, msg.Bcc);

			if (qe.Attachments != null && qe.Attachments.Count > 0)
			{
				foreach (var qea in qe.Attachments)
				{
					Attachment attachment = null;

					if (qea.StorageLocation == EmailAttachmentStorageLocation.Blob)
					{
						var data = qea.Data;
						if (data != null && data.Length > 0)
						{
							attachment = new Attachment(data.ToStream(), qea.Name, qea.MimeType);
						}
					}
					else if (qea.StorageLocation == EmailAttachmentStorageLocation.Path)
					{
						var path = qea.Path;
						if (path.HasValue())
						{
							if (path[0] == '~' || path[0] == '/')
							{
								path = CommonHelper.MapPath(VirtualPathUtility.ToAppRelative(path), false);
							}
							if (File.Exists(path))
							{
								attachment = new Attachment(path, qea.MimeType);
								attachment.Name = qea.Name;
							}
						}
					}
					else if (qea.StorageLocation == EmailAttachmentStorageLocation.FileReference)
					{
						var file = qea.File;
						if (file != null && file.UseDownloadUrl == false && file.DownloadBinary != null && file.DownloadBinary.Length > 0)
						{
							attachment = new Attachment(file.DownloadBinary.ToStream(), file.Filename + file.Extension, file.ContentType);
						}
					}

					if (attachment != null)
					{
						msg.Attachments.Add(attachment);
					}
				}
			}

			return msg;
		}

		#region Attachments

		public virtual QueuedEmailAttachment GetQueuedEmailAttachmentById(int id)
		{
			if (id == 0)
				return null;

			var qea = _queuedEmailAttachmentRepository.GetById(id);
			return qea;
		}

		public virtual void DeleteQueuedEmailAttachment(QueuedEmailAttachment qea)
		{
			if (qea == null)
				throw new ArgumentNullException("qea");

			_queuedEmailAttachmentRepository.Delete(qea);

			_services.EventPublisher.EntityDeleted(qea);
		}

		#endregion
	}

	public class SearchEmailsQuery
	{
		public string From { get; set; }
		public string To { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool UnsentOnly { get; set; }
		public int MaxSendTries { get; set; }
		public bool OrderByLatest { get; set; }
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public bool? SendManually { get; set; }

		/// <summary>
		/// Navigation properties to eager load (comma separataed)
		/// </summary>
		public string Expand { get; set; }
	}

}
