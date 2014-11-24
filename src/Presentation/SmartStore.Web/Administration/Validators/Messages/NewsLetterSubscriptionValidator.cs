using FluentValidation;
using SmartStore.Admin.Models.Messages;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Messages
{
	public partial class NewsLetterSubscriptionValidator : AbstractValidator<NewsLetterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotNull().WithMessage(localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
        }
    }
}