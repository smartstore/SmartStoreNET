using FluentValidation;
using SmartStore.Admin.Models.Polls;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Polls
{
	public partial class PollValidator : AbstractValidator<PollModel>
    {
        public PollValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Polls.Fields.Name.Required"));
        }
    }
}