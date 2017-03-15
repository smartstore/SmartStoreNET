﻿using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FluentValidation.Attributes;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Messages;
using SmartStore.Collections;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(MessageTemplateValidator))]
    public class MessageTemplateModel : EntityModelBase, ILocalizedModel<MessageTemplateLocalizedModel>
    {
        public MessageTemplateModel()
        {
            Locales = new List<MessageTemplateLocalizedModel>();
            AvailableEmailAccounts = new List<EmailAccountModel>();
			AvailableStores = new List<StoreModel>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.AllowedTokens")]
        [ScriptIgnore, JsonIgnore]
        public TreeNode<string> TokensTree { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

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
        [AllowHtml]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.EmailAccount")]
        public int EmailAccountId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.SendManually")]
		public bool SendManually { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment1FileId")]
		public int? Attachment1FileId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId")]
		public int? Attachment2FileId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId")]
		public int? Attachment3FileId { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        public IList<MessageTemplateLocalizedModel> Locales { get; set; }
        public IList<EmailAccountModel> AvailableEmailAccounts { get; set; }
    }

    public class MessageTemplateLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

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
		public int? Attachment1FileId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment2FileId")]
		public int? Attachment2FileId { get; set; }

		[SmartResourceDisplayName("Admin.ContentManagement.MessageTemplates.Fields.Attachment3FileId")]
		public int? Attachment3FileId { get; set; }
    }
}