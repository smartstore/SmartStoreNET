using System;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
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

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store")]
        public string StoreName { get; set; }
    }

    public partial class NewsLetterSubscriptionValidator : AbstractValidator<NewsLetterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}