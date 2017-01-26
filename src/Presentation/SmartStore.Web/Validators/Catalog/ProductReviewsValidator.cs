using FluentValidation;
using SmartStore.Services.Localization;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Validators.Catalog
{
    public class ProductReviewsValidator : AbstractValidator<ProductReviewsModel>
    {
        public ProductReviewsValidator(ILocalizationService localizationService)
        {
			RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.Title.Required"));
            RuleFor(x => x.Title).Length(1, 200).WithMessage(string.Format(localizationService.GetResource("Reviews.Fields.Title.MaxLengthValidation"), 200)).When(x => !string.IsNullOrEmpty(x.Title));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Reviews.Fields.ReviewText.Required"));
        }
    }
}