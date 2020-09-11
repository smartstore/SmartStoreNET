using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FluentValidation;
using FluentValidation.Attributes;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(MessageTemplateValidator))]
    public class MessageTemplateModel : TabbableModel, ILocalizedModel<MessageTemplateLocalizedModel>
    {
        public MessageTemplateModel()
        {
            Locales = new List<MessageTemplateLocalizedModel>();
            AvailableEmailAccounts = new List<EmailAccountModel>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.AllowedTokens")]
        [ScriptIgnore, JsonIgnore]
        public TreeNode<string> TokensTree { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.To")]
        [AllowHtml]
        public string To { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.ReplyTo")]
        [AllowHtml]
        public string ReplyTo { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.AllowedTokens")]
        [ScriptIgnore, JsonIgnore]
        public string LastModelTree { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.BccEmailAddresses")]
        [AllowHtml]
        public string BccEmailAddresses { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Common.Active")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.EmailAccount")]
        public int EmailAccountId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.SendManually")]
        public bool SendManually { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment1FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment1FileId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment2FileId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment3FileId { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        public IList<MessageTemplateLocalizedModel> Locales { get; set; }
        public IList<EmailAccountModel> AvailableEmailAccounts { get; set; }
    }

    public class MessageTemplateLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.To")]
        [AllowHtml]
        public string To { get; set; }

        [SmartResourceDisplayName("Admin.System.QueuedEmails.Fields.ReplyTo")]
        [AllowHtml]
        public string ReplyTo { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.BccEmailAddresses")]
        [AllowHtml]
        public string BccEmailAddresses { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.EmailAccount")]
        public int EmailAccountId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment1FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment1FileId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment2FileId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int? Attachment3FileId { get; set; }
    }

    public partial class MessageTemplateValidator : AbstractValidator<MessageTemplateModel>
    {
        public MessageTemplateValidator()
        {
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Body).NotEmpty();
        }
    }
}