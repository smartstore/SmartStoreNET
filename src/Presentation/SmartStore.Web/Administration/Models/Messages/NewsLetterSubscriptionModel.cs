using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Messages;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(NewsLetterSubscriptionValidator))]
    public class NewsLetterSubscriptionModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store")]
		public string StoreName { get; set; }
    }
}