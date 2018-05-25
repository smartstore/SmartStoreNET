using FluentValidation;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Localization;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Validators.Catalog
{
    public class ProductAskQuestionValidator : AbstractValidator<ProductAskQuestionModel>
    {
        public ProductAskQuestionValidator(ILocalizationService localizationService, PrivacySettings privacySettings)
        {
            RuleFor(x => x.SenderEmail).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.Email.Required"));
            RuleFor(x => x.SenderEmail).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(x => x.Question).NotEmpty().WithMessage(localizationService.GetResource("Products.AskQuestion.Question.Required"));

			if (privacySettings.FullNameOnProductRequestRequired)
			{
				RuleFor(x => x.SenderName).NotEmpty().WithMessage(localizationService.GetResource("Account.Fields.FullName.Required"));
			}
		}
	}
}