using FluentValidation;
using SmartStore.Admin.Models.News;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.News
{
	public partial class NewsItemValidator : AbstractValidator<NewsItemModel>
    {
        public NewsItemValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Fields.Title.Required"));

            RuleFor(x => x.Short)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Fields.Short.Required"));

            RuleFor(x => x.Full)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Fields.Full.Required"));
        }
    }
}