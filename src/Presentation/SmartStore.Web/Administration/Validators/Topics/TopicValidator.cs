using FluentValidation;
using SmartStore.Admin.Models.Topics;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Topics
{
	public partial class TopicValidator : AbstractValidator<TopicModel>
    {
        public TopicValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.SystemName).NotNull().WithMessage(localizationService.GetResource("Admin.ContentManagement.Topics.Fields.SystemName.Required"));
        }
    }
}