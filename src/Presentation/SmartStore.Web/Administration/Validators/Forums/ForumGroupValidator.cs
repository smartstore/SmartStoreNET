using FluentValidation;
using SmartStore.Admin.Models.Forums;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Forums
{
	public partial class ForumGroupValidator : AbstractValidator<ForumGroupModel>
    {
        public ForumGroupValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required"));
        }
    }
}