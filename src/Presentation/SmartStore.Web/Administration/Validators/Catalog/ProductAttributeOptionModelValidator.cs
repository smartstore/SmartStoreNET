using FluentValidation;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class ProductAttributeOptionModelValidator : AbstractValidator<ProductAttributeOptionModel>
    {
        public ProductAttributeOptionModelValidator(ILocalizationService localize)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(
				localize.GetResource("Admin.Validation.RequiredField").FormatInvariant(localize.GetResource("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name"))
			);

			RuleFor(x => x.Quantity).GreaterThan(0)
				.When(x => x.ValueTypeId == (int)ProductVariantAttributeValueType.ProductLinkage)
				.WithMessage(localize.GetResource("Admin.Validation.ValueGreaterZero"));
        }
    }
}