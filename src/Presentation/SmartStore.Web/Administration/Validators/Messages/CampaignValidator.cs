using FluentValidation;
using SmartStore.Admin.Models.Messages;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Messages
{
	public partial class CampaignValidator : AbstractValidator<CampaignModel>
    {
        public CampaignValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Promotions.Campaigns.Fields.Name.Required"));

            RuleFor(x => x.Subject)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Promotions.Campaigns.Fields.Subject.Required"));

            RuleFor(x => x.Body)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Promotions.Campaigns.Fields.Body.Required"));
        }
    }
}