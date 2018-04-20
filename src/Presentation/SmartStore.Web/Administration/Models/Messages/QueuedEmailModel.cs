using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Messages;
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
			this.Attachments = new List<QueuedEmailAttachmentModel>();
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

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.CreatedOn")]
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
}