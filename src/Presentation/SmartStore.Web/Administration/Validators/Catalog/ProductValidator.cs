using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
    public class ProductValidator : AbstractValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));

			// validate PAnGV
			When(x => x.BasePrice_Enabled, () =>
			{
				RuleFor(x => x.BasePrice_MeasureUnit).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceMeasureUnit.Required"));
				RuleFor(x => x.BasePrice_BaseAmount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"));
				RuleFor(x => x.BasePrice_Amount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceAmount.Required"));
			});
        }
    }
}