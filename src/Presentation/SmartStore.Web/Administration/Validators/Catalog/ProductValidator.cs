using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;
using System;
using System.Linq;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class ProductValidator : AbstractValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));

            When(x => x.LoadedTabs != null && x.LoadedTabs.Contains("Inventory", StringComparer.OrdinalIgnoreCase), () => {
                RuleFor(x => x.OrderMinimumQuantity).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Validation.ValueGreaterZero"));
                RuleFor(x => x.OrderMaximumQuantity).GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Validation.ValueGreaterZero"));
            });
            
            // validate PAnGV
            When(x => x.BasePriceEnabled && x.LoadedTabs != null && x.LoadedTabs.Contains("Price"), () =>
			{
				RuleFor(x => x.BasePriceMeasureUnit).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceMeasureUnit.Required"));
				RuleFor(x => x.BasePriceBaseAmount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"));
				RuleFor(x => x.BasePriceAmount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceAmount.Required"));
			});
        }
    }
}