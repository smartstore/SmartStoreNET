using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Catalog
{
    [Validator(typeof(ProductAskQuestionValidator))]
    public partial class ProductAskQuestionModel : EntityModelBase
    {
        public LocalizedValue<string> ProductName { get; set; }

        public string ProductSeName { get; set; }

        [SmartResourceDisplayName("Account.Fields.Email")]
        public string SenderEmail { get; set; }

        [SmartResourceDisplayName("Account.Fields.FullName")]
        public string SenderName { get; set; }
        public bool SenderNameRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.Phone")]
        public string SenderPhone { get; set; }

        [SanitizeHtml]
        [SmartResourceDisplayName("Common.Question")]
        public string Question { get; set; }

        public string SelectedAttributes { get; set; }
        public string ProductUrl { get; set; }
        public bool IsQuoteRequest { get; set; }

        public bool DisplayCaptcha { get; set; }
    }

    public class ProductAskQuestionValidator : AbstractValidator<ProductAskQuestionModel>
    {
        public ProductAskQuestionValidator(PrivacySettings privacySettings)
        {
            RuleFor(x => x.SenderEmail).NotEmpty();
            RuleFor(x => x.SenderEmail).EmailAddress();
            RuleFor(x => x.Question).NotEmpty();

            if (privacySettings.FullNameOnProductRequestRequired)
            {
                RuleFor(x => x.SenderName).NotEmpty();
            }
        }
    }
}