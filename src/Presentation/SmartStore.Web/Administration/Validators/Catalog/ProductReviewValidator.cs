using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class ProductReviewValidator : AbstractValidator<ProductReviewModel>
    {
        public ProductReviewValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.Title.Required"));
            RuleFor(x => x.ReviewText).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductReviews.Fields.ReviewText.Required"));
        }}
}