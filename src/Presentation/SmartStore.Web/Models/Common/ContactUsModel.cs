using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Common
{
    [Validator(typeof(ContactUsValidator))]
    public partial class ContactUsModel : ModelBase
    {
        [SmartResourceDisplayName("ContactUs.Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [SanitizeHtml]
        [SmartResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [SmartResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }
        public bool FullNameRequired { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }

    public class ContactUsValidator : AbstractValidator<ContactUsModel>
    {
        public ContactUsValidator(PrivacySettings privacySettings)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Enquiry).NotEmpty();

            if (privacySettings.FullNameOnContactUsRequired)
            {
                RuleFor(x => x.FullName).NotEmpty();
            }
        }
    }
}