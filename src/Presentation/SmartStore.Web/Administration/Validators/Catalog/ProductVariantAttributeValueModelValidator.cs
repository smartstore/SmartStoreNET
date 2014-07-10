using FluentValidation;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class ProductVariantAttributeValueModelValidator : AbstractValidator<ProductModel.ProductVariantAttributeValueModel>
    {
        public ProductVariantAttributeValueModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(
				localizationService.GetResource("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Required")
			);

			RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).WithMessage(
				localizationService.GetResource("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.GreaterOrEqualToOne")
			)
			.When(x => x.ValueTypeId == (int)ProductVariantAttributeValueType.ProductLinkage);
        }
    }
}