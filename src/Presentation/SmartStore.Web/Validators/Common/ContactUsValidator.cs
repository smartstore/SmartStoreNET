using FluentValidation;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Localization;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Validators.Common
{
    public class ContactUsValidator : AbstractValidator<ContactUsModel>
    {
        public ContactUsValidator(ILocalizationService localizationService, PrivacySettings privacySettings)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Enquiry).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.Enquiry.Required"));

			if (privacySettings.FullNameOnContactUsRequired)
			{
				RuleFor(x => x.FullName).NotEmpty().WithMessage(localizationService.GetResource("ContactUs.FullName.Required"));
			}
		}
	}
}