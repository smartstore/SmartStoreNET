using FluentValidation;
using SmartStore.Admin.Models.Polls;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Polls
{
	public partial class PollAnswerValidator : AbstractValidator<PollAnswerModel>
    {
        public PollAnswerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.Polls.Answers.Fields.Name.Required"));
        }
    }
}