using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Utilities;

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
            _queuedEmailRepository = queuedEmailRepository;
            _queuedEmailAttachmentRepository = queuedEmailAttachmentRepository;
            _emailSender = emailSender;
            _services = services;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;


        public virtual void InsertQueuedEmail(QueuedEmail queuedEmail)
        {
            Guard.NotNull(queuedEmail, nameof(queuedEmail));

            _queuedEmailRepository.Insert(queuedEmail);
        }

        public virtual void UpdateQueuedEmail(QueuedEmail queuedEmail)
        {
            Guard.NotNull(queuedEmail, nameof(queuedEmail));

            _queuedEmailRepository.Update(queuedEmail);
        }

        public virtual void DeleteQueuedEmail(QueuedEmail queuedEmail)
        {
            Guard.NotNull(queuedEmail, nameof(queuedEmail));

            _queuedEmailRepository.Delete(queuedEmail);
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

            // sort by passed identifier sequence
            return queuedEmails.OrderBySequence(queuedEmailIds).ToList();
        }

        public virtual IPagedList<QueuedEmail> SearchEmails(SearchEmailsQuery query)
        {
            Guard.NotNull(query, nameof(query));

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

        public virtual async Task<bool> SendEmailAsync(QueuedEmail queuedEmail)
        {
            var result = false;

            try
            {
                var smtpContext = new SmtpContext(queuedEmail.EmailAccount);
                var msg = ConvertEmail(queuedEmail);

                await _emailSender.SendEmailAsync(smtpContext, msg);

                queuedEmail.SentOnUtc = DateTime.UtcNow;
                result = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, string.Concat(T("Admin.Common.ErrorSendingEmail"), ": ", ex.Message));
            }
            finally
            {
                queuedEmail.SentTries += 1;
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
                new EmailAddress(qe.To),
                qe.Subject.Replace("\r\n", string.Empty),
                qe.Body,
                new EmailAddress(qe.From));

            if (qe.ReplyTo.HasValue())
            {
                msg.ReplyTo.Add(new EmailAddress(qe.ReplyTo));
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
                        var data = qea.MediaStorage?.Data;

                        if (data != null && data.LongLength > 0)
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
                                attachment = new Attachment(path, qea.MimeType) { Name = qea.Name };
                            }
                        }
                    }
                    else if (qea.StorageLocation == EmailAttachmentStorageLocation.FileReference)
                    {
                        var file = qea.MediaFile;
                        if (file != null)
                        {
                            var mediaFile = _services.MediaService.ConvertMediaFile(file);
                            var stream = mediaFile.OpenRead();
                            if (stream != null)
                            {
                                attachment = new Attachment(stream, file.Name, file.MimeType);
                            }
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

        public virtual void DeleteQueuedEmailAttachment(QueuedEmailAttachment attachment)
        {
            Guard.NotNull(attachment, nameof(attachment));

            _queuedEmailAttachmentRepository.Delete(attachment);
        }

        public virtual byte[] LoadQueuedEmailAttachmentBinary(QueuedEmailAttachment attachment)
        {
            Guard.NotNull(attachment, nameof(attachment));

            if (attachment.StorageLocation == EmailAttachmentStorageLocation.Blob)
            {
                return attachment.MediaStorage?.Data ?? new byte[0];
            }

            return null;
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
