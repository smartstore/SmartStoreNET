using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Messages;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(QueuedEmailValidator))]
    public class QueuedEmailModel : EntityModelBase
    {
        public QueuedEmailModel()
        {
            Attachments = new List<QueuedEmailAttachmentModel>();
        }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Id")]
        public override int Id { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Priority")]
        public int Priority { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.From")]
        [AllowHtml]
        public string From { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.To")]
        [AllowHtml]
        public string To { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.CC")]
        [AllowHtml]
        public string CC { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Bcc")]
        [AllowHtml]
        public string Bcc { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.SentTries")]
        public int SentTries { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.SentOn")]
        //[DisplayFormat(DataFormatString = "{0}", NullDisplayText = "n/a")]
        public DateTime? SentOn { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.EmailAccountName")]
        [AllowHtml]
        public string EmailAccountName { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.SendManually")]
        public bool SendManually { get; set; }

        public int AttachmentsCount { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.Attachments")]
        public ICollection<QueuedEmailAttachmentModel> Attachments { get; set; }

        public class QueuedEmailAttachmentModel : EntityModelBase
        {
            public string Name { get; set; }
            public string MimeType { get; set; }
        }
    }

    public partial class QueuedEmailValidator : AbstractValidator<QueuedEmailModel>
    {
        public QueuedEmailValidator()
        {
            RuleFor(x => x.Priority).InclusiveBetween(0, 99999);
            RuleFor(x => x.From).NotEmpty();
            RuleFor(x => x.To).NotEmpty();
            RuleFor(x => x.SentTries).InclusiveBetween(0, 99999);
        }
    }

    public class QueuedEmailMapper :
        IMapper<QueuedEmail, QueuedEmailModel>
    {
        public void Map(QueuedEmail from, QueuedEmailModel to)
        {
            MiniMapper.Map(from, to);
            to.EmailAccountName = from.EmailAccount?.FriendlyName ?? string.Empty;
            to.AttachmentsCount = from.Attachments?.Count ?? 0;
            to.Attachments = from.Attachments
                .Select(x => new QueuedEmailModel.QueuedEmailAttachmentModel { Id = x.Id, Name = x.Name, MimeType = x.MimeType })
                .ToList();
        }
    }
}