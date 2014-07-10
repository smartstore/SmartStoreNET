using FluentValidation;
using SmartStore.Admin.Models.Forums;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Forums
{
	public partial class ForumValidator : AbstractValidator<ForumModel>
    {
        public ForumValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.Name.Required"));
            RuleFor(x => x.ForumGroupId).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required"));
        }
    }
}